using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsForms = System.Windows.Forms;

namespace Turner_Seattle_VDC_Server
{
    class UIMethods
    {
        // Updates all child tree nodes recursively.
        public static void CheckAllChildNodes(WindowsForms.TreeNode treeNode, bool nodeChecked)
        {
            foreach (WindowsForms.TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    UIMethods.CheckAllChildNodes(node, nodeChecked);
                }
            }
        }
    }
}
