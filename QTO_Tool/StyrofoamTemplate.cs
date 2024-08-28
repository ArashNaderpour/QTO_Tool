using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTO_Tool
{
    class StyrofoamTemplate
    {
        public Brep geometry { get; set; }
        public System.Drawing.Color color { get; set; }
        public string layerName { get; set; }
        public string nameAbb { get; set; }
        public string id { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();

        public string floor { get; set; }
        public double volume { get; set; }

        public string type = "StyrofoamTemplate";

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "N/A" };

        private List<double> downfacingFaceElevations = new List<double>();

        public StyrofoamTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, Dictionary<double, string> floorElevations)
        {
            this.color = layerColor;

            this.geometry = (Brep)rhobj.Geometry;

            this.layerName = _layerName;

            id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(geometry);
            volume = Math.Round(mass_properties.Volume * RunQTO.volumeConversionFactor, 2);

            for (int i = 0; i < this.geometry.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(this.geometry.Faces[i]);

                Point3d center = area_properties.Centroid;

                double u, v;

                if (this.geometry.Faces[i].ClosestPoint(center, out u, out v))
                {
                    this.downfacingFaceElevations.Add(center.Z);
                }
            }

            if (floorElevations.Count > 0)
            {
                this.floor = Methods.FindFloor(floorElevations, this.downfacingFaceElevations.Min());
            }
            else
            {
                this.floor = "-";
            }
        }
    }
}
