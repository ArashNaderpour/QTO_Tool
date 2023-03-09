using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.IO;

namespace QTO_Tool
{
    class ExcelMethods
    {
        public static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static void ExportExcel(StackPanel layerBasedConcreteTable, StackPanel projectBasedConcreteTable, List<string> layerPropertyColumnHeaders)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xlsx";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string outputPath = saveFileDialog.FileName;

                Thread newWindowThread = new Thread(new ThreadStart(() =>
                {
                    // Create our context, and install it:
                    SynchronizationContext.SetSynchronizationContext(
                        new DispatcherSynchronizationContext(
                            Dispatcher.CurrentDispatcher));

                    // Create and configure the window
                    ProgressWindow progressWindow = new ProgressWindow();

                    // When the window closes, shut down the dispatcher
                    progressWindow.Closed += (s, eventArg) =>
                       Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                    progressWindow.Show();
                    // Start the Dispatcher Processing
                    Dispatcher.Run();
                }));

                newWindowThread.SetApartmentState(ApartmentState.STA);
                // Make the thread a background thread
                newWindowThread.IsBackground = true;
                // Start the thread
                newWindowThread.Start();

                Excel.Application excel = new Excel.Application();

                try
                {
                    ExcelMethods.PrepareExel(layerBasedConcreteTable, outputPath, layerPropertyColumnHeaders, excel);
                }

                catch (Exception ex)
                {
                    excel.Quit();
                    MessageBox.Show(ex.ToString());
                }


                Dispatcher.FromThread(newWindowThread).InvokeShutdown();

                MessageBox.Show("Export was successful.");
            }

