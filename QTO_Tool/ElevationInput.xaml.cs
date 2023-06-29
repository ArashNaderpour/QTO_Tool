using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QTO_Tool
{
    /// <summary>
    /// Interaction logic for ElevationInput.xaml
    /// </summary>
    public partial class ElevationInput : Window
    {
        public static Dictionary<string, string> inputElevation = new Dictionary<string, string>();
        public ElevationInput()
        {
            InitializeComponent();
        }

        private void AddLevel_Clicked(object sender, RoutedEventArgs e)
        {
            UIMethods.AddElevationInput(this.ElevationInputWrapper);
        }

        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            ElevationInput.inputElevation.Clear();

            foreach (Grid wrapper in this.ElevationInputWrapper.Children)
            {
                string floor = ((TextBox)wrapper.Children[1]).Text;
                string elevation = ((TextBox)wrapper.Children[2]).Text;

                if (floor != string.Empty && elevation != string.Empty)
                {
                    try
                    {
                        Convert.ToDouble(elevation);
                        ElevationInput.inputElevation[floor] = elevation;
                    }
                    catch
                    {
                        MessageBox.Show(floor + " was not added to the program because the input elevation is not a number.");
                    }   
                }
            }

            MessageBox.Show(ElevationInput.inputElevation.Count.ToString());
        }
    }
}
