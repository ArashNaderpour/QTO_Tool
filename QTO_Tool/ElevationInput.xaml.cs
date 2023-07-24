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
        public static Dictionary<double, string> floorElevations = new Dictionary<double, string>();
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
            ElevationInput.floorElevations.Clear();

            foreach (Grid wrapper in this.ElevationInputWrapper.Children)
            {
                string floor = ((TextBox)wrapper.Children[1]).Text;
                string elevationText = ((TextBox)wrapper.Children[2]).Text;

                if (floor != string.Empty && elevationText != string.Empty)
                {
                    try
                    {
                        double elevation = Convert.ToDouble(elevationText);
                        ElevationInput.floorElevations[elevation] = floor;
                    }
                    catch
                    {
                        MessageBox.Show(floor + " was not added to the program because the input elevation is not a number.");
                    }   
                }
            }

            this.Close();
        }
    }
}
