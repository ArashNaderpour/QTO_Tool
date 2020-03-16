using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;


namespace QTO_Tool
{
    class Methods
    {
        internal static void SetChildStatus(QTOUI mw, ChildStatus winChildStatus)
        {
            switch (winChildStatus)
            {
                //case childStatus.ChildOfGH:
                //    setOwner(Grasshopper.Instances.DocumentEditor, mw);
                //    break;
                case ChildStatus.AlwaysOnTop:
                    mw.Topmost = true;
                    break;
                case ChildStatus.ChildOfRhino:
                    setOwner(Rhino.RhinoApp.MainWindowHandle(), mw);
                    break;
                default:
                    break;
            }
        }

        //Utility functions to set the ownership of a window object
        static void setOwner(System.Windows.Forms.Form ownerForm, System.Windows.Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerForm.Handle;
        }

        static void setOwner(IntPtr ownerPtr, System.Windows.Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerPtr;
        }

        /* ------------------------ Method For Generating Expander ------------------------ */
        //public static ComboBox ConcreteTemplateGenerator(int index)
        //{
        //    ComboBox template = new ComboBox();
        //    template.Name = department.Name + "ComboBox" + i.ToString();
        //    foreach (string functionName in functions.Keys)
        //    {
        //        if (functions[functionName]["DGSFMax"] != 0 && functions[functionName]["keyMax"] != 0)
        //        {
        //            ComboBoxItem item = new ComboBoxItem();
        //            item.Content = functionName;
        //            program.Items.Add(item);
        //        }
        //    }
        //    program.SelectedIndex = 0;
        //    program.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    program.Margin = new Thickness(0, 5, 2, 0);
        //    program.SelectionChanged += ComboBox_SelectionChanged;

        //    return template;
        //}
    }
}
