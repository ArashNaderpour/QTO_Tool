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
            string objType = "";
            string examinationResult = "";
            int objCount = new int();
            int invalidObjCount = 0;
            int badGeometryCount = 0;

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();

            foreach (Rhino.DocObjects.RhinoObject obj in RunQTO.doc.Objects)
            {
                if (obj.IsValid)
                {
                    objType = obj.GetType().ToString().Split('.').Last<string>();

                    if (objType == "InstanceObject")
                    {
                        PrepareBlockInstance(obj, surfaceList, invalidObjCount);
                    }

                    else if (objType == "ExtrusionObject")
                    {
                        Brep tempBrep = Brep.TryConvertBrep(obj.Geometry);

                        if (tempBrep.IsSolid)
                        {
                            tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                            RunQTO.doc.Objects.Add(tempBrep, obj.Attributes);
                        }

                        else
                        {
                            surfaceList.Add(tempBrep);
                        }
                    }

                    else if (objType == "BrepObject")
                    {
                        Brep tempBrep = (Brep)obj.Geometry;

                        if (tempBrep.IsSolid)
                        {
                            tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                            RunQTO.doc.Objects.Add(tempBrep, obj.Attributes);
                        }

                        else
                        {
                            surfaceList.Add(tempBrep);
                        }
                    }

                    else if (objType == "MeshObject")
                    {
                        PrepareMesh(obj, surfaceList, invalidObjCount);
                    }
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

            RunQTO.doc.Views.Redraw();

            examinationResult = invalidObjCount.ToString() + " invalid objects exist in the model. \n";
            examinationResult += badGeometryCount.ToString() + " bad geometry objects exist in the model.";

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
        static void PrepareBlockInstance(Rhino.DocObjects.RhinoObject inputObj, List<Brep> _surfaceList, int _invalidObjCount)
        {
            Rhino.DocObjects.InstanceObject instanceObj = (Rhino.DocObjects.InstanceObject)inputObj;

            Rhino.DocObjects.RhinoObject[] geometryPieces = { };
            Rhino.DocObjects.ObjectAttributes[] objAtts = { };
            Rhino.Geometry.Transform[] objTransform = { };

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();

            Rhino.DocObjects.RhinoObject[] subObjs = instanceObj.GetSubObjects();

            instanceObj.Explode(true, out geometryPieces, out objAtts, out objTransform);

            foreach (Rhino.DocObjects.RhinoObject subObj in subObjs)
            {
                string objType = subObj.GetType().ToString().Split('.').Last<string>();

                if (objType == "BrepObject")
                {
                    Brep tempBrep = (Brep)subObj.Geometry;

                    if (tempBrep.IsSolid)
                    {
                        tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);
                        RunQTO.doc.Objects.Add(tempBrep, subObj.Attributes);
                    }

                    else
                    {
                        surfaceList.Add(tempBrep);
                    }
                }

                else if (objType == "ExtrusionObject")
                {
                    Brep tempBrep = Brep.TryConvertBrep(subObj.Geometry);

                    if (tempBrep.IsSolid)
                    {
                        tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);
                        RunQTO.doc.Objects.Add(tempBrep, subObj.Attributes);
                    }

                    else
                    {
                        surfaceList.Add(tempBrep);
                    }
                }

                else if (objType == "MeshObject")
                {
                    Methods.PrepareMesh(subObj, _surfaceList, _invalidObjCount);
                }

                else if (objType == "InstanceObject")
                {
                    PrepareBlockInstance(subObj, _surfaceList, _invalidObjCount);
                }

                else
                {
                    _invalidObjCount++;
                }
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
        static void PrepareMesh(Rhino.DocObjects.RhinoObject inputObj, List<Brep> _surfaceList, int _invalidObjCount)
        {
            Brep tempBrep = Brep.CreateFromMesh(((Mesh)inputObj.Geometry), true);

            if (((Mesh)inputObj.Geometry).IsClosed)
            {
                tempBrep.MergeCoplanarFaces(RunQTO.doc.ModelAngleToleranceRadians);

                RunQTO.doc.Objects.AddBrep(tempBrep);
            }

            else
            {
                _surfaceList.Add(tempBrep);
            }
        }
    }
}
