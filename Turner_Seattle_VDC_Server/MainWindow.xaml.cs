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
using Excel = Microsoft.Office.Interop.Excel;

namespace Turner_Seattle_VDC_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string connStr = @"server=172.18.30.54;userid=TurnerUser;password=VDCTurner2021";

        MySqlConnection conn = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            string connectionResult = MySqlMethods.ConnectToServer(this.connStr, this.conn);

            if (connectionResult == "success")
            {
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
            else
            {
                this.ConnectionResult.Text = "Connection to the server was not successful:" + "\n" + "\n" + connectionResult;

                this.ConnectButton.Background = System.Windows.Media.Brushes.IndianRed;

                this.ConnectionResultWrapper.Visibility = Visibility.Visible;
            }
        }

        private void ImportConcrete_Clicked(object sender, RoutedEventArgs e)
        {
            // Open The Spread Sheet File
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // Excel File Properties
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            String filePath = "";

            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = openFileDialog.FileName;

                if (filePath.Substring(filePath.Length - 3).ToLower() != "xls" &&
                    filePath.Substring(filePath.Length - 4).ToLower() != "xlsx")
                {
                    MessageBox.Show("Please select an Execl file.");
                    return;
                }
            }
            else
            {
                // Nothing Was Selected
                return;
            }

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(filePath, 0, true, 5, "", "", true,
                Excel.XlPlatform.xlWindows, "\t", false,
                false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            int rowCount = range.Rows.Count;
            int columnCount = range.Columns.Count;

            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();

                string connectionResult = MySqlMethods.CreateMySqlDatabase("Test", this.connStr, this.conn);
                
                if (connectionResult == "success")
                {
                    this.ConnectionResult.Text = "Data has been successfully uploaded to the database.";
                }
                else
                {
                    this.ConnectionResult.Text = "Connection problem:" + "\n" + "\n" + connectionResult;

                    this.ConnectButton.Background = System.Windows.Media.Brushes.IndianRed;

                    this.ConnectionResultWrapper.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                this.ConnectionResult.Text = "Connection problem:" + "\n" + "\n" + ex.ToString();

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
