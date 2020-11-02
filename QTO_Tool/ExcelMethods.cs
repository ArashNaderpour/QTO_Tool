using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace QTO_Tool
{
    class ExcelMethods
    {
        public static void ExportExcel(StackPanel layerBasedConcreteTable, StackPanel projectBasedConcreteTable)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xlsx";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string outputPath = saveFileDialog.FileName;

                ExcelMethods.PrepareExel(layerBasedConcreteTable, outputPath);

                string messageBoxText = "Do you want to save \"Project Based\" results?";
                string caption = "Save Project Based";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult userDecision = MessageBox.Show(messageBoxText, caption, button, icon);

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

                switch (userDecision)
                {
                    case MessageBoxResult.Yes:

                        outputPath = outputPath.Replace(".xlsx", "_Project-Based.xlsx");
                       
                        ExcelMethods.PrepareExel(projectBasedConcreteTable, outputPath);

                        Dispatcher.FromThread(newWindowThread).InvokeShutdown();

                        MessageBox.Show("Export was successful.");

                        return;

                    case MessageBoxResult.No:

                        Dispatcher.FromThread(newWindowThread).InvokeShutdown();

                        MessageBox.Show("Export was successful.");

                        return;
                }
            }

            else
            {
                MessageBox.Show("Something went wrong, please try again.");
            }
        }

        static void PrepareExel(StackPanel ConcreteTable, string savePath)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workBook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet workSheet = (Excel.Worksheet)workBook.ActiveSheet;

            int rowCount = 1;

            foreach (UIElement container in ConcreteTable.Children)
            {
                Expander expander = (Expander)container;

                string template = expander.Name.Split('_')[1];

                Grid contentGrid = (Grid)expander.Content;

                if (rowCount > 1)
                {
                    rowCount += 2;
                }
                workSheet.Cells[rowCount, 1].Interior.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                workSheet.Cells[rowCount, 1] = expander.Header;

                workSheet.Cells[rowCount, 2].Interior.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                workSheet.Cells[rowCount, 2] = "Quantity";

                workSheet.Cells[rowCount, 3].Interior.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.YellowGreen);
                workSheet.Cells[rowCount, 3] = "Unit";


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

                                workSheet.Cells[rowCount + 1, 1] = header;

                                if (template == "Beam")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = BeamTemplate.units[i];
                                }

                                if (template == "Column")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = ColumnTemplate.units[i];
                                }

                                if (template == "ContinousFooting")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = ContinousFootingTemplate.units[i];
                                }

                                if (template == "Curb")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = CurbTemplate.units[i];
                                }

                                if (template == "Footing")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = FootingTemplate.units[i];
                                }

                                if (template == "Slab")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = SlabTemplate.units[i];
                                }

                                if (template == "Styrofoam")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = StyrofoamTemplate.units[i];
                                }

                                if (template == "Wall")
                                {
                                    workSheet.Cells[rowCount + 1, 3] = WallTemplate.units[i];
                                }

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

                            workBook.SaveAs(savePath);
                            workBook.Close();
                            excel.Quit();

                            return;
                        }
                    }
                    workSheet.Cells[rowCount + 1, 2] = result;

                    rowCount++;
                }

                if (contentGrid.Children.Count > 1)
                {
                    workSheet.Cells[rowCount + 1, 1] = "Count";
                    workSheet.Cells[rowCount + 1, 2] = contentGrid.RowDefinitions.Count - 1;
                }
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
