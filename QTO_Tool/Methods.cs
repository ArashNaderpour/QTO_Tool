using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

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

        static void setOwner(IntPtr ownerPtr, Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerPtr;
        }

        //Concrete model preparations
        public static string ConcreteModelSetup()
        {
            string examinationResult = "";
            int invalidObjCount = 0;
            int badGeometryCount = 0;

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();

            foreach (RhinoObject obj in RunQTO.doc.Objects)
            {
                if (obj.IsValid)
                {
                    int blockLevel = 0;

                    Methods.PrepareObject(obj, surfaceList, invalidObjCount, blockLevel);
                }

                else
                {
                    invalidObjCount++;
                }

                RunQTO.doc.Objects.Delete(obj);
            }

            Brep[] newBreps = Brep.JoinBreps(surfaceList, 0.01);

            if (newBreps != null)
            {
                foreach (Brep newBrep in newBreps)
                {
                    newBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    RunQTO.doc.Objects.AddBrep(newBrep);
                }
            }

            foreach (RhinoObject obj in RunQTO.doc.Objects)
            {
                var mass_properties = VolumeMassProperties.Compute((Brep)obj.Geometry);
                double volume_error_percentage = Math.Round((mass_properties.VolumeError / mass_properties.Volume) * 100, 3);
               
                if (((Brep)obj.Geometry).IsSolid & volume_error_percentage <= 1)
                {
                    continue;
                }
                else
                {
                    badGeometryCount = Methods.BadGeometryDetected(obj, badGeometryCount);
                }
            }

            examinationResult = invalidObjCount.ToString() + " invalid objects exist in the model. \n";
            examinationResult += badGeometryCount.ToString() + " bad geometry objects exist in the model.";

            RunQTO.doc.Views.Redraw();

            return examinationResult;
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
        static void PrepareBlockInstance(RhinoObject inputObj, List<Brep> _surfaceList, int _badGeometryCount, int _blockLevel)
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
                subObj.Attributes.LayerIndex = inputObj.Attributes.LayerIndex;
                
                Methods.PrepareObject(subObj, _surfaceList, _badGeometryCount, _blockLevel);
            }

            Brep[] newBreps = Brep.JoinBreps(surfaceList, 0.01);

            if (newBreps != null)
            {
                foreach (Brep newBrep in newBreps)
                {
                    newBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    RunQTO.doc.Objects.AddBrep(newBrep);
                }
            }
        }

        //Prepare Brep or extrusion
        static void PrepareMesh(RhinoObject inputObj, List<Brep> _surfaceList)
        {
            Brep tempBrep = Brep.CreateFromMesh(((Mesh)inputObj.Geometry), true);

            if (tempBrep.IsSurface)
            {
                _surfaceList.Add(tempBrep);
            }

            else
            {
                tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                RunQTO.doc.Objects.Add(tempBrep, inputObj.Attributes);
            }
        }

        static void PrepareObject(RhinoObject inputObj, List<Brep> _surfaceList, int _badGeometryCount, int _blockLevel)
        {
            inputObj.Attributes.ObjectColor = System.Drawing.Color.Black;
            inputObj.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
            inputObj.CommitChanges();

            string objType = inputObj.GetType().ToString().Split('.').Last<string>();

            if (objType == "BrepObject")
            {
                Brep tempBrep = (Brep)inputObj.Geometry;

                if (tempBrep.IsSurface)
                {
                    _surfaceList.Add(tempBrep);
                }

                else
                {
                    tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    RunQTO.doc.Objects.Add(tempBrep, inputObj.Attributes);
                }
            }

            else if (objType == "ExtrusionObject")
            {
                Brep tempBrep = Brep.TryConvertBrep(inputObj.Geometry);

                if (tempBrep.IsSurface)
                {
                    _surfaceList.Add(tempBrep);
                }

                else
                {
                    tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                    RunQTO.doc.Objects.Add(tempBrep, inputObj.Attributes);
                }
            }

            else if (objType == "MeshObject")
            {
                Methods.PrepareMesh(inputObj, _surfaceList);
            }

            else if (objType == "InstanceObject")
            {
                Methods.PrepareBlockInstance(inputObj, _surfaceList, _badGeometryCount, _blockLevel);
            }
        }

        static int BadGeometryDetected(RhinoObject inputObj, int _badGeometryCount)
        {
            inputObj.Attributes.ObjectColor = System.Drawing.Color.Red;
            inputObj.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
            inputObj.CommitChanges();
            _badGeometryCount++;

            return _badGeometryCount;
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
    }
}
