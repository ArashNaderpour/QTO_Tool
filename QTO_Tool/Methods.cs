﻿using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Reflection;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace QTO_Tool
{
    class Methods
    {
        public static Random random = new Random();

        internal static void SetChildStatus(QTOUI mw, ChildStatus winChildStatus)
        {
            switch (winChildStatus)
            {
                //case childStatus.ChildOfGH:
                //    setOwner(Grasshopper.Instances.DocumentEditor, mw);
                //    break;
                case ChildStatus.AlwaysOnTop:
                    mw.Topmost = true;
                    break;
                case ChildStatus.ChildOfRhino:
                    setOwner(RhinoApp.MainWindowHandle(), mw);
                    break;
                default:
                    break;
            }
        }

        //Utility functions to set the ownership of a window object
        static void setOwner(System.Windows.Forms.Form ownerForm, Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerForm.Handle;
        }

        public static double CalculateAngleThreshold(double angleThresholdSlider)
        {
            double result = 1;

            Vector2d baseVector = new Vector2d(1, 0);

            Vector2d rotatedVector = new Vector2d(1, 0);

            rotatedVector.Rotate(angleThresholdSlider * (Math.PI / 180));

            result = (baseVector.X * rotatedVector.X) + (baseVector.Y * rotatedVector.Y);

            return result;
        }

        static void setOwner(IntPtr ownerPtr, Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerPtr;
        }

        //Concrete model preparations
        public static string ConcreteModelSetup()
        {
            string modelUnitSystem = "Model's current unit system is: " + RunQTO.doc.GetUnitSystemName(true, true, true, true);
            string modelAngleTolerance = "Model's current angle tolerance is: " + RunQTO.doc.ModelAngleToleranceDegrees.ToString();
            string modelAbsoluteTolerance = "Model's current unit system is: " + RunQTO.doc.ModelAbsoluteTolerance.ToString();

            string examinationResult = "";
            int invalidObjCount = 0;
            int badGeometryCount = 0;

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();

            Dictionary<Brep, string> newBreps = new Dictionary<Brep, string>();
            List<ObjectAttributes> newObjectAttributes = new List<ObjectAttributes>();

            foreach (RhinoObject obj in RunQTO.doc.Objects)
            {
                if (obj.IsValid)
                {
                    int blockLevel = 0;

                    ObjectAttributes mainObjectAttributes = obj.Attributes;

                    Methods.PrepareObject(obj, mainObjectAttributes, surfaceList, invalidObjCount, blockLevel);
                }

                else
                {
                    invalidObjCount++;
                }

                Brep[] tempBreps = Brep.JoinBreps(surfaceList, RunQTO.doc.ModelAbsoluteTolerance);

                if (tempBreps != null)
                {
                    if (tempBreps.Length == 1 && tempBreps[0].IsSolid)
                    {
                        newBreps.Add(tempBreps[0], "Good");
                        newObjectAttributes.Add(obj.Attributes);
                    }

                    else
                    {
                        for (int i = 0; i < tempBreps.Length; i++)
                        {
                            if (tempBreps[i].IsSolid)
                            {
                                newBreps.Add(tempBreps[i], "Good");
                                newObjectAttributes.Add(obj.Attributes);
                            }

                            else
                            {
                                newBreps.Add(tempBreps[i], "Bad");
                                newObjectAttributes.Add(obj.Attributes);
                            }
                        }
                    }
                }

                surfaceList.Clear();
                RunQTO.doc.Objects.Delete(obj);
            }

            if (newBreps.Count != 0)
            {
                foreach (Brep newBrep in newBreps.Keys)
                {
                    newBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    var mass_properties = VolumeMassProperties.Compute(newBrep);
                    double volume_error_percentage = Math.Round((mass_properties.VolumeError / mass_properties.Volume) * 100, 3);

                    if (volume_error_percentage <= 1 && newBreps[newBrep] == "Good")
                    {
                        RunQTO.doc.Objects.AddBrep(newBrep, newObjectAttributes[newBreps.Keys.ToList().IndexOf(newBrep)]);
                    }
                    else
                    {
                        badGeometryCount = Methods.BadGeometryDetected(newBrep, newObjectAttributes[newBreps.Keys.ToList().IndexOf(newBrep)], badGeometryCount);
                    }
                }
            }

            examinationResult = invalidObjCount.ToString() + " invalid objects exist in the model. \n";
            examinationResult += badGeometryCount.ToString() + " bad geometry objects exist in the model.";

            RunQTO.doc.Views.Redraw();

            return String.Join(Environment.NewLine, examinationResult, modelUnitSystem, modelAngleTolerance, modelAbsoluteTolerance);
        }

        //Concrete model preparations
        static void ExteriorModelExamination()
        {

        }

        //Concrete model preparations
        static void ConcreteModelArrangements()
        {

        }

        //Prepare BlockInstance
        static void PrepareBlockInstance(RhinoObject inputObj, ObjectAttributes _mainObjectAttributes, List<Brep> _surfaceList, int _badGeometryCount, int _blockLevel)
        {
            InstanceObject instanceObj = (InstanceObject)inputObj;

            RhinoObject[] geometryPieces = { };
            ObjectAttributes[] objAtts = { };
            Rhino.Geometry.Transform[] objTransform = { };

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();

            RhinoObject[] subObjs = instanceObj.GetSubObjects();

            instanceObj.Explode(true, out geometryPieces, out objAtts, out objTransform);

            foreach (RhinoObject subObj in subObjs)
            {
                Methods.PrepareObject(subObj, _mainObjectAttributes, _surfaceList, _badGeometryCount, _blockLevel);
            }
        }

        //Prepare Brep or extrusion
        static void PrepareMesh(RhinoObject inputObj, ObjectAttributes _mainObjectAttributes, List<Brep> _surfaceList)
        {

            Brep tempBrep = Brep.CreateFromMesh(((Mesh)inputObj.Geometry), true);

            if (tempBrep.Faces.Count == 1)
            {
                _surfaceList.Add(tempBrep);
            }

            else
            {
                tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                if (tempBrep.IsSolid)
                {
                    RunQTO.doc.Objects.Add(tempBrep, _mainObjectAttributes);
                }

                else
                {
                    _surfaceList.Add(tempBrep);
                }
            }
        }

        static void PrepareObject(RhinoObject inputObj, ObjectAttributes _mainObjectAttributes, List<Brep> _surfaceList, int _badGeometryCount, int _blockLevel)
        {
            _mainObjectAttributes.ObjectColor = System.Drawing.Color.Black;
            _mainObjectAttributes.ColorSource = ObjectColorSource.ColorFromObject;

            string objType = inputObj.GetType().ToString().Split('.').Last<string>();

            if (objType == "BrepObject")
            {
                Brep tempBrep = (Brep)inputObj.Geometry;

                if (tempBrep.Faces.Count == 1)
                {
                    _surfaceList.Add(tempBrep);
                }

                else
                {
                    tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);

                    if (tempBrep.IsSolid)
                    {
                        if (tempBrep.IsValid)
                        {
                            RunQTO.doc.Objects.Add(tempBrep, _mainObjectAttributes);
                        }
                        else
                        {
                            tempBrep = (Brep)inputObj.Geometry;

                            RunQTO.doc.Objects.Add(tempBrep, _mainObjectAttributes);
                        }
                    }

                    else
                    {
                        _surfaceList.Add(tempBrep);
                    }
                }
            }

            else if (objType == "ExtrusionObject")
            {
                Brep tempBrep = Brep.TryConvertBrep(inputObj.Geometry);

                if (tempBrep.Faces.Count == 1)
                {
                    _surfaceList.Add(tempBrep);
                }

                else
                {
                    tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    if (tempBrep.IsSolid)
                    {
                        RunQTO.doc.Objects.Add(tempBrep, _mainObjectAttributes);
                    }

                    else
                    {
                        _surfaceList.Add(tempBrep);
                    }
                }
            }

            else if (objType == "MeshObject")
            {
                Methods.PrepareMesh(inputObj, _mainObjectAttributes, _surfaceList);
            }

            else if (objType == "InstanceObject")
            {
                Methods.PrepareBlockInstance(inputObj, _mainObjectAttributes, _surfaceList, _badGeometryCount, _blockLevel);
            }
        }

        static int BadGeometryDetected(Brep brep, ObjectAttributes attributes, int _badGeometryCount)
        {
            _badGeometryCount++;

            attributes.ObjectColor = System.Drawing.Color.Red;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;

            RunQTO.doc.Objects.AddBrep(brep, attributes);

            return _badGeometryCount;
        }

        public static void HighlightBadGeometry(RhinoObject rhobj)
        {
            if (rhobj != null)
            {
                ObjectAttributes newObjectAttributes = rhobj.Attributes;
                newObjectAttributes.ObjectColor = System.Drawing.Color.Red;
                newObjectAttributes.ColorSource = ObjectColorSource.ColorFromObject;

                RunQTO.doc.Objects.ModifyAttributes(rhobj, newObjectAttributes, false);
            }
        }

        public static UIElement GetByUid(DependencyObject rootElement, string uid)
        {
            foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
            {
                if (element.Uid == uid)
                {
                    return element;
                }

                UIElement resultChildren = GetByUid(element, uid);

                if (resultChildren != null)
                {
                    return resultChildren;
                }
            }
            return null;
        }

        public static void CloseWindowUsingIdentifier(string windowName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string name;

            foreach (Window w in Application.Current.Windows)
            {

                try
                {
                    name = w.Name;
                }
                catch
                {
                    name = "";
                }

                if (name == windowName)
                {
                    w.Close();
                    break;
                }
            }
        }

        public static int AutomaticTemplateSelect(string layerName, List<string> concreteTemplateNames)
        {
            int result = 0;

            for (int i = 0; i < concreteTemplateNames.Count; i++)
            {
                if (layerName.ToLower().Split('_')[0].Contains(concreteTemplateNames[i].ToLower()))
                {
                    if (layerName.ToLower().Contains("continuous") == false)
                    {
                        result = i;
                    }
                }

                if (layerName.ToLower().Contains("continuous") == true)
                {
                    result = concreteTemplateNames.IndexOf("Continuous Footing");
                }
            }

            return result;
        }

        public static string FindFloor(Dictionary<double, string> floorElevations, double targetValue)
        {
            List<double> elevations = floorElevations.Keys.ToList();

            double closestValue = elevations[0];
            double minDifference = Math.Abs(elevations[0] - targetValue);

            for (int i = 1; i < elevations.Count; i++)
            {
                double difference = Math.Abs(elevations[i] - targetValue);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestValue = elevations[i];
                }
            }

            return floorElevations[closestValue];
        }

        public static void SaveDictionaryToDocumentStrings(Dictionary<double, string> data)
        {
            // Serialize the dictionary to a JSON string
            string jsonString = JsonConvert.SerializeObject(data);

            // Store the JSON string in RhinoDoc.Strings
            RunQTO.doc.Strings.SetString("FloorElevations", jsonString);

            // Save changes to the Rhino document
            RunQTO.doc.Modified = true;
        }

        public static Dictionary<double, string> RetrieveDictionaryFromDocumentStrings()
        {
            // Get the active Rhino document
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            // Retrieve the JSON string from RhinoDoc.Strings
            string jsonString = doc.Strings.GetValue("FloorElevations");

            if (!string.IsNullOrEmpty(jsonString))
            {
                // Deserialize the JSON string back to a dictionary
                return JsonConvert.DeserializeObject<Dictionary<double, string>>(jsonString);
            }

            // Return an empty dictionary if the JSON string is not found
            return new Dictionary<double, string>();
        }

        public static double SetVolumeConversionFactor(string modelUnit)
        {
            double result;

            if (modelUnit == "ft")
            {
                result = 0.037037;
            }

            else if (modelUnit == "in")
            {
                result = 2.14335e-5;
            }
            else
            {
                result = 1;
            }

            return result;
        }

        public static void Blockify()
        {
            int objectIndex = 0;

            foreach (RhinoObject obj in RunQTO.doc.Objects)
            {
                if (!(obj is InstanceObject))
                {
                    ObjectAttributes mainObjectAttributes = obj.Attributes;
                    Layer layer = RunQTO.doc.Layers.FindIndex(mainObjectAttributes.LayerIndex);

                    mainObjectAttributes.ColorSource = ObjectColorSource.ColorFromLayer;
                    mainObjectAttributes.ObjectColor = layer.Color;

                    string blockObjectName = LayerParentsPath(layer) + layer.Name + "_" + objectIndex.ToString();

                    // Duplicate the original geometry
                    GeometryBase geom = obj.Geometry.Duplicate();

                    // Calculate the center of the geometry's bounding box
                    BoundingBox bbox = geom.GetBoundingBox(true);
                    Point3d bboxCenter = bbox.Center;

                    // Create a block definition using the bounding box center as the base point
                    int blockDefIndex = RunQTO.doc.InstanceDefinitions.Add(blockObjectName, "Block containing one object", bboxCenter, new List<GeometryBase> { geom }, new List<ObjectAttributes> { mainObjectAttributes });

                    // Place the block instance at the original location
                    if (blockDefIndex != -1) // Check if the block was created successfully
                    {
                        // Calculate the transformation to move the block instance back to its original position
                        Transform placeBack = Transform.Translation(bboxCenter - Point3d.Origin);
                        RunQTO.doc.Objects.AddInstanceObject(blockDefIndex, placeBack, mainObjectAttributes);
                    }

                    // Delete the original object
                    RunQTO.doc.Objects.Delete(obj, true);

                    objectIndex++;
                }
            }

            RunQTO.doc.Views.Redraw();
        }

        public static double CalculateGrossVolume(Brep brep)
        {
            List<BrepLoop> brepInnerLoopsToRemove = new List<BrepLoop>();
            List<ComponentIndex> brepInnerLoopsToRemoveIndices = new List<ComponentIndex>();

            foreach (BrepLoop loop in brep.Loops)
            {
                if (loop.LoopType == BrepLoopType.Inner)
                {

                    Curve innerLoopCurve = loop.To3dCurve();

                    Brep innerLoopBrep = Brep.CreatePlanarBreps(innerLoopCurve, RunQTO.doc.ModelAbsoluteTolerance)[0];
                    Surface innerLoopSurface = innerLoopBrep.Surfaces[0];

                    Point3d centroid = innerLoopSurface.PointAt(
                        innerLoopSurface.Domain(0).Min + (innerLoopSurface.Domain(0).Max - innerLoopSurface.Domain(0).Min) * 0.02,
                        innerLoopSurface.Domain(1).Min + (innerLoopSurface.Domain(1).Max - innerLoopSurface.Domain(1).Min) * 0.02);

                    Vector3d normal = innerLoopSurface.NormalAt(innerLoopSurface.Domain(0).Mid, innerLoopSurface.Domain(1).Mid);
                    normal.Unitize();

                    // Create a line representing the ray for the intersection test
                    Line normalLine_1 = new Line(centroid, normal * 1000);
                    Line normalLine_2 = new Line(centroid, normal * -1000);

                    LineCurve normalCurve = new List<LineCurve> { new LineCurve(normalLine_1), new LineCurve(normalLine_2) }[0];

                    Curve[] overlapCurves;
                    Point3d[] intersectionPoints;
                    bool intersect = Rhino.Geometry.Intersect.Intersection.CurveBrep(normalCurve, brep, RunQTO.doc.ModelAbsoluteTolerance, out overlapCurves, out intersectionPoints);

                    if (intersect && intersectionPoints.Length == 0)
                    {
                        // If there is an intersection, mark this inner loop for removal
                        brepInnerLoopsToRemove.Add(loop);
                        brepInnerLoopsToRemoveIndices.Add(loop.ComponentIndex());
                    }
                }
            }

            if (brepInnerLoopsToRemoveIndices.Count > 0)
            {
                Brep newBrep = brep.RemoveHoles(brepInnerLoopsToRemoveIndices, RunQTO.doc.ModelAbsoluteTolerance);

                return newBrep.GetVolume();
            }
            else {
                return brep.GetVolume();
            }
        }

        private static string LayerParentsPath(Layer layer)
        {
            Guid parentLayerId = layer.ParentLayerId;
            if (parentLayerId == Guid.Empty)
            {
                return ""; // Base case: no parent
            }

            Layer parentLayer = RhinoDoc.ActiveDoc.Layers.FindId(parentLayerId);
            if (parentLayer == null)
            {
                return "";
            }

            string layerParentsPath = LayerParentsPath(parentLayer);
            return string.IsNullOrEmpty(layerParentsPath)
                ? parentLayer.Name + "_"
                : layerParentsPath + parentLayer.Name + "_";
        }
    }
}
