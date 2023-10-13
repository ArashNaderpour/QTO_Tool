using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QTO_Tool
{
    /// <summary>
    /// Interaction logic for ElevationInput.xaml
    /// </summary>
    public partial class ElevationInput : Window
    {
        public static Dictionary<double, string> floorElevations = new Dictionary<double, string>();

        public event EventHandler ChangeSetFloorButtonColorRequest;

        public ElevationInput()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ElevationInput.floorElevations = Methods.RetrieveDictionaryFromDocumentStrings();

            if (ElevationInput.floorElevations.Count > 0)
            {
                PopulateTextBoxesFromDictionary(floorElevations);
            }
        }

        private void AddLevel_Clicked(object sender, RoutedEventArgs e)
        {
            UIMethods.AddElevationInput(this.ElevationInputWrapper);
        }

        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            ElevationInput.floorElevations.Clear();

            foreach (Grid wrapper in this.ElevationInputWrapper.Children)
            {
                string floor = ((TextBox)wrapper.Children[1]).Text;
                string elevationText = ((TextBox)wrapper.Children[2]).Text;

                if (floor != string.Empty && elevationText != string.Empty)
                {
                    try
                    {
                        double elevation = Convert.ToDouble(elevationText);
                        ElevationInput.floorElevations[elevation] = floor;
                    }
                    catch
                    {
                        MessageBox.Show(floor + " was not added to the program because the input elevation is not a number, or the elevation is repetitive.");
                    }
                }
            }

            Methods.SaveDictionaryToDocumentStrings(ElevationInput.floorElevations);

            // Raise the custom event when the button is clicked
            ChangeSetFloorButtonColorRequest?.Invoke(this, EventArgs.Empty);

            this.Close();
        }

        private void PopulateTextBoxesFromDictionary(Dictionary<double, string> elevationInput)
        {
            int rowIndex = 0;

            foreach (var kvp in elevationInput)
            {
                TextBox textBoxRow1 = FindTextBoxByGridRowAndColumn(ElevationInputWrapper, rowIndex, 1);
                TextBox textBoxRow2 = FindTextBoxByGridRowAndColumn(ElevationInputWrapper, rowIndex, 2);

                if (textBoxRow1 != null && textBoxRow2 != null)
                {
                    textBoxRow1.Text = kvp.Value;
                    textBoxRow2.Text = kvp.Key.ToString();
                }
                else
                {
                    // Add a new row if necessary
                    AddRowToGrid(ElevationInputWrapper, kvp.Value, kvp.Key.ToString(), rowIndex);
                }

                rowIndex++;
            }
        }

        private TextBox FindTextBoxByGridRowAndColumn(Grid elevationInputWrapper, int rowIndex, int column)
        {
            foreach (UIElement grid in elevationInputWrapper.Children)
            {
                if (Grid.GetRow(grid) == rowIndex)
                {
                    Grid rowGrid = (Grid)grid;

                    foreach (UIElement uiElement in rowGrid.Children)
                    {
                        if (Grid.GetColumn(uiElement) == column && uiElement is TextBox textBox)
                        {
                            if (uiElement is TextBox)
                            {
                                return (TextBox)uiElement;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void AddRowToGrid(Grid elevationInputWrapper, string key, string value, int rowIndex)
        {
            Grid rowGrid = new Grid();
            rowGrid.Margin = new Thickness(0, 10, 0, 10);

            rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            rowGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });

            Label label = new Label();
            label.Content = rowIndex.ToString();
            label.FontSize = 16;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;

            TextBox textBox1 = new TextBox();
            textBox1.FontSize = 20;
            textBox1.Margin = new Thickness(0, 0, 10, 0);
            textBox1.Text = key.ToString();

            TextBox textBox2 = new TextBox();
            textBox2.FontSize = 20;
            textBox2.Margin = new Thickness(10, 0, 0, 0);
            textBox2.Text = value;

            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 0);

            Grid.SetColumn(textBox1, 1);
            Grid.SetRow(textBox1, 0);

            Grid.SetColumn(textBox2, 2);
            Grid.SetRow(textBox2, 0);

            rowGrid.Children.Add(label);
            rowGrid.Children.Add(textBox1);
            rowGrid.Children.Add(textBox2);

            elevationInputWrapper.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            Grid.SetRow(rowGrid, rowIndex);
            elevationInputWrapper.Children.Add(rowGrid);
        }
    }
}
