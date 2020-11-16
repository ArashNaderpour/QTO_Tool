using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QTO_Tool
{
    class MySqlMethods
    {
        public static void CreateMySqlDatabase(string databaseName, MySqlConnection conn)
        {
            string MySqlQuery = string.Format("DROP DATABASE IF EXISTS `{0}`;", databaseName);
            MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuery, conn);
            MySqlCommand.ExecuteNonQuery();

            MySqlQuery = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", databaseName);
            MySqlCommand = new MySqlCommand(MySqlQuery, conn);
            MySqlCommand.ExecuteNonQuery();
        }

        public static void CreateMySqlTable(string databaseName, StackPanel concreteTable, MySqlConnection conn)
        {
            string MySqlQuaery = string.Format(
                "CREATE TABLE `{0}`.`{0}` (`RowNumber` INT NOT NULL AUTO_INCREMENT,`Category` VARCHAR(45) NULL,`Quantity` VARCHAR(45) NULL,`Unit` VARCHAR(45) NULL,PRIMARY KEY(`RowNumber`)); ",
                databaseName);
            MySqlCommand MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
            MySqlCommand.ExecuteNonQuery();

            //MySqlQuaery = string.Format("INSERT INTO {0}.{0}(RowNumber, Category, Quantity, Unit) VALUES('{1}', '{2}', '{3}', '{4}')", databaseName, "1", "Asghar", "Salman", "Babi");
            //MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
            //MySqlCommand.ExecuteNonQuery();

            int rowCount = 1;

            foreach (UIElement container in concreteTable.Children)
            {
                Expander expander = (Expander)container;

                string template = expander.Name.Split('_')[1];

                Grid contentGrid = (Grid)expander.Content;

                if (rowCount > 1)
                {
                    rowCount += 2;
                }

                MySqlQuaery = string.Format("INSERT INTO {0}.{0}(Category, Quantity, Unit) VALUES('{1}', '{2}', '{3}')", databaseName, expander.Header, "Quantity", "Unit");
                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                MySqlCommand.ExecuteNonQuery();

                for (int i = 2; i < contentGrid.ColumnDefinitions.Count - 1; i++)
                {
                    double result = 0;
                    string header = "";

                    for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
                    {
                        UIElement element = contentGrid.Children.Cast<UIElement>().
                            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

                        if (element != null)
                        {
                            if (j == 0)
                            {
                                header = ((Label)element).Content.ToString();

                                MySqlQuaery = string.Format("INSERT INTO {0}.{0}(Category) VALUES('{1}')", databaseName, header);
                                MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                MySqlCommand.ExecuteNonQuery();

                                if (template == "Beam")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, BeamTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Column")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, ColumnTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "ContinousFooting")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, ContinousFootingTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Curb")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, CurbTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Footing")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, FootingTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Slab")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, SlabTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Styrofoam")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, StyrofoamTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                                if (template == "Wall")
                                {
                                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Unit='{1}' WHERE RowNumber='{2}'", databaseName, WallTemplate.units[i], rowCount + 1);
                                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                                    MySqlCommand.ExecuteNonQuery();
                                }

                            }
                            else
                            {
                                result += Convert.ToDouble(((Label)element).Content.ToString());
                            }
                        }
                        else
                        {

                            MessageBox.Show("A Null data was detected.");

                            return;
                        }
                    }
                    
                    MySqlQuaery = string.Format("UPDATE {0}.{0} SET Quantity='{1}' WHERE RowNumber='{2}'", databaseName, result, rowCount + 1);
                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                    MySqlCommand.ExecuteNonQuery();

                    rowCount++;
                }

                if (contentGrid.Children.Count > 1)
                {
                    MySqlQuaery = string.Format("INSERT INTO {0}.{0}(Category, Quantity, Unit) VALUES('{1}', '{2}', '{3}')", databaseName, "COUNT", contentGrid.RowDefinitions.Count - 1, "");
                    MySqlCommand = new MySqlCommand(MySqlQuaery, conn);
                    MySqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
