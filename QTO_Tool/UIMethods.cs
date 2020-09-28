using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace QTO_Tool
{
    class UIMethods
    {
        public static void GenerateLayerTemplate(Grid grid)
        {
            List<string> concreteTemplateNames = new List<string>() { "N/A", "Footing", "Continous Footing", "Slab", "Column", "Beam", "Wall", "Curb", "Styrofoam" };
            int layerCounter = 0;

            foreach (Rhino.DocObjects.Layer layer in RunQTO.doc.Layers)
            {
                if (layer.IsDeleted == false)
                {
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

                        grid.Children.Add(concreteTemplatesSelector);
                        Grid.SetColumn(concreteTemplatesSelector, 1);
                        Grid.SetRow(concreteTemplatesSelector, layerCounter);
                    }
                }

                layerCounter++;
            }
        }

        public static void GenerateDissipatedTableExpander(StackPanel stackPanel,
            string layerName, string templateType, List<object> layerTemplate, List<string> values,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            Expander layerEstimateExpander = new Expander();
            layerEstimateExpander.Name = "LayerEstimateExpader_";
            layerEstimateExpander.Header = layerName;
            layerEstimateExpander.FontWeight = FontWeights.DemiBold;
            layerEstimateExpander.Background = Brushes.DarkOrange;
            layerEstimateExpander.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#036fad"));

            /*--- The Grid for setting up the name of the department input---*/
            Grid layerEstimateGrid = new Grid();
            layerEstimateGrid.Margin = new Thickness(2, 5, 2, 0);
            layerEstimateGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f0f0f0"));

            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(1, GridUnitType.Star);
            layerEstimateGrid.RowDefinitions.Add(rowDef);

            for (int i = 0; i < values.Count; i++)
            {
                // Column Definition for Grids
                ColumnDefinition colDef = new ColumnDefinition();
                layerEstimateGrid.ColumnDefinitions.Add(colDef);

                Label quantityLabel = new Label();
                quantityLabel.Content = values[i];
                quantityLabel.FontSize = 20;
                quantityLabel.FontWeight = FontWeights.Bold;
                quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                layerEstimateGrid.Children.Add(quantityLabel);
                Grid.SetColumn(quantityLabel, i);
                Grid.SetRow(quantityLabel, 0);
            }

            int counter = 0;
            int valueFontSize = 18;

            foreach (object obj in layerTemplate)
            {
                //Dynamically adding Rows to the Grid
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                layerEstimateGrid.RowDefinitions.Add(rowDef);

                int count = layerTemplate.IndexOf(obj) + 1;

                if (templateType == "Slab")
                {
                    UIMethods.GenerateSlabTableExpander(obj, count, layerEstimateGrid,
                        valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Slab";
                    }
                }

                else if (templateType == "Footing")
                {
                    UIMethods.GenerateFootingTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Footing";
                    }
                }

                else if (templateType == "Column")
                {
                    UIMethods.GenerateColumnTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Column";
                    }
                }

                else if (templateType == "Beam")
                {
                    UIMethods.GenerateBeamTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Beam";
                    }
                }

                else if (templateType == "Wall")
                {
                    UIMethods.GenerateWallTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Wall";
                    }
                }

                else if (templateType == "Curb")
                {
                    UIMethods.GenerateCurbTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Curb";
                    }
                }

                else if (templateType == "ContinousFooting")
                {
                    UIMethods.GenerateContinousFootingTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "ContinousFooting";
                    }
                }

                else if (templateType == "Styrofoam")
                {
                    UIMethods.GenerateStyrofoamTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                        SelectObjectActivated, DeselectObjectActivated);

                    if (counter == 0)
                    {
                        layerEstimateExpander.Name += "Styrofoam";
                    }
                }

                counter++;
            }

            layerEstimateExpander.Content = layerEstimateGrid;

            stackPanel.Children.Add(layerEstimateExpander);
        }

        public static void GenerateCombinedTableExpander(StackPanel stackPanel, Dictionary<string,
            object> selectedTemplates, Dictionary<string, List<string>> values,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            Expander combinedEstimateExpander;

            /*--- The Grid for setting up the name of the department input---*/
            Grid combinedEstimateGrid;

            RowDefinition rowDef;

            //int counter = 0;
            int valueFontSize = 18;

            foreach (string template in selectedTemplates.Keys)
            {
                combinedEstimateExpander = new Expander();
                combinedEstimateExpander.Name = "CombinedEstimateExpader_";
                combinedEstimateExpander.FontWeight = FontWeights.DemiBold;
                combinedEstimateExpander.Background = Brushes.DarkOrange;
                combinedEstimateExpander.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#036fad"));

                combinedEstimateGrid = new Grid();
                combinedEstimateGrid.Margin = new Thickness(2, 5, 2, 0);
                combinedEstimateGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f0f0f0"));

                int count = 1;

                if (template == "Beam")
                {
                    combinedEstimateExpander.Header = "BEAMS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (BeamTemplate obj in (List<BeamTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateBeamTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Column")
                {
                    combinedEstimateExpander.Header = "COLUMNS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (ColumnTemplate obj in (List<ColumnTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateColumnTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Curb")
                {
                    combinedEstimateExpander.Header = "CURBS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (CurbTemplate obj in (List<CurbTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateCurbTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Continous Footing")
                {
                    combinedEstimateExpander.Header = "CONTINOUS FOOTINGS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (ContinousFootingTemplate obj in (List<ContinousFootingTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateContinousFootingTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Footing")
                {
                    combinedEstimateExpander.Header = "FOOTINGS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (FootingTemplate obj in (List<FootingTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateFootingTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Wall")
                {
                    combinedEstimateExpander.Header = "WALLS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (WallTemplate obj in (List<WallTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateWallTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Slab")
                {
                    combinedEstimateExpander.Header = "SLABS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (SlabTemplate obj in (List<SlabTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateSlabTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                if (template == "Styrofoam")
                {
                    combinedEstimateExpander.Header = "STYROFOAMS";

                    ColumnDefinition colDef;
                    Label quantityLabel;

                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    combinedEstimateGrid.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < values[template].Count; i++)
                    {
                        // Column Definition for Grids
                        colDef = new ColumnDefinition();
                        combinedEstimateGrid.ColumnDefinitions.Add(colDef);

                        quantityLabel = new Label();
                        quantityLabel.Content = values[template][i];
                        quantityLabel.FontSize = 20;
                        quantityLabel.FontWeight = FontWeights.Bold;
                        quantityLabel.Margin = new Thickness(0, 0, 2, 0);
                        quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;

                        combinedEstimateGrid.Children.Add(quantityLabel);
                        Grid.SetColumn(quantityLabel, i);
                        Grid.SetRow(quantityLabel, 0);
                    }

                    foreach (StyrofoamTemplate obj in (List<StyrofoamTemplate>)selectedTemplates[template])
                    {
                        rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                        combinedEstimateGrid.RowDefinitions.Add(rowDef);

                        UIMethods.GenerateStyrofoamTableExpander(obj, count, combinedEstimateGrid,
                            valueFontSize, SelectObjectActivated, DeselectObjectActivated);

                        count++;
                    }
                }

                combinedEstimateExpander.Content = combinedEstimateGrid;

                stackPanel.Children.Add(combinedEstimateExpander);
            }
        }

        static void GenerateSlabTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            SlabTemplate slab = (SlabTemplate)_obj;

            Label slabCount = new Label();
            slabCount.Content = _count;
            slabCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabCount);
            slabCount.FontSize = _valueFontSize;
            Grid.SetColumn(slabCount, 0);
            Grid.SetRow(slabCount, _count);

            Label slabName = new Label();
            slabName.Content = slab.name;
            slabName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabName);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabName, 1);
            Grid.SetRow(slabName, _count);

            Label slabGrossVolume = new Label();
            slabGrossVolume.Content = slab.grossVolume.ToString();
            slabGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabGrossVolume);
            slabGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabGrossVolume, 2);
            Grid.SetRow(slabGrossVolume, _count);

            Label slabNetVolume = new Label();
            slabNetVolume.Content = slab.netVolume.ToString();
            slabNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabNetVolume);
            slabNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabNetVolume, 3);
            Grid.SetRow(slabNetVolume, _count);

            Label slabTopArea = new Label();
            slabTopArea.Content = slab.topArea.ToString();
            slabTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabTopArea);
            slabTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabTopArea, 4);
            Grid.SetRow(slabTopArea, _count);

            Label slabBottomArea = new Label();
            slabBottomArea.Content = slab.bottomArea.ToString();
            slabBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabBottomArea);
            slabBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabBottomArea, 5);
            Grid.SetRow(slabBottomArea, _count);

            Label slabEdgeArea = new Label();
            slabEdgeArea.Content = slab.edgeArea.ToString();
            slabEdgeArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabEdgeArea);
            slabEdgeArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabEdgeArea, 6);
            Grid.SetRow(slabEdgeArea, _count);

            Label slabPerimeter = new Label();
            slabPerimeter.Content = slab.perimeter.ToString();
            slabPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabPerimeter);
            slabPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabPerimeter, 7);
            Grid.SetRow(slabPerimeter, _count);

            Label slabOpeningPerimeter = new Label();
            slabOpeningPerimeter.Content = slab.openingPerimeter.ToString();
            slabOpeningPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabOpeningPerimeter);
            slabOpeningPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabOpeningPerimeter, 8);
            Grid.SetRow(slabOpeningPerimeter, _count);

            ToggleButton slabSelectObject = new ToggleButton();
            slabSelectObject.Uid = slab.id;
            slabSelectObject.Content = "SELECT";
            slabSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            slabSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            slabSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            slabSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(slabSelectObject);
            slabSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(slabSelectObject, 9);
            Grid.SetRow(slabSelectObject, _count);
        }

        static void GenerateFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            FootingTemplate footing = (FootingTemplate)_obj;

            Label footingCount = new Label();
            footingCount.Content = _count;
            footingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingCount);
            footingCount.FontSize = _valueFontSize;
            Grid.SetColumn(footingCount, 0);
            Grid.SetRow(footingCount, _count);

            Label footingName = new Label();
            footingName.Content = footing.name;
            footingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingName);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingName, 1);
            Grid.SetRow(footingName, _count);

            Label footingVolume = new Label();
            footingVolume.Content = footing.volume.ToString();
            footingVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingVolume);
            footingVolume.FontSize = _valueFontSize;
            Grid.SetColumn(footingVolume, 2);
            Grid.SetRow(footingVolume, _count);

            Label footingTopArea = new Label();
            footingTopArea.Content = footing.topArea.ToString();
            footingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingTopArea);
            footingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingTopArea, 3);
            Grid.SetRow(footingTopArea, _count);

            Label footingBottomArea = new Label();
            footingBottomArea.Content = footing.bottomArea.ToString();
            footingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingBottomArea);
            footingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingBottomArea, 4);
            Grid.SetRow(footingBottomArea, _count);

            Label footingSideArea = new Label();
            footingSideArea.Content = footing.sideArea.ToString();
            footingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingSideArea);
            footingSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingSideArea, 5);
            Grid.SetRow(footingSideArea, _count);

            ToggleButton footingSelectObject = new ToggleButton();
            footingSelectObject.Uid = footing.id;
            footingSelectObject.Content = "SELECT";
            footingSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            footingSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            footingSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            footingSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(footingSelectObject);
            footingSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(footingSelectObject, 6);
            Grid.SetRow(footingSelectObject, _count);
        }

        static void GenerateColumnTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ColumnTemplate column = (ColumnTemplate)_obj;

            Label columnCount = new Label();
            columnCount.Content = _count;
            columnCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnCount);
            columnCount.FontSize = _valueFontSize;
            Grid.SetColumn(columnCount, 0);
            Grid.SetRow(columnCount, _count);

            Label columnName = new Label();
            columnName.Content = column.name;
            columnName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnName);
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnName, 1);
            Grid.SetRow(columnName, _count);

            Label columnVolume = new Label();
            columnVolume.Content = column.volume.ToString();
            columnVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnVolume);
            columnVolume.FontSize = _valueFontSize;
            Grid.SetColumn(columnVolume, 2);
            Grid.SetRow(columnVolume, _count);

            Label columnHeight = new Label();
            columnHeight.Content = column.height.ToString();
            columnHeight.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnHeight);
            columnHeight.FontSize = _valueFontSize;
            Grid.SetColumn(columnHeight, 3);
            Grid.SetRow(columnHeight, _count);

            Label columnSideArea = new Label();
            columnSideArea.Content = column.sideArea.ToString();
            columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnSideArea);
            columnSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(columnSideArea, 4);
            Grid.SetRow(columnSideArea, _count);

            ToggleButton columnSelectObject = new ToggleButton();
            columnSelectObject.Uid = column.id;
            columnSelectObject.Content = "SELECT";
            columnSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            columnSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            columnSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            columnSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(columnSelectObject);
            columnSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(columnSelectObject, 5);
            Grid.SetRow(columnSelectObject, _count);
        }

        static void GenerateBeamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            BeamTemplate beam = (BeamTemplate)_obj;

            Label beamCount = new Label();
            beamCount.Content = _count;
            beamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamCount);
            beamCount.FontSize = _valueFontSize;
            Grid.SetColumn(beamCount, 0);
            Grid.SetRow(beamCount, _count);

            Label beamName = new Label();
            beamName.Content = beam.name;
            beamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamName);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamName, 1);
            Grid.SetRow(beamName, _count);

            Label beamVolume = new Label();
            beamVolume.Content = beam.volume.ToString();
            beamVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamVolume);
            beamVolume.FontSize = _valueFontSize;
            Grid.SetColumn(beamVolume, 2);
            Grid.SetRow(beamVolume, _count);

            Label beamBottomArea = new Label();
            beamBottomArea.Content = beam.bottomArea.ToString();
            beamBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamBottomArea);
            beamBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamBottomArea, 3);
            Grid.SetRow(beamBottomArea, _count);

            Label beamSideArea = new Label();
            beamSideArea.Content = beam.sideArea.ToString();
            beamSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamSideArea);
            beamSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamSideArea, 4);
            Grid.SetRow(beamSideArea, _count);

            Label beamLength = new Label();
            beamLength.Content = beam.length.ToString();
            beamLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamLength);
            beamLength.FontSize = _valueFontSize;
            Grid.SetColumn(beamLength, 5);
            Grid.SetRow(beamLength, _count);

            ToggleButton beamSelectObject = new ToggleButton();
            beamSelectObject.Uid = beam.id;
            beamSelectObject.Content = "SELECT";
            beamSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            beamSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            beamSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            beamSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(beamSelectObject);
            beamSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(beamSelectObject, 6);
            Grid.SetRow(beamSelectObject, _count);
        }

        static void GenerateWallTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            WallTemplate wall = (WallTemplate)_obj;

            Label wallCount = new Label();
            wallCount.Content = _count;
            wallCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallCount);
            wallCount.FontSize = _valueFontSize;
            Grid.SetColumn(wallCount, 0);
            Grid.SetRow(wallCount, _count);

            Label wallName = new Label();
            wallName.Content = wall.name;
            wallName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallName);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallName, 1);
            Grid.SetRow(wallName, _count);

            Label wallGrossVolume = new Label();
            wallGrossVolume.Content = wall.grossVolume.ToString();
            wallGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallGrossVolume);
            wallGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallGrossVolume, 2);
            Grid.SetRow(wallGrossVolume, _count);

            Label wallNetVolume = new Label();
            wallNetVolume.Content = wall.netVolume.ToString();
            wallNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallNetVolume);
            wallNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallNetVolume, 3);
            Grid.SetRow(wallNetVolume, _count);

            Label wallTopArea = new Label();
            wallTopArea.Content = wall.topArea.ToString();
            wallTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallTopArea);
            wallTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallTopArea, 4);
            Grid.SetRow(wallTopArea, _count);

            Label wallEndArea = new Label();
            wallEndArea.Content = wall.endArea.ToString();
            wallEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallEndArea);
            wallEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallEndArea, 5);
            Grid.SetRow(wallEndArea, _count);

            Label wallSideArea_1 = new Label();
            wallSideArea_1.Content = wall.sideArea_1.ToString();
            wallSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_1);
            wallSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_1, 6);
            Grid.SetRow(wallSideArea_1, _count);

            Label wallSideArea_2 = new Label();
            wallSideArea_2.Content = wall.sideArea_2.ToString();
            wallSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_2);
            wallSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_2, 7);
            Grid.SetRow(wallSideArea_2, _count);

            Label wallLength = new Label();
            wallLength.Content = wall.length.ToString();
            wallLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallLength);
            wallLength.FontSize = _valueFontSize;
            Grid.SetColumn(wallLength, 8);
            Grid.SetRow(wallLength, _count);

            Label wallOpeningArea = new Label();
            wallOpeningArea.Content = wall.openingArea.ToString();
            wallOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallOpeningArea);
            wallOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallOpeningArea, 9);
            Grid.SetRow(wallOpeningArea, _count);

            ToggleButton wallSelectObject = new ToggleButton();
            wallSelectObject.Uid = wall.id;
            wallSelectObject.Content = "SELECT";
            wallSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            wallSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            wallSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            wallSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(wallSelectObject);
            wallSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(wallSelectObject, 10);
            Grid.SetRow(wallSelectObject, _count);
        }

        static void GenerateCurbTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            CurbTemplate curb = (CurbTemplate)_obj;

            Label curbCount = new Label();
            curbCount.Content = _count;
            curbCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbCount);
            curbCount.FontSize = _valueFontSize;
            Grid.SetColumn(curbCount, 0);
            Grid.SetRow(curbCount, _count);

            Label curbName = new Label();
            curbName.Content = curb.name;
            curbName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbName);
            curbName.FontSize = _valueFontSize;
            Grid.SetColumn(curbName, 1);
            Grid.SetRow(curbName, _count);

            Label curbVolume = new Label();
            curbVolume.Content = curb.volume.ToString();
            curbVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbVolume);
            curbVolume.FontSize = _valueFontSize;
            Grid.SetColumn(curbVolume, 2);
            Grid.SetRow(curbVolume, _count);

            Label curbTopArea = new Label();
            curbTopArea.Content = curb.topArea.ToString();
            curbTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbTopArea);
            curbTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbTopArea, 3);
            Grid.SetRow(curbTopArea, _count);

            Label curbSideArea = new Label();
            curbSideArea.Content = curb.sideArea.ToString();
            curbSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbSideArea);
            curbSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbSideArea, 4);
            Grid.SetRow(curbSideArea, _count);

            Label curbLength = new Label();
            curbLength.Content = curb.length.ToString();
            curbLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbLength);
            curbLength.FontSize = _valueFontSize;
            Grid.SetColumn(curbLength, 5);
            Grid.SetRow(curbLength, _count);

            ToggleButton curbSelectObject = new ToggleButton();
            curbSelectObject.Uid = curb.id;
            curbSelectObject.Content = "SELECT";
            curbSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            curbSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            curbSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            curbSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(curbSelectObject);
            curbSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(curbSelectObject, 6);
            Grid.SetRow(curbSelectObject, _count);
        }

        static void GenerateContinousFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ContinousFootingTemplate continousFooting = (ContinousFootingTemplate)_obj;

            Label continousFootingCount = new Label();
            continousFootingCount.Content = _count;
            continousFootingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingCount);
            continousFootingCount.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingCount, 0);
            Grid.SetRow(continousFootingCount, _count);

            Label continousFootingName = new Label();
            continousFootingName.Content = continousFooting.name;
            continousFootingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingName);
            continousFootingName.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingName, 1);
            Grid.SetRow(continousFootingName, _count);

            Label continousFootingVolume = new Label();
            continousFootingVolume.Content = continousFooting.volume.ToString();
            continousFootingVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingVolume);
            continousFootingVolume.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingVolume, 2);
            Grid.SetRow(continousFootingVolume, _count);

            Label continousFootingTopArea = new Label();
            continousFootingTopArea.Content = continousFooting.topArea.ToString();
            continousFootingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingTopArea);
            continousFootingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingTopArea, 3);
            Grid.SetRow(continousFootingTopArea, _count);

            Label continousFootingBottomArea = new Label();
            continousFootingBottomArea.Content = continousFooting.bottomArea.ToString();
            continousFootingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingBottomArea);
            continousFootingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingBottomArea, 4);
            Grid.SetRow(continousFootingBottomArea, _count);

            Label continousFootingSideArea = new Label();
            continousFootingSideArea.Content = continousFooting.sideArea.ToString();
            continousFootingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingSideArea);
            continousFootingSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingSideArea, 5);
            Grid.SetRow(continousFootingSideArea, _count);

            Label continousFootingLength = new Label();
            continousFootingLength.Content = continousFooting.length.ToString();
            continousFootingLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continousFootingLength);
            continousFootingLength.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingLength, 6);
            Grid.SetRow(continousFootingLength, _count);

            ToggleButton continousFootingSelectObject = new ToggleButton();
            continousFootingSelectObject.Uid = continousFooting.id;
            continousFootingSelectObject.Content = "SELECT";
            continousFootingSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            continousFootingSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            continousFootingSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            continousFootingSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(continousFootingSelectObject);
            continousFootingSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(continousFootingSelectObject, 7);
            Grid.SetRow(continousFootingSelectObject, _count);
        }

        static void GenerateStyrofoamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            StyrofoamTemplate styrofoam = (StyrofoamTemplate)_obj;

            Label styrofoamCount = new Label();
            styrofoamCount.Content = _count;
            styrofoamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamCount);
            styrofoamCount.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamCount, 0);
            Grid.SetRow(styrofoamCount, _count);

            Label styrofoamName = new Label();
            styrofoamName.Content = styrofoam.name;
            styrofoamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamName);
            styrofoamName.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamName, 1);
            Grid.SetRow(styrofoamName, _count);

            Label styrofoamVolume = new Label();
            styrofoamVolume.Content = styrofoam.volume.ToString();
            styrofoamVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamVolume);
            styrofoamVolume.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamVolume, 2);
            Grid.SetRow(styrofoamVolume, _count);

            ToggleButton styrofoamSelectObject = new ToggleButton();
            styrofoamSelectObject.Uid = styrofoam.id;
            styrofoamSelectObject.Content = "SELECT";
            styrofoamSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            styrofoamSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            styrofoamSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            styrofoamSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(styrofoamSelectObject);
            styrofoamSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamSelectObject, 3);
            Grid.SetRow(styrofoamSelectObject, _count);
        }
    }
}
