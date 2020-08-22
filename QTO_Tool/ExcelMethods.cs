using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

                int rowCount = 1;

                foreach (UIElement expander in concreteTable.Children)
                {
                    Grid contentGrid = (Grid)(((Expander)expander).Content);

                    if (rowCount > 1)
                    {
                        rowCount++;
                    }
                    workSheet.Cells[rowCount, 1].Interior.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                    workSheet.Cells[rowCount, 1] = "NAME";
                        workSheet.Cells[rowCount + 1, 1] = ((Expander)expander).Header;

                    workSheet.Cells[rowCount, 2].Interior.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                    workSheet.Cells[rowCount, 2] = "COUNT";
                        workSheet.Cells[rowCount + 1, 2] = contentGrid.RowDefinitions.Count - 1;
                    

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

                                    workSheet.Cells[rowCount, i + 1] = header;

                                    workSheet.Cells[rowCount, i + 1].Interior.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                                }
                                else
                                {
                                    result += Convert.ToDouble(((Label)element).Content.ToString());
                                }
                            }
                            else
                            {
                                Label errorElement = contentGrid.Children.Cast<Label>().
                                FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == 0);

                                string err = errorElement.Content.ToString();

                                MessageBox.Show(String.Format("An error apeared in exporting {0} value of number {1}. Please repair model and export later.",
                                    err, j.ToString()));

                                workBook.SaveAs(outputPath);
                                workBook.Close();
                                excel.Quit();

                                return;
                            }
                        }
                        workSheet.Cells[rowCount + 1, i + 1] = result;
                    }

                    rowCount++;
                }

                Excel.Range formatRange = workSheet.UsedRange;
                formatRange.EntireColumn.AutoFit();
                formatRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

               // workSheet.Columns["A"].Delete();

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
