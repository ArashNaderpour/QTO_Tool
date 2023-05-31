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
            MessageBox.Show("Accept");
        }
    }
}
