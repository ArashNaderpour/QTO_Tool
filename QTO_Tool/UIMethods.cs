using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QTO_Tool
{
    class UIMethods
    {
        public static void GenerateLayerTemplate(Grid grid)
        {
            List<string> concreteTemplateNames = new List<string>() { "N/A", "Footing", "Continous Footing", "Slab", "Column", "Beam", "Wall", "Curb", "Styrofoam" };
            int layerCounter = 0;

            foreach(Rhino.DocObjects.Layer layer in RunQTO.doc.Layers) { 

                if (!String.IsNullOrWhiteSpace(layer.Name))
                {
                   //Dynamically adding Rows to the Grid
                   RowDefinition rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(60, GridUnitType.Pixel);
                    grid.RowDefinitions.Add(rowDef);

                    // Layer Name
                    Label layerName = new Label();
                    layerName.Name = "Layer_" + layer.Index.ToString();
                    layerName.Content = RunQTO.doc.Layers[layer.Index].Name;
                    layerName.HorizontalAlignment = HorizontalAlignment.Left;
                    layerName.HorizontalContentAlignment = HorizontalAlignment.Center;
                    layerName.VerticalAlignment = VerticalAlignment.Center;
                    layerName.Margin = new Thickness(0, 5, 10, 0);

                    DockPanel panel = new DockPanel();
                    panel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    panel.VerticalAlignment = VerticalAlignment.Center;

                    Rectangle rect = new Rectangle();
                    rect.Fill = Brushes.Gray;
                    rect.Height = 1;
                    rect.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rect.Margin = new Thickness(10, 5, 10, 0);

                    panel.Children.Add(layerName);
                    panel.Children.Add(rect);

                    grid.Children.Add(panel);
                    Grid.SetColumn(panel, 0);
                    Grid.SetRow(panel, layerCounter);

                    ComboBox concreteTemplatesSelector = new ComboBox();
                    concreteTemplatesSelector.Name = "ConcreteTemplates_" + layer.Index.ToString();

                    foreach (string templateName in concreteTemplateNames)
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = templateName;
                        concreteTemplatesSelector.Items.Add(item);
                    }
                    concreteTemplatesSelector.SelectedIndex = 0;
                    concreteTemplatesSelector.HorizontalAlignment = HorizontalAlignment.Stretch;
                    concreteTemplatesSelector.VerticalAlignment = VerticalAlignment.Center;
                    concreteTemplatesSelector.Margin = new Thickness(10, 5, 0, 0);
                    //concreteTemplatesSelector.SelectionChanged += ComboBox_SelectionChanged;

                    grid.Children.Add(concreteTemplatesSelector);
                    Grid.SetColumn(concreteTemplatesSelector, 1);
                    Grid.SetRow(concreteTemplatesSelector, layerCounter);

                    layerCounter++;
                }
            }
        }
    }
}