            else
            {
                MessageBox.Show("Export was canceled.");
            }
        }

        static void PrepareExel(StackPanel ConcreteTable, string savePath, List<string> _layerPropertyColumnHeaders, Excel.Application excel)
        {
            List<string> summarySheetHeaders = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "BOTTOM AREA", "OPENING AREA",
                "TOP AREA", "SIDE AREA", "END AREA", "SIDE-1", "SIDE-2", "EDGE AREA", "LENGTH", "HEIGHT", "PERIMETER", "OPENING PERIMETER" };

            List<string> projectSheetHeaders = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "BOTTOM AREA", "OPENING AREA",
                "TOP AREA", "SIDE AREA", "END AREA", "SIDE-1", "SIDE-2", "EDGE AREA", "LENGTH", "HEIGHT", "PERIMETER", "OPENING PERIMETER" };

            string tempExcelTemplate = @"c:\Temp\QTO_Template.xlsx";

            File.WriteAllBytes(tempExcelTemplate, Resources.template);

            Dictionary<string, string> dataColumns = new Dictionary<string, string>();

            projectSheetHeaders.InsertRange(2, _layerPropertyColumnHeaders);

            excel.DisplayAlerts = false;
            Excel.Workbook workBook = (Excel.Workbook)(excel.Workbooks._Open(tempExcelTemplate, System.Reflection.Missing.Value,
                System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                System.Reflection.Missing.Value, System.Reflection.Missing.Value));

            Excel.Sheets sheets = workBook.Worksheets;

            Excel.Worksheet summarySheet = (Excel.Worksheet)sheets.get_Item(1);
            Excel.Worksheet projectSheet = (Excel.Worksheet)sheets.get_Item(2);

            int projectRowCount = ConcreteTable.Children.Count + 1;

            Excel.ListObject summaryTable = summarySheet.ListObjects[1];
            Excel.ListObject projectTable = projectSheet.ListObjects[1];

            projectTable.Resize(projectSheet.Range["A1", ExcelMethods.alphabet[projectSheetHeaders.Count - 1] + projectRowCount.ToString()]);

            List<string> uniqueNameAbbs = new List<string>();

            int layerCount = 0;

            foreach (UIElement container in ConcreteTable.Children)
            {
                int colCount = 1;

                if (layerCount == 0)
                {
                    foreach (string header in projectSheetHeaders)
                    {
                        projectSheet.Cells[1, colCount] = header;
                        projectSheet.Cells[1, colCount].Interior.Color =
                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);

                        projectSheet.Cells[2, colCount] = "-";

                        projectSheet.Cells[3 + ConcreteTable.Children.Count, colCount].Formula =
                            "=Sum(" + projectSheet.Cells[2, colCount].Address + ":" + projectSheet.Cells[2 + ConcreteTable.Children.Count, colCount].Address + ")";

                        projectSheet.Cells[3 + ConcreteTable.Children.Count, colCount].Interior.Color =
                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);

                        projectSheet.Cells[3 + ConcreteTable.Children.Count, colCount].Font.Bold = true;

                        projectSheet.Cells[3 + ConcreteTable.Children.Count, colCount].NumberFormat = "#,#.00";

                        colCount++;
                    }
                }
                else
                {
                    foreach (string header in projectSheetHeaders)
                    {
                        projectSheet.Cells[2 + layerCount, colCount] = "-";

                        colCount++;
                    }
                }

                Expander expander = (Expander)container;

                string template = expander.Name.Split('_')[1];

                Grid contentGrid = (Grid)expander.Content;

                for (int i = 0; i < contentGrid.ColumnDefinitions.Count - 1; i++)
                {
                    double numberValue = 0;
                    string textValue = string.Empty;

                    int projectColumnIndex = 0;
                    int summaryColumnIndex = 0;

                    for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
                    {
                        UIElement element = contentGrid.Children.Cast<UIElement>().
                            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

                        if (element != null)
                        {
                            string value = ((TextBlock)element).Text;

                            if (j == 0)
                            {
                                projectColumnIndex = projectSheetHeaders.IndexOf(value);
                                summaryColumnIndex = summarySheetHeaders.IndexOf(value);
                            }

                            else
                            {
                                try
                                {
                                    if (i == 0)
                                    {
                                        numberValue++;
                                    }
                                    else
                                    {
                                        numberValue += Convert.ToDouble(value);
                                    }
                                }
                                catch
                                {
                                    if (value == "N/A")
                                    {
                                        textValue = "-";
                                    }
                                    else
                                    {
                                        textValue = value;
                                    }

                                    if (summaryColumnIndex == 1)
                                    {
                                        if (!uniqueNameAbbs.Contains(textValue))
                                        {
                                            uniqueNameAbbs.Add(textValue);
                                        }
                                    }
                                }
                            }
                        }

                        else
                        {
                            TextBlock errorElement = contentGrid.Children.Cast<TextBlock>().
                            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == 0);

                            string err = errorElement.Text;

                            MessageBox.Show(String.Format("An error apeared in exporting {0} value of number {1}. Please repair model and export later.",
                                err, j.ToString()));

                            workBook.SaveAs(savePath);
                            workBook.Close();
                            excel.Quit();

                            return;
                        }
                    }

                    if (textValue == string.Empty)
                    {
                        if (numberValue > 0)
                        {
                            projectSheet.Cells[2 + layerCount, 1 + projectColumnIndex] = numberValue;
                            projectSheet.Cells[2 + layerCount, 1 + projectColumnIndex].NumberFormat = "#,#.00";
                        }
                        else
                        {
                            projectSheet.Cells[2 + layerCount, 1 + projectColumnIndex] = "-";
                        }
                    }
                    else
                    {
                        projectSheet.Cells[2 + layerCount, 1 + projectColumnIndex] = textValue;
                    }
                }

                layerCount++;
            }

            int summaryRowCount = uniqueNameAbbs.Count + 1;
            int SumCellRowNumber = summaryRowCount + 2;

            summarySheet.Range["A2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[COUNT])";
            summarySheet.Range["A" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'A') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'A') + 1].Address + ")";
            summarySheet.Range["A" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["A" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Cells[SumCellRowNumber, Array.IndexOf(ExcelMethods.alphabet, 'B') + 1] = "-";
            summarySheet.Range["B" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["B" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["C2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[GROSS VOLUME])";
            summarySheet.Range["C" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'C') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'C') + 1].Address + ")";
            summarySheet.Range["C" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["C" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["D2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[NET VOLUME])";
            summarySheet.Range["D" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'D') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'D') + 1].Address + ")";
            summarySheet.Range["D" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["D" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["E2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[BOTTOM AREA])";
            summarySheet.Range["E" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'E') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'E') + 1].Address + ")";
            summarySheet.Range["E" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["E" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["F2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[OPENING AREA])";
            summarySheet.Range["F" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'F') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'F') + 1].Address + ")";
            summarySheet.Range["F" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["F" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["G2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[TOP AREA])";
            summarySheet.Range["G" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'G') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'G') + 1].Address + ")";
            summarySheet.Range["G" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["G" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["H2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[SIDE AREA])";
            summarySheet.Range["H" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'H') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'H') + 1].Address + ")";
            summarySheet.Range["H" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["H" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["I2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[END AREA])";
            summarySheet.Range["I" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'I') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'I') + 1].Address + ")";
            summarySheet.Range["I" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["I" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["J2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[SIDE-1])";
            summarySheet.Range["J" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'J') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'J') + 1].Address + ")";
            summarySheet.Range["J" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["J" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["K2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[SIDE-2])";
            summarySheet.Range["K" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'K') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'K') + 1].Address + ")";
            summarySheet.Range["K" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["K" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["L2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[EDGE AREA])";
            summarySheet.Range["L" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'L') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'L') + 1].Address + ")";
            summarySheet.Range["L" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["L" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["M2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[LENGTH])";
            summarySheet.Range["M" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'M') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'M') + 1].Address + ")";
            summarySheet.Range["M" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["M" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["N2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[HEIGHT])";
            summarySheet.Range["N" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'N') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'N') + 1].Address + ")";
            summarySheet.Range["N" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["N" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["O2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[PERIMETER])";
            summarySheet.Range["O" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'O') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'O') + 1].Address + ")";
            summarySheet.Range["O" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["O" + SumCellRowNumber.ToString()].Font.Bold = true;
            summarySheet.Range["P2"].Formula = "=SUMIF(PROJECT!PROJECT_TABLE[NAME ABB.],$B2,PROJECT!PROJECT_TABLE[OPENING PERIMETER])";
            summarySheet.Range["P" + SumCellRowNumber.ToString()].Formula =
                "=Sum(" + summarySheet.Cells[2, Array.IndexOf(ExcelMethods.alphabet, 'P') + 1].Address + ":" + summarySheet.Cells[(SumCellRowNumber - 1), Array.IndexOf(ExcelMethods.alphabet, 'P') + 1].Address + ")";
            summarySheet.Range["P" + SumCellRowNumber.ToString()].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.CornflowerBlue);
            summarySheet.Range["P" + SumCellRowNumber.ToString()].Font.Bold = true;

            summaryTable.Resize(summarySheet.Range["A1", ExcelMethods.alphabet[summarySheetHeaders.Count - 1] + summaryRowCount.ToString()]);

            for (int i = 0; i < uniqueNameAbbs.Count; i++)
            {
                summarySheet.Cells[2 + i, 2] = uniqueNameAbbs[i];
            }

            Excel.Range projectFormatRange = projectSheet.UsedRange;
            projectFormatRange.EntireColumn.AutoFit();
            projectFormatRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            Excel.Range summaryFormatRange = summarySheet.UsedRange;
            summaryFormatRange.EntireColumn.AutoFit();
            summaryFormatRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            workBook.SaveAs(savePath);
            workBook.Close();
            excel.Quit();
        }
    }
}
