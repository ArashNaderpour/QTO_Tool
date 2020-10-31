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
    class BeamTemplate
    {
        public string name { get; set; }
        public string id { get; set; }
        public double volume { get; set; }
        public double bottomArea { get; set; }
        public double sideArea { get; set; }
        public double length { get; set; }

        static string type = "BeamTemplate";
        private Brep topBrepFace;
        private Brep bottomBrepFace;

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Square Foot", "Square Foot", "Foot", "N/A" };

        public BeamTemplate(RhinoObject rhobj, string layerName, double angleThreshold)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = rhobj.Name;

            id = rhobj.Id.ToString();

            var mass_properties = VolumeMassProperties.Compute(tempBrep);
            volume = Math.Round(mass_properties.Volume * 0.037037, 2);

            bottomArea = BottomArea(tempBrep, angleThreshold);

            sideArea = SideArea(tempBrep, angleThreshold);

            length = Length();

        }

        double TopArea(Brep brep, double angleThreshold)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                Point3d center = area_properties.Centroid;

                double u, v;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    Vector3d normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    double dotProduct = Vector3d.Multiply(normal, Vector3d.ZAxis);

                    if (dotProduct > angleThreshold && dotProduct <= 1)
                    {
                        area = Math.Round(area_properties.Area, 2);

                        this.topBrepFace = brep.Faces[i].DuplicateFace(false);
                    }
                }
            }

            if (area == 0 && brep.Faces.Count > 0)
            {
                List<double> centerZValues = new List<double>();
                List<double> faceAreas = new List<double>();

                for (int i = 0; i < brep.Faces.Count; i++)
                {
                    var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                    Point3d center = area_properties.Centroid;

                    centerZValues.Add(center.Z);
                    faceAreas.Add(Math.Round(area_properties.Area, 2));
                }

                int topFaceIndex = centerZValues.IndexOf(centerZValues.Max());

                area = faceAreas[topFaceIndex];

                this.topBrepFace = brep.Faces[topFaceIndex].DuplicateFace(false);
            }

            return area;
        }

        double BottomArea(Brep brep, double angleThreshold)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                Point3d center = area_properties.Centroid;

                double u, v;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    Vector3d normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    double dotProduct = Vector3d.Multiply(normal, Vector3d.ZAxis);

                    if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        area = Math.Round(area_properties.Area, 2);
                    }
                }
            }

            if (area == 0 && brep.Faces.Count > 0)
            {
                List<double> centerZValues = new List<double>();
                List<double> faceAreas = new List<double>();

                for (int i = 0; i < brep.Faces.Count; i++)
                {
                    var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                    Point3d center = area_properties.Centroid;

                    centerZValues.Add(center.Z);
                    faceAreas.Add(Math.Round(area_properties.Area, 2));
                }

                int bottomFaceIndex = centerZValues.IndexOf(centerZValues.Min());

                area = faceAreas[bottomFaceIndex];

                this.bottomBrepFace = brep.Faces[bottomFaceIndex].DuplicateFace(false);
            }

            return area;
        }

        double SideArea(Brep brep, double angleThreshold)
        {
            double area = 0;
            List<double> faceAreas = new List<double>();

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                double faceArea = Math.Round(area_properties.Area, 2);

                area += faceArea;

                faceAreas.Add(faceArea);
            }

            faceAreas.Sort();

            area -= (this.TopArea(brep, angleThreshold) + this.bottomArea) +
                (faceAreas[0] + faceAreas[1]);

            return area;
        }

        double Length()
        {
            double length = 0;

            List<double> edgeLengths = new List<double>();

            for (int i = 0; i < this.topBrepFace.Edges.Count; i++)
            {
                double edgeLength = Math.Round(this.topBrepFace.Edges[i].GetLength(), 2);

                edgeLengths.Add(edgeLength);

                length += edgeLength;
            }

            edgeLengths.Sort();

            length -= (edgeLengths[0] + edgeLengths[1]);

            length /= 2;

            return length;
        }
    }
}
