using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTO_Tool
{
    class CurbTemplate
    {
        public string nameAbb { get; set; }
        public string id { get; set; }

        public string layerName { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();

        public double grossVolume = double.MaxValue;
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double endArea { get; set; }
        public double sideArea_1 { get; set; }
        public double sideArea_2 { get; set; }
        public double length { get; set; }
        public double openingArea { get; set; }

        public Brep geometry { get; set; }

        public string type = "CurbTemplate";

        private List<double> upfacingFaceElevations = new List<double>();
        private List<double> upfacingFaceAreas = new List<double>();
        private List<Brep> upfacingFaces = new List<Brep>();
        private List<Point3d> upfacingFacesCenters = new List<Point3d>();
        private List<Vector3d> upfacingFacesNormals = new List<Vector3d>();

        private List<Brep> topFaces = new List<Brep>();

        private List<double> downfacingFaceElevations = new List<double>();
        private List<double> downfacingFaceAreas = new List<double>();
        private List<Brep> downfacingFaces = new List<Brep>();
        private List<Point3d> downfacingFacesCenters = new List<Point3d>();
        private List<Vector3d> downfacingFacesNormals = new List<Vector3d>();

        private List<Brep> bottomFaces = new List<Brep>();

        private List<Brep> sideAndEndFaces = new List<Brep>();
        private List<double> sideAndEndFaceAreas = new List<double>();
        private List<Curve> sideEdges = new List<Curve>();
        private List<double> endFaceAreas = new List<double>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Cubic Yard", "Square Foot", "Square Foot",
            "Square Foot", "Square Foot", "Foot", "Foot", "N/A" };

        public CurbTemplate(RhinoObject rhobj, string _layerName, double angleThreshold)
        {
            this.layerName = _layerName;

            geometry = (Brep)rhobj.Geometry;

            this.id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(geometry);
            this.netVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            mass_properties = VolumeMassProperties.Compute(geometry.RemoveHoles(RunQTO.doc.ModelAbsoluteTolerance));
            this.grossVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            Dictionary<string, double> topAndBottomArea = this.TopAndBottomArea(geometry, angleThreshold);

            this.topArea = Math.Round(topAndBottomArea["Top Area"], 2);

            this.bottomArea = Math.Round(topAndBottomArea["Bottom Area"], 2);

            this.length = Math.Round(Length(), 2);

            // Using the method that calculates Sides and End areas
            SidesAndOpeingArea();

            this.endArea = Math.Round(this.endFaceAreas.Sum(), 2);
        }

        Dictionary<string, double> TopAndBottomArea(Brep brep, double angleThreshold)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            double topArea = 0;
            double bottomArea = 0;

            Vector3d normal;
            double u, v;
            Point3d center;

            Plane frame;

            double dotProduct;

            Ray3d ray;
            Mesh mesh;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                center = area_properties.Centroid;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    brep.Faces[i].FrameAt(u, v, out frame);

                    dotProduct = Math.Round(Vector3d.Multiply(normal, Vector3d.ZAxis), 2);

                    if (dotProduct > angleThreshold && dotProduct <= 1)
                    {
                        this.upfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        this.upfacingFaceAreas.Add(area_properties.Area);
                        this.upfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.upfacingFacesCenters.Add(center);
                        this.upfacingFacesNormals.Add(normal);
                    }

                    else if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        this.downfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        this.downfacingFaceAreas.Add(area_properties.Area);
                        this.downfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.downfacingFacesCenters.Add(center);
                        this.downfacingFacesNormals.Add(normal);
                    }

                    else
                    {
                        this.sideAndEndFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.sideAndEndFaceAreas.Add(area_properties.Area);
                    }
                }
            }

            List<double> tempDownfacingFaceElevations = this.downfacingFaceElevations;
            List<double> tempDownfacingFaceAreas = this.downfacingFaceAreas;

            for (int i = 0; i < this.upfacingFaceElevations.Count; i++)
            {
                ray = new Ray3d(this.upfacingFacesCenters[i], this.upfacingFacesNormals[i]);

                bool isEndFace = false;

                List<Brep> tempUpfacingFaces = new List<Brep>(this.upfacingFaces);

                while (tempUpfacingFaces.Count > 0 && !isEndFace)
                {
                    mesh = Mesh.CreateFromBrep(tempUpfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

                    if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        isEndFace = true;
                    }

                    tempUpfacingFaces.Remove(tempUpfacingFaces[0]);
                }

                if (isEndFace)
                {
                    this.endFaceAreas.Add(this.upfacingFaceAreas[i]);
                }
                else
                {
                    topArea += this.upfacingFaceAreas[i];

                    this.topFaces.Add(this.upfacingFaces[i]);
                }
            }

            for (int i = 0; i < this.downfacingFaceElevations.Count; i++)
            {
                ray = new Ray3d(this.downfacingFacesCenters[i], this.downfacingFacesNormals[i]);

                bool isEndFace = false;

                List<Brep> tempDownfacingFaces = new List<Brep>(this.downfacingFaces);

                while (tempDownfacingFaces.Count > 0 && !isEndFace)
                {
                    mesh = Mesh.CreateFromBrep(tempDownfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

                    if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        isEndFace = true;
                    }

                    tempDownfacingFaces.Remove(tempDownfacingFaces[0]);
                }

                if (isEndFace)
                {
                    this.endFaceAreas.Add(this.downfacingFaceAreas[i]);
                }
                else
                {
                    bottomArea += this.downfacingFaceAreas[i];

                    this.bottomFaces.Add(this.downfacingFaces[i]);
                }
            }

            result.Add("Top Area", topArea);
            result.Add("Bottom Area", bottomArea);

            return result;
        }

        void SidesAndOpeingArea()
        {
            List<Brep> sideFaces = new List<Brep>();

            List<Brep> tempSideAndEndFaces = new List<Brep>(this.sideAndEndFaces);
            List<double> tempSideAndEndFacesAreas = new List<double>(this.sideAndEndFaceAreas);

            for (int i = 0; i < this.sideEdges.Count; i++)
            {
                for (int j = 0; j < this.sideAndEndFaces.Count; j++)
                {
                    Curve[] overlapCurves;
                    Point3d[] intersectionPoints;

                    Rhino.Geometry.Intersect.Intersection.CurveBrep(this.sideEdges[i], this.sideAndEndFaces[j], RunQTO.doc.ModelAbsoluteTolerance, out overlapCurves, out intersectionPoints);

                    if (overlapCurves.Length > 0 && intersectionPoints.Length == 0)
                    {
                        if (tempSideAndEndFaces.Contains(this.sideAndEndFaces[j]))
                        {
                            sideFaces.Add(this.sideAndEndFaces[j]);
                            tempSideAndEndFaces.Remove(this.sideAndEndFaces[j]);
                            tempSideAndEndFacesAreas.Remove(this.sideAndEndFaceAreas[j]);
                        }
                    }
                }
            }

            sideFaces = Brep.JoinBreps(sideFaces, RunQTO.doc.ModelAbsoluteTolerance).ToList<Brep>();

            this.sideArea_1 = Math.Round(sideFaces[0].GetArea(), 2);
            this.sideArea_2 = Math.Round(sideFaces[1].GetArea(), 2);

            this.openingArea = Math.Round(sideFaces[0].RemoveHoles(RunQTO.doc.ModelAbsoluteTolerance).GetArea() - this.sideArea_1, 2);

            this.endFaceAreas.AddRange(tempSideAndEndFacesAreas);
        }

        double Length()
        {
            double length = 0;

            List<double> edgeLengths;
            List<Curve> edges;

            foreach (Brep topFace in this.topFaces)
            {
                edgeLengths = new List<double>();
                edges = new List<Curve>();

                for (int i = 0; i < topFace.Edges.Count; i++)
                {
                    edgeLengths.Add(topFace.Edges[i].GetLength());
                    edges.Add(topFace.Edges[i]);
                }

                for (int i = 0; i < 2; i++)
                {
                    edges.RemoveAt(edgeLengths.IndexOf(edgeLengths.Min()));
                    edgeLengths.Remove(edgeLengths.Min());
                }

                this.sideEdges.AddRange(edges);

                edges = Curve.JoinCurves(edges).ToList<Curve>();

                length += (edges[0].GetLength() + edges[1].GetLength()) / 2;
            }

            return length;
        }
    }
}
