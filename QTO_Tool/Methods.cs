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
        public static void ConcreteModelSetup()
        {
            string objType = "";
            string examinationResult = "";
            int objCount = new int();
            int invalidObjCount = 0;

            foreach (Rhino.DocObjects.RhinoObject obj in RunQTO.doc.Objects)
            {
                if (obj.IsValid)
                {
                    objType = obj.GetType().ToString().Split('.').Last<string>();

                    if (objType == "InstanceObject")
                    {
                        PrepareBlockInstance(obj, invalidObjCount);
                    }

                    if (objType == "BrepObject" || objType == "ExtrusionObject")
                    {

                    }

                    if (objType == "MeshObject")
                    {
                        PrepareMesh(obj, objType, invalidObjCount);
                    }
                }

                else
                {
                    invalidObjCount++;
                }
            }
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
        static void PrepareBlockInstance(Rhino.DocObjects.RhinoObject inputObj, int _invalidObjCount)
        {
            Rhino.DocObjects.InstanceObject instanceObj = (Rhino.DocObjects.InstanceObject)inputObj;

            Rhino.DocObjects.RhinoObject[] geometryPieces = { };
            Rhino.DocObjects.ObjectAttributes[] objAtts = { };
            Rhino.Geometry.Transform[] objTransform = { };

            List<Mesh> meshList = new List<Mesh>();
            List<Brep> surfaceList = new List<Brep>();


            instanceObj.Explode(true, out geometryPieces, out objAtts, out objTransform);

            string inputObjType = "";

            for (int i = 0; i < geometryPieces.Length; i++)
            {
                if (geometryPieces[i].IsValid)
                {

                    inputObjType = geometryPieces[i].GetType().ToString().Split('.').Last<string>();

                    if (inputObjType == "BrepObject")
                    {
                        //PrepareBrepOrExtrusion(geometryPieces[i], objTransform[i], objAtts[i], inputObjType, surfaceList, _invalidObjCount);
                        Brep tempBrep = (Brep)geometryPieces[i].Geometry;

                        if (tempBrep.IsSolid)
                        {
                            tempBrep.Transform(objTransform[i]);
                            RunQTO.doc.Objects.Add(tempBrep, objAtts[i]);
                        }

                        if (tempBrep.IsSurface)
                        {
                            surfaceList.Add(tempBrep);
                        }
                    }

                    if (inputObjType == "ExtrusionObject")
                    {
                        Brep tempBrep = Brep.TryConvertBrep(geometryPieces[i].Geometry);

                        if (tempBrep.IsSolid)
                        {
                            tempBrep.Transform(objTransform[i]);
                            RunQTO.doc.Objects.Add(tempBrep, objAtts[i]);
                        }

                        if (tempBrep.IsSurface)
                        {
                            _invalidObjCount++;
                        }
                    }

                    if (inputObjType == "MeshObject")
                    {
                        PrepareMesh(geometryPieces[i], inputObjType, _invalidObjCount);
                    }
                }

                else
                {
                    _invalidObjCount++;
                }
            }

            Brep[] newBreps = Brep.JoinBreps(surfaceList, 0.01);

            if (newBreps != null)
            {
                foreach (Brep b in newBreps) {
                    Rhino.Geometry.Transform t = instanceObj.InstanceXform;
                    b.Transform(t);
                    RunQTO.doc.Objects.AddBrep(b);
                }
            }
            
            RunQTO.doc.Objects.Delete(inputObj);
            RunQTO.doc.Views.Redraw();
        }

        //Prepare Brep or extrusion
        static void PrepareMesh(Rhino.DocObjects.RhinoObject inputObj, string inputObjType, int _invalidObjCount)
        {
            MessageBox.Show(inputObjType);
        }
    }
}
