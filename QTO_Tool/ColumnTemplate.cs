using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTO_Tool
{
    class ColumnTemplate
    {
        public string name { get; set; }
        public double voulme { get; set; }
        public double height { get; set; }
        public double sideArea { get; set; }

        public ColumnTemplate(RhinoObject rhobj)
        {

        }
    }
}
