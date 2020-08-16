using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace QTO_Tool
{
    class ExcelMethods
    {
        public static void ExportExcel(StackPanel concreteTable)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xlsx";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string outputPath = saveFileDialog.FileName;

                Excel.Application excel = new Excel.Application();
                Excel.Workbook workBook = excel.Workbooks.Add(Type.Missing);
                Excel.Worksheet workSheet = (Excel.Worksheet)workBook.ActiveSheet;

                foreach (UIElement expander in concreteTable.Children)
                {
                    Grid contentGrid = (Grid)(((Expander)expander).Content);

                    for (int i = 2; i < contentGrid.ColumnDefinitions.Count - 1; i++)
                    {
                        double result = 0;

                        for (int j = 1; j < contentGrid.RowDefinitions.Count; j++)
                        {
                            Label element = contentGrid.Children.Cast<Label>().
                                FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);
                            result += Convert.ToDouble(element.Content.ToString());
                            if (element != null)
                            {
                                result += Convert.ToDouble(element.Content.ToString());
                            }
                            else
                            {
                                Label errorElement = contentGrid.Children.Cast<Label>().
                                FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == 0);

                                string header = errorElement.Content.ToString();

                                MessageBox.Show(String.Format("An error apeared in exporting {0} value of number {1}. Please repair model and export later.",
                                    header, j.ToString()));

                                workBook.SaveAs(outputPath);
                                workBook.Close();
                                excel.Quit();

                                return;
                            }
                        }
                        workSheet.Cells[1, i] = result;
                    }
                }

                workBook.SaveAs(outputPath);
                workBook.Close();
                excel.Quit();

                MessageBox.Show("Export was successful.");

                return;
            }
            else
            {
                MessageBox.Show("Something went wrong, please try again.");
            }
        }
    }
}
