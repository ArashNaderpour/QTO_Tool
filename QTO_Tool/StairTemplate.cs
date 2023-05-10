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
    class StairTemplate
    {
        public Brep geometry { get; set; }
        public System.Drawing.Color color { get; set; }
        public string layerName { get; set; }
        public string nameAbb { get; set; }
        public string id { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();
        public double volume { get; set; }
        public double treadArea { get; set; }
        public double riserArea { get; set; }
        public int treadCount { get; set; }
        public double bottomArea { get; set; }
        public double sideArea { get; set; }

        public string type = "StairTemplate";

        private List<double> upfacingFaceAreas = new List<double>();
        private List<Brep> upfacingFaces = new List<Brep>();

        private List<double> downfacingFaceAreas = new List<double>();
        private List<Brep> downfacingFaces = new List<Brep>();

        private List<double> sideAndRiserFaceAreas = new List<double>();
        private List<Brep> sideAndRiserFaces = new List<Brep>();

        private List<double> sideFaceAreas = new List<double>();
        private List<Brep> sideFaces = new List<Brep>();

        private List<double> riserFaceAreas = new List<double>();
        private List<Brep> riserFaces = new List<Brep>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Square Foot", "Square Foot", "Square Foot", "N/A", "Square Foot", "Square Foot", "N/A" };

        public StairTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, double angleThreshold)
        {
            this.layerName = _layerName;

            this.color = layerColor;

            this.geometry = (Brep)rhobj.Geometry;

            this.id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(this.geometry);
            this.volume = Math.Round(mass_properties.Volume * 0.037037, 2);

            this.TreadAndRiserAndBottomArea(this.geometry, angleThreshold);
        }

        void TreadAndRiserAndBottomArea(Brep brep, double angleThreshold)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            Vector3d normal;
            double u, v;
            Point3d center;

            Plane frame;

            double dotProduct, curveParameter;

            Vector3d curveTangent;

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

                    if (dotProduct > RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        this.upfacingFaceAreas.Add(area_properties.Area);
                        this.upfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }

                    else if (dotProduct < -RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        this.downfacingFaceAreas.Add(area_properties.Area);
                        this.downfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }

                    else
                    {
                        this.sideAndRiserFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.sideAndRiserFaceAreas.Add(area_properties.Area);
                    }
                }
            }

            this.treadArea = Math.Round(this.upfacingFaceAreas.Sum(), 2);
            this.bottomArea = Math.Round(this.downfacingFaceAreas.Sum(), 2);

            this.treadCount = this.upfacingFaceAreas.Count;

            double offsetDistance = double.MaxValue;

            for (int i = 0; i < this.upfacingFaces[0].Edges.Count; i++)
            {
                if (this.upfacingFaces[0].Edges[i].GetLength() < offsetDistance)
                {
                    offsetDistance = this.upfacingFaces[0].Edges[i].GetLength();
                }
            }

            offsetDistance *= 0.48;

            Curve treadBoundary = Curve.JoinCurves(this.upfacingFaces[0].Edges)[0].Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);

            Curve curveOffset1 = treadBoundary.Offset(Plane.WorldXY, offsetDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            Curve curveOffset2 = treadBoundary.Offset(Plane.WorldXY, -offsetDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            List<Curve> shorterSegments = new List<Curve>();
            Curve[] curveOffsetSegments;

            if (curveOffset1.GetLength() > curveOffset2.GetLength())
            {
                curveOffsetSegments = curveOffset2.DuplicateSegments();
            }
            else
            {
                curveOffsetSegments = curveOffset1.DuplicateSegments();
            }

            for (int i = 0; i < curveOffsetSegments.Length; i++)
            {
                if (i == 0)
                {
                    shorterSegments.Add(curveOffsetSegments[i]);
                }
                else
                {
                    if (shorterSegments[0].GetLength() > curveOffsetSegments[i].GetLength())
                    {
                        shorterSegments.Clear();
                        shorterSegments.Add(curveOffsetSegments[i]);
                    }

                    else if (shorterSegments[0].GetLength() == curveOffsetSegments[i].GetLength())
                    {
                        shorterSegments.Add(curveOffsetSegments[i]);
                    }
                }
            }

            Curve centerLine = new Line(shorterSegments[0].PointAtLength(shorterSegments[0].GetLength() / 2), shorterSegments[1].PointAtLength(shorterSegments[1].GetLength() / 2)).ToNurbsCurve();

            //Side and Edges Calculation
            for (int i = 0; i < this.sideAndRiserFaces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(this.sideAndRiserFaces[i]);

                center = area_properties.Centroid;

                centerLine.ClosestPoint(center, out curveParameter);

                curveTangent = centerLine.TangentAt(curveParameter);

                if (this.sideAndRiserFaces[i].Faces[0].ClosestPoint(center, out u, out v))
                {
                    normal = this.sideAndRiserFaces[i].Faces[0].NormalAt(u, v);

                    normal.Unitize();

                    dotProduct = Math.Round(Vector3d.Multiply(normal, curveTangent), 2);

                    if (dotProduct > -0.1 && dotProduct < 0.1)
                    {
                        this.riserFaces.Add(this.sideAndRiserFaces[i]);
                        this.riserFaceAreas.Add(this.sideAndRiserFaceAreas[i]);
                    }

                    else
                    {
                        this.sideFaces.Add(this.sideAndRiserFaces[i]);
                        this.sideFaceAreas.Add(this.sideAndRiserFaceAreas[i]);
                    }
                }
            }

            this.riserArea = Math.Round(this.riserFaceAreas.Sum(), 2);
            this.sideArea = Math.Round(this.sideFaceAreas.Sum(), 2);
        }
    }
}
