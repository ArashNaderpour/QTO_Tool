using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace QTO_Tool
{

    enum ChildStatus { ChildOfGH, ChildOfRhino, AlwaysOnTop };

    enum ConcreteTemplates { FOOTING, CONTINOUS_FOOTING, COLUMN, SLAB, BEAM, WALL, CURB, STYROFOAM };

    public class RunQTO : Command
    {
        // SubWindows: Generate Initial Data Window
        QTOUI UI;

        public static RhinoDoc doc = RhinoDoc.ActiveDoc;
        
        public RunQTO()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RunQTO Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "RunQTO"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---

            ChildStatus winChildStatus = ChildStatus.ChildOfRhino;

            //try closing a window if it's already up
            try
            {
                this.UI.Close();
            }
            catch { }

            this.UI = new QTOUI();

            Methods.SetChildStatus(this.UI, winChildStatus);

            UI.Show();

            // ---

            return Result.Success;
        }
    }
}
