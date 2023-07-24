using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Interop;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using System.Reflection;

namespace QTO_Tool
{
    class Methods
    {
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

                RunQTO.doc.Views.Redraw();
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
    }
}
