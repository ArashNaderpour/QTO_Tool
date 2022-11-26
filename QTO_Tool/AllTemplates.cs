using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTO_Tool
{
    class AllTemplates
    {
        public Dictionary<string, List<object>> allTemplates = new Dictionary<string, List<object>>();


        public void Clear()
        {
            allTemplates.Clear();
        }
    }
}
