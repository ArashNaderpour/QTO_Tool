using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace Turner_Seattle_VDC_Server
{
    public class MySqlMethods
    {
        public static string ConnectToServer(string connStr, MySqlConnection conn)
        {
            string result = "";
            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();

                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static string CreateConcreteMySqlDatabase(string databaseName, string connStr, MySqlConnection conn)
        {
            string connectionResult = "";

            MySqlMethods.ConnectToServer(connStr, conn);

            try
            {
                string MySqlQuery = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", databaseName);
                MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuery, conn);
                MySqlCommand.ExecuteNonQuery();

                connectionResult = "";
            }
            catch (Exception ex)
            {
                connectionResult = ex.ToString() + "\n";
            }

            return connectionResult;
        }

        public static string CreateMySqlTable(string databaseName, string tableName, MySqlConnection conn, Excel.Range range)
        {
            string connectionResult = "";

            string excelColumn1;
            string excelColumn2;
            string excelColumn3;

            int rowCount = range.Rows.Count;
            int columnCount = range.Columns.Count;

            string MySqlQuaery;
            MySqlCommand MySqlCommand;
            
            try
            {
                MySqlQuaery = string.Format("DROP TABLE IF EXISTS `{0}`.`{1}`;", databaseName, tableName);
                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlCommand.ExecuteNonQuery();

                MySqlQuaery = string.Format("CREATE TABLE `{0}`.`{1}` (`RowNumber` INT NOT NULL AUTO_INCREMENT,`Category` VARCHAR(45) NULL,`Quantity` VARCHAR(45) NULL,`Unit` VARCHAR(45) NULL,PRIMARY KEY(`RowNumber`));",
                    databaseName, tableName);
                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlCommand.ExecuteNonQuery();

                connectionResult = "";
            }
            catch (Exception ex)
            {
                connectionResult = ex.ToString() + "\n";
            }

            for (int r = 1; r < rowCount; r++)
            {
                try
                {
                    excelColumn1 = ((range.Cells[r, 1] as Excel.Range).Value2).ToString();
                }
                catch
                {
                    excelColumn1 = "";
                }
                try
                {
                    excelColumn2 = ((range.Cells[r, 2] as Excel.Range).Value2).ToString();
                }
                catch
                {
                    excelColumn2 = "";
                }
                try
                {
                    excelColumn3 = ((range.Cells[r, 3] as Excel.Range).Value2).ToString();
                }
                catch
                {
                    excelColumn3 = "";
                }
                MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category, Quantity, Unit) VALUES('{2}', '{3}', '{4}');", databaseName, tableName,
                    excelColumn1, excelColumn2, excelColumn3);

                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlCommand.ExecuteNonQuery();
            }

            return connectionResult;
        }

        public static void CreateConcreteDataTable(string connStr, MySqlConnection conn, Border concreteDataTableWrapper)
        {
            Dictionary<string, List<string>> dataTable = new Dictionary<string, List<string>>();

            string MySqlQuaery = "show databases;";

            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();

                MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlDataReader MySqlReader = MySqlCommand.ExecuteReader();
                while (MySqlReader.Read())
                {
                    if (MySqlReader.GetString(0).Contains("concrete_")) {
                        dataTable.Add(MySqlReader.GetString(0), new List<string>());
                    }
                }
                MySqlReader.Close();

                foreach (string databaseName in dataTable.Keys)
                {
                    MySqlQuaery = string.Format("show tables from {0}", databaseName);

                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                    MySqlReader = MySqlCommand.ExecuteReader();
                    while (MySqlReader.Read())
                    {
                        dataTable[databaseName].Add(MySqlReader.GetString(0));
                    }
                    MySqlReader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //connectionResult = ex.ToString() + "\n";
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            TreeView concreteDataTable = new TreeView();
            
            concreteDataTable.FontSize = 40;
            concreteDataTable.Margin = new Thickness(10, 10, 10, 0);
            concreteDataTable.HorizontalAlignment = HorizontalAlignment.Stretch;
            concreteDataTable.VerticalAlignment = VerticalAlignment.Top;

            TreeViewItem ParentItem = new TreeViewItem();
            ParentItem.Header = "Concrete Data";
            concreteDataTable.Items.Add(ParentItem);

            foreach (string databaseName in dataTable.Keys)
            {
                TreeViewItem project = new TreeViewItem();
                project.Header = databaseName.Replace("concrete_", "");
                ParentItem.Items.Add(project);
                project.FontSize = 30;
                project.Foreground = System.Windows.Media.Brushes.MidnightBlue;

                foreach (string tableName in dataTable[databaseName])
                {
                    TreeViewItem tableItem = new TreeViewItem();
                    tableItem.Header = tableName.Replace("concrete_", "");
                    tableItem.Items.Add(tableItem);
                    tableItem.FontSize = 20;
                }
            }

            concreteDataTableWrapper.Child = concreteDataTable;
        }
    }
}
