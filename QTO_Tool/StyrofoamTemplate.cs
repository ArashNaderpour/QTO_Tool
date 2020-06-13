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
        public double volume { get; set; }

        public StyrofoamTemplate(RhinoObject rhobj, string layerName)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = layerName;
        }
    }
}
