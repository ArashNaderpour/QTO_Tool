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
    class ColumnTemplate
    {
        public string name { get; set; }
        public string id { get; set; }
        public double volume { get; set; }
        public double height { get; set; }
        public double sideArea { get; set; }

        static string type = "ColumnTemplate";
        Dictionary<string, Point3d> topAndBottomFaceCenters = new Dictionary<string, Point3d>();

        public ColumnTemplate(RhinoObject rhobj, string layerName)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = rhobj.Name;

            id = rhobj.Id.ToString();

            var mass_properties = VolumeMassProperties.Compute(tempBrep);
            volume = Math.Round(mass_properties.Volume, 2);

            sideArea = SideArea(tempBrep);

            height = Height(topAndBottomFaceCenters);
        }

        double SideArea(Brep brep)
        {
            double area = 0;

            List<double> faceAreas = new List<double>();
            List<double> faceCentersZ = new List<double>();
            List<Point3d> faceCenters = new List<Point3d>();

            Point3d center;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                center = area_properties.Centroid;

                faceAreas.Add(area_properties.Area);
                faceCentersZ.Add(center.Z);
                faceCenters.Add(center);

                area += area_properties.Area;
            }

            int topFaceIndex = faceCentersZ.IndexOf(faceCentersZ.Max());
            int bottomFaceIndex = faceCentersZ.IndexOf(faceCentersZ.Min());

            topAndBottomFaceCenters.Add("Top", faceCenters[topFaceIndex]);
            topAndBottomFaceCenters.Add("Bottom", faceCenters[bottomFaceIndex]);

            area -= (faceAreas[topFaceIndex] + faceAreas[bottomFaceIndex]);

            area = Math.Round(area, 2);

            return area;
        }

        double Height(Dictionary<string, Point3d> _topAndBottomFaceCenters)
        {
            double height = 0;

            height = _topAndBottomFaceCenters["Top"].Z - _topAndBottomFaceCenters["Bottom"].Z;

            height = Math.Round(height, 2);

            return height;
        }
    }
}
