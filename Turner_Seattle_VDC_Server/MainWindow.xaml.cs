using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsForms = System.Windows.Forms;
using System.Windows.Forms.Integration;
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

            this.ConnectionResult.Text = "";

            string connectionResult = MySqlMethods.ConnectToServer(this.connStr, this.conn);

            MySqlMethods.CreateConcreteDataTable(this.connStr, this.conn, this.ConcreteDataTree);


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

            this.ConnectionResult.Text = "";
            // Open The Spread Sheet File
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // Excel File Properties
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            string filePath = "";
            string fileName = "";

            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                fileName = filePath.Split('\\').Last().Split('.').First().Replace('-', '_');
                
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

            try
            {
                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(filePath, 0, true, 5, "", "", true,
                    Excel.XlPlatform.xlWindows, "\t", false,
                    false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                range = xlWorkSheet.UsedRange;

                conn = new MySqlConnection(connStr);
                conn.Open();

                //string[] nameParts = fileName.ToLower().Split('_');

                string databaseName = "concrete_" + fileName.ToLower();
                string tableName = fileName.ToLower();

                //if (nameParts.Length > 2)
                //{
                //    for (int i = 0; i < nameParts.Length; i++)
                //    {
                //        databaseName += nameParts[i] + "_";
                //    }

                //    databaseName = databaseName.Remove(databaseName.Length - 1);
                //}
                //else
                //{
                //    databaseName = "concrete_" + nameParts.First();
                //}

                string connectionResult = MySqlMethods.CreateConcreteMySqlDatabase(databaseName, this.connStr, this.conn);

                connectionResult += MySqlMethods.CreateMySqlTable(databaseName, tableName, this.conn, range);

                if (connectionResult == "")
                {
                    this.ConnectionResult.Text = "Data has been uploaded to the database successfully.";
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

        // NOTE   This code can be added to the BeforeCheck event handler instead of the AfterCheck event.
        // After a tree node's Checked property is changed, all its child nodes are updated to the same value.
        private void node_AfterCheck(object sender, WindowsForms.TreeViewEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != WindowsForms.TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    /* Calls the CheckAllChildNodes method, passing in the current 
                    Checked value of the TreeNode whose checked state changed. */
                    UIMethods.CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }
    }
}
