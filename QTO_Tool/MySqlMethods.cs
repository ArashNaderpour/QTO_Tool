using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace QTO_Tool
{
    class MySqlMethods
    {
        public static void CreateMySqlDatabase(string databaseName, MySqlConnection conn)
        {
            string s0 = string.Format("DROP DATABASE IF EXISTS `{0}`;", databaseName);
            MySqlCommand sqlCommand = new MySqlCommand(s0, conn);
            sqlCommand.ExecuteNonQuery();

            s0 = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", databaseName);
            sqlCommand = new MySqlCommand(s0, conn);
            sqlCommand.ExecuteNonQuery();
        }

        public static void CreateMySqlTable(string databaseName, StackPanel concreteTable, MySqlConnection conn)
        {
            string strCreate = string.Format(
                "CREATE TABLE `{0}`.`{0}` (`RowNumber` INT NOT NULL,`Category` VARCHAR(45) NULL,`Quantity` VARCHAR(45) NULL,`Unit` VARCHAR(45) NULL,PRIMARY KEY(`RowNumber`)); ",
                databaseName);

            MySqlCommand cmdDatabase = new MySqlCommand(strCreate, conn);
            cmdDatabase.ExecuteNonQuery();

            strCreate = string.Format(
                "CREATE TABLE `{0}`.`{1}` (`RowNumber` INT NOT NULL,`Category` VARCHAR(45) NULL,`Quantity` VARCHAR(45) NULL,`Unit` VARCHAR(45) NULL,PRIMARY KEY(`RowNumber`)); ",
                databaseName, databaseName + "_Project-Based");

            cmdDatabase = new MySqlCommand(strCreate, conn);
            cmdDatabase.ExecuteNonQuery();


            //MySqlCommand Create_table = new MySqlCommand(s0, conn);
            //Create_table.ExecuteNonQuery();

            //    int rowCount = 1;

            //    foreach (UIElement container in concreteTable.Children)
            //    {
            //        Expander expander = (Expander)container;

            //        string template = expander.Name.Split('_')[1];

            //        Grid contentGrid = (Grid)expander.Content;

            //        if (rowCount > 1)
            //        {
            //            rowCount += 2;
            //        }
            //        workSheet.Cells[rowCount, 1].Interior.Color =
            //                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
            //        workSheet.Cells[rowCount, 1] = expander.Header;

            //        workSheet.Cells[rowCount, 2].Interior.Color =
            //                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
            //        workSheet.Cells[rowCount, 2] = "Quantity";

            //        workSheet.Cells[rowCount, 3].Interior.Color =
            //                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
            //        workSheet.Cells[rowCount, 3] = "Unit";


            //        for (int i = 2; i < contentGrid.ColumnDefinitions.Count - 1; i++)
            //        {
            //            double result = 0;
            //            string header = "";

            //            for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
            //            {
            //                UIElement element = contentGrid.Children.Cast<UIElement>().
            //                    FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

            //                if (element != null)
            //                {
            //                    if (j == 0)
            //                    {
            //                        header = ((Label)element).Content.ToString();

            //                        workSheet.Cells[rowCount + 1, 1] = header;

            //                        if (template == "Beam")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = BeamTemplate.units[i];
            //                        }

            //                        if (template == "Column")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = ColumnTemplate.units[i];
            //                        }

            //                        if (template == "ContinousFooting")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = ContinousFootingTemplate.units[i];
            //                        }

            //                        if (template == "Curb")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = CurbTemplate.units[i];
            //                        }

            //                        if (template == "Footing")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = FootingTemplate.units[i];
            //                        }

            //                        if (template == "Slab")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = SlabTemplate.units[i];
            //                        }

            //                        if (template == "Styrofoam")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = StyrofoamTemplate.units[i];
            //                        }

            //                        if (template == "Wall")
            //                        {
            //                            workSheet.Cells[rowCount + 1, 3] = WallTemplate.units[i];
            //                        }

            //                    }
            //                    else
            //                    {
            //                        result += Convert.ToDouble(((Label)element).Content.ToString());
            //                    }
            //                }
            //                else
            //                {
            //                    Label errorElement = contentGrid.Children.Cast<Label>().
            //                    FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == 0);

            //                    string err = errorElement.Content.ToString();

            //                    MessageBox.Show(String.Format("An error apeared in exporting {0} value of number {1}. Please repair model and export later.",
            //                        err, j.ToString()));

            //                    workBook.SaveAs(savePath);
            //                    workBook.Close();
            //                    excel.Quit();

            //                    return;
            //                }
            //            }
            //            workSheet.Cells[rowCount + 1, 2] = result;

            //            rowCount++;
            //        }

            //        if (contentGrid.Children.Count > 1)
            //        {
            //            workSheet.Cells[rowCount + 1, 1] = "Count";
            //            workSheet.Cells[rowCount + 1, 2] = contentGrid.RowDefinitions.Count - 1;
            //        }
            //    }

            //    Excel.Range formatRange = workSheet.UsedRange;
            //    formatRange.EntireColumn.AutoFit();
            //    formatRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            //    workBook.SaveAs(savePath);
            //    workBook.Close();
            //    excel.Quit();
        }
    }
}
