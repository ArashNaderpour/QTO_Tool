using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace QTO_Tool
{
    class BeamTemplate
    {
        public string name { get; set; }
        public double volume { get; set; }
        public double length { get; set; }
        public double bottomArea { get; set; }
        public double sideArea { get; set; }

        public BeamTemplate()
        {

        }

        public BeamTemplate(RhinoObject rhobj)
        {

        }
    }
}
