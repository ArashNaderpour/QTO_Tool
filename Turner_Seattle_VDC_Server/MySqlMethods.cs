using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Windows;
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
        public static string CreateMySqlDatabase(string databaseName, string connStr, MySqlConnection conn)
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
            catch(Exception ex)
            {
                connectionResult = ex.ToString() + "\n";
            }

            return connectionResult;
        }

        public static string CreateMySqlTable(string databaseName, string tableName, MySqlConnection conn, Excel.Range range)
        {
            string connectionResult = "";

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
                MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category, Quantity, Unit) VALUES('{2}', '{3}', '{4}');", databaseName, tableName,
                    ((range.Cells[r, 1] as Excel.Range).Value2).ToString(),
                    ((range.Cells[r, 2] as Excel.Range).Value2).ToString(),
                    ((range.Cells[r, 3] as Excel.Range).Value2).ToString());

                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlCommand.ExecuteNonQuery();


                //for (int i = 2; i < contentGrid.ColumnDefinitions.Count - 1; i++)
                //{
                //    double result = 0;
                //    string header = "";

                //    for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
                //    {
                //        UIElement element = contentGrid.Children.Cast<UIElement>().
                //            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

                //        if (element != null)
                //        {
                //            if (j == 0)
                //            {
                //                header = ((Label)element).Content.ToString();

                //                MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category) VALUES('{2}')", databaseName, tableName, header);
                //                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                MySqlCommand.ExecuteNonQuery();

                //                if (template == "Beam")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, BeamTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Column")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, ColumnTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "ContinuousFooting")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, ContinuousFootingTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Curb")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, CurbTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Footing")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, FootingTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Slab")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, SlabTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Styrofoam")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, StyrofoamTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }

                //                if (template == "Wall")
                //                {
                //                    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, WallTemplate.units[i], rowCount + 1);
                //                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //                    MySqlCommand.ExecuteNonQuery();
                //                }
                //            }
                //            else
                //            {
                //                result += Convert.ToDouble(((Label)element).Content.ToString());
                //            }
                //        }
                //        else
                //        {
                //            MessageBox.Show("A Null data was detected.");

                //            return;
                //        }
                //    }
                //    MySqlQuaery = string.Format("UPDATE {0}.{1} SET Quantity='{2}' WHERE RowNumber='{3}'", databaseName, tableName, result, rowCount + 1);
                //    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //    MySqlCommand.ExecuteNonQuery();

                //    rowCount++;
                //}

                //if (contentGrid.Children.Count > 1)
                //{
                //    MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category, Quantity, Unit) VALUES('{2}', '{3}', '{4}')", databaseName, tableName, "COUNT", contentGrid.RowDefinitions.Count - 1, "");
                //    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                //    MySqlCommand.ExecuteNonQuery();
                //}
            }

            return connectionResult;
        }
    }
}
