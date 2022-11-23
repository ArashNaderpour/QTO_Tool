using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;

namespace QTO_Tool
{
    class ExcelMethods
    {
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
                try
                {
                    ExcelMethods.PrepareExel(layerBasedConcreteTable, outputPath, layerPropertyColumnHeaders);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }


                Dispatcher.FromThread(newWindowThread).InvokeShutdown();

                string messageBoxText = "Do you want to save \"Project Based\" results?";
                string caption = "Save Project Based";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult userDecision = MessageBox.Show(messageBoxText, caption, button, icon);

                Thread newWindowThread_1 = new Thread(new ThreadStart(() =>
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

                newWindowThread_1.SetApartmentState(ApartmentState.STA);
                // Make the thread a background thread
                newWindowThread_1.IsBackground = true;
                // Start the thread
                newWindowThread_1.Start();

                switch (userDecision)
                {
                    case MessageBoxResult.Yes:

                        outputPath = outputPath.Replace(".xlsx", "_Project-Based.xlsx");

                        ExcelMethods.PrepareExel(projectBasedConcreteTable, outputPath, layerPropertyColumnHeaders);

                        Dispatcher.FromThread(newWindowThread_1).InvokeShutdown();

                        MessageBox.Show("Export was successful.");

                        return;

                    case MessageBoxResult.No:

                        newWindowThread_1.Abort();

                        MessageBox.Show("Export was successful.");

                        return;
                }
            }

            else
            {
                MessageBox.Show("Something went wrong, please try again.");
            }
        }

        static void PrepareExel(StackPanel ConcreteTable, string savePath, List<string> _layerPropertyColumnHeaders)
        {
            List<string> sSHeaders = new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "BOTTOM AREA", "OPENING AREA",
                "TOP AREA", "SIDE AREA", "END AREA", "SIDE-1", "SIDE-2", "EDGE AREA", "LENGTH", "HEIGHT", "PERIMETER", "OPENING PERIMETER" };

            Dictionary<string, string> dataColumns = new Dictionary<string, string>();

            sSHeaders.InsertRange(0, _layerPropertyColumnHeaders);

            Excel.Application excel = new Excel.Application();
            Excel.Workbook workBook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet workSheet = (Excel.Worksheet)workBook.ActiveSheet;

            int layerCount = 0;

            foreach (UIElement container in ConcreteTable.Children)
            {
                int colCount = 1;

                if (layerCount == 0)
                {
                    foreach (string header in sSHeaders)
                    {
                        workSheet.Cells[1, colCount] = header;

                        workSheet.Cells[1, colCount].Interior.Color = 
                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);

                        workSheet.Cells[2, colCount] = "N/A";

                        colCount++;
                    }
                }
                else
                {
                    foreach (string header in sSHeaders)
                    {
                        workSheet.Cells[2 + layerCount, colCount] = "N/A";

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

                    int columnIndex = 0;

                    for (int j = 0; j < contentGrid.RowDefinitions.Count; j++)
                    {
                        UIElement element = contentGrid.Children.Cast<UIElement>().
                            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == j);

                        if (element != null)
                        {
                            string value = ((TextBlock)element).Text;

                            if (j == 0)
                            {
                                columnIndex = sSHeaders.IndexOf(value);
                                //MessageBox.Show(value + "------>" + columnIndex.ToString());
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
                                    textValue = value;
                                }
                            }
                        }

                        else
                        {
                            Label errorElement = contentGrid.Children.Cast<Label>().
                            FirstOrDefault(e => Grid.GetColumn(e) == i && Grid.GetRow(e) == 0);

                            string err = errorElement.Content.ToString();

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
                        workSheet.Cells[2 + layerCount, 1 + columnIndex] = numberValue;
                    }
                    else
                    {
                        workSheet.Cells[2 + layerCount, 1 + columnIndex] = textValue;
                    }
                }

                layerCount++;
            }

            Excel.Range formatRange = workSheet.UsedRange;
            formatRange.EntireColumn.AutoFit();
            formatRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            workBook.SaveAs(savePath);
            workBook.Close();
            excel.Quit();
        }
    }
}
