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
        public string name { get; set; }
        public string id { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();
        public double volume { get; set; }

        static string type = "StyrofoamTemplate";

        private Brep topBrepFace;

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "N/A" };

        public StyrofoamTemplate(RhinoObject rhobj, string layerName)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = rhobj.Name;

            id = rhobj.Id.ToString();

            for (int i = 0; i < layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + i.ToString(), layerName.Split('_').ToList()[i]);
            }

            var mass_properties = VolumeMassProperties.Compute(tempBrep.RemoveHoles(0.01));
            volume = Math.Round(mass_properties.Volume * 0.037037, 2);
        }
    }
}
