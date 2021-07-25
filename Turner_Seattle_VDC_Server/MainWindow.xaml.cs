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
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;

namespace Turner_Seattle_VDC_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            string connStr = @"server=172.18.30.54;userid=TurnerUser;password=VDCTurner2021";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();

                this.ConnectionResult.Text = "Connection to the database was successful.";
                this.ConnectionResultWrapper.Visibility = Visibility.Visible;

                this.ConcreteTab.IsEnabled = true;
                this.ExteriorTab.IsEnabled = true;
                this.ConcreteAnalyticsTab.IsEnabled = true;
                this.ExteriorAnalyticsTab.IsEnabled = true;

                this.ImportConcreteButton.IsEnabled = true;
                this.ImportExteriorButton.IsEnabled = true;

                this.ConnectButton.Background = System.Windows.Media.Brushes.YellowGreen;
   
            }
            catch (Exception ex)
            {
                this.ConnectionResult.Text = "Connection to the database was not successful:" + "\n" + "\n" + ex.ToString();

                this.ConnectButton.Background = System.Windows.Media.Brushes.IndianRed;

                this.ConnectionResultWrapper.Visibility = Visibility.Visible;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
    }
}
