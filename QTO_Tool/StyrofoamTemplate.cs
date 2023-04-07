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
        public double volume { get; set; }

        public string type = "StyrofoamTemplate";

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "N/A" };

        public StyrofoamTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor)
        {
            this.color = layerColor;

            this.geometry = (Brep)rhobj.Geometry;

            this.color = rhobj.Attributes.ObjectColor;

            this.layerName = _layerName;

            id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(geometry.RemoveHoles(0.01));
            volume = Math.Round(mass_properties.Volume * 0.037037, 2);
        }
    }
}
