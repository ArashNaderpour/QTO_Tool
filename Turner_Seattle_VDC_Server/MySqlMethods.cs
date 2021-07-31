using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                string MySqlQuery = string.Format("DROP DATABASE IF EXISTS `{0}`;", databaseName);
                MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuery, conn);
                MySqlCommand.ExecuteNonQuery();

                MySqlQuery = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", databaseName);
                MySqlCommand = new MySqlCommand(MySqlQuery, conn);
                MySqlCommand.ExecuteNonQuery();

                connectionResult = "success";
            }
            catch(Exception ex)
            {
                connectionResult = ex.ToString();
            }

            return connectionResult;
        }

        //    public static void CreateMySqlTable(string databaseName, string tableName, StackPanel concreteTable, MySqlConnection conn)
        //    {
        //        string MySqlQuaery = string.Format(
        //            "CREATE TABLE `{0}`.`{1}` (`RowNumber` INT NOT NULL AUTO_INCREMENT,`Category` VARCHAR(45) NULL,`Quantity` VARCHAR(45) NULL,`Unit` VARCHAR(45) NULL,PRIMARY KEY(`RowNumber`)); ",
        //            databaseName, tableName);
        //        MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //        MySqlCommand.ExecuteNonQuery();

        //        int rowCount = 1;

        //        foreach (UIElement container in concreteTable.Children)
        //        {
        //            Expander expander = (Expander)container;

        //            string template = expander.Name.Split('_')[1];

        //            Grid contentGrid = (Grid)expander.Content;

        //            if (rowCount > 1)
        //            {
        //                rowCount += 2;
        //            }

        //            MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category, Quantity, Unit) VALUES('{2}', '{3}', '{4}')", databaseName, tableName, expander.Header, "Quantity", "Unit");
        //            MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //            MySqlCommand.ExecuteNonQuery();

        //            for (int i = 2; i < contentGrid.ColumnDefinitions.Count - 1; i++)
        //            {
        //                double result = 0;
        //                string header = "";

        //                for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
        //                {
        //                    UIElement element = contentGrid.Children.Cast<UIElement>().
        //                        FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

        //                    if (element != null)
        //                    {
        //                        if (j == 0)
        //                        {
        //                            header = ((Label)element).Content.ToString();

        //                            MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category) VALUES('{2}')", databaseName, tableName, header);
        //                            MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                            MySqlCommand.ExecuteNonQuery();

        //                            if (template == "Beam")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, BeamTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Column")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, ColumnTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "ContinuousFooting")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, ContinuousFootingTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Curb")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, CurbTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Footing")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, FootingTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Slab")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, SlabTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Styrofoam")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, StyrofoamTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }

        //                            if (template == "Wall")
        //                            {
        //                                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Unit='{2}' WHERE RowNumber='{3}'", databaseName, tableName, WallTemplate.units[i], rowCount + 1);
        //                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                                MySqlCommand.ExecuteNonQuery();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            result += Convert.ToDouble(((Label)element).Content.ToString());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("A Null data was detected.");

        //                        return;
        //                    }
        //                }
        //                MySqlQuaery = string.Format("UPDATE {0}.{1} SET Quantity='{2}' WHERE RowNumber='{3}'", databaseName, tableName, result, rowCount + 1);
        //                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                MySqlCommand.ExecuteNonQuery();

        //                rowCount++;
        //            }

        //            if (contentGrid.Children.Count > 1)
        //            {
        //                MySqlQuaery = string.Format("INSERT INTO {0}.{1}(Category, Quantity, Unit) VALUES('{2}', '{3}', '{4}')", databaseName, tableName, "COUNT", contentGrid.RowDefinitions.Count - 1, "");
        //                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
        //                MySqlCommand.ExecuteNonQuery();
        //            }
        //        }
        //    }
    }
}
