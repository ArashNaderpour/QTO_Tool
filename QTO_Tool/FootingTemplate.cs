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
    class FootingTemplate
    {
        public Brep geometry { get; set; }
        public System.Drawing.Color color { get; set; }
        public string layerName { get; set; }
        public string nameAbb { get; set; }
        public string id { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();
        public double volume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double sideArea { get; set; }

        public string type = "FootingTemplate";

        private Brep topBrepFace;

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Square Foot", "Square Foot", "Square Foot", "N/A" };

        public FootingTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, double angleThreshold)
        {
            this.geometry = (Brep)rhobj.Geometry;

            this.color = layerColor;

            id = rhobj.Id.ToString();

            this.layerName = _layerName;

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(geometry);
            volume = Math.Round(mass_properties.Volume * 0.037037, 2);

            this.topArea = TopArea(geometry, angleThreshold);
           
            this.bottomArea = BottomArea(geometry, angleThreshold);

            this.sideArea = SideArea(geometry);
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
                        area += Math.Round(area_properties.Area, 2);

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
                        area += Math.Round(area_properties.Area, 2);
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
            }

            return area;
        }

        double SideArea(Brep brep)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                double faceArea = area_properties.Area;

                area += faceArea;
            }

            area -= (this.topArea + this.bottomArea);

            return Math.Round(area, 2);
        }
    }
}
