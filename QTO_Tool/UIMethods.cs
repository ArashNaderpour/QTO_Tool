using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QTO_Tool
{
    class UIMethods
    {
        public static void GenerateLayerTemplate(Grid grid, List<string> layerPropertyColumnHeaders)
        {
            List<string> concreteTemplateNames = new List<string>() { "N/A", "Footing", "Continuous Footing", "Slab", "Column", "Non-Rectangular Column", "Beam", "Wall", "Curb", "Stair", "Styrofoam" };
            int layerCounter = 0;

            layerPropertyColumnHeaders.Add("C1");
            layerPropertyColumnHeaders.Add("C2");

            foreach (Rhino.DocObjects.Layer layer in RunQTO.doc.Layers)
            {
                if (layer.IsDeleted == false)
                {
                    if (!String.IsNullOrWhiteSpace(layer.Name))
                    {
                        int layerNameValueLength = layer.Name.Split('_').Length;
                        if (layerNameValueLength > layerPropertyColumnHeaders.Count)
                        {
                            for (int i = layerPropertyColumnHeaders.Count; i < layerNameValueLength; i++)
                            {
                                layerPropertyColumnHeaders.Add("C" + (i+1));
                            }
                        }

                        //Dynamically adding Rows to the Grid
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(60, GridUnitType.Pixel);
                        grid.RowDefinitions.Add(rowDef);

                        // Layer Name
                        TextBlock layerName = new TextBlock();
                        layerName.Name = "Layer_" + layer.Index.ToString();
                        layerName.Text = layer.Name;
                        layerName.HorizontalAlignment = HorizontalAlignment.Left;
                        layerName.TextAlignment = TextAlignment.Center;
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

                        concreteTemplatesSelector.SelectedIndex = Methods.AutomaticTemplateSelect(layer.Name, concreteTemplateNames); ;
                        concreteTemplatesSelector.HorizontalAlignment = HorizontalAlignment.Stretch;
                        concreteTemplatesSelector.VerticalAlignment = VerticalAlignment.Center;
                        concreteTemplatesSelector.Margin = new Thickness(10, 5, 0, 0);

                        grid.Children.Add(concreteTemplatesSelector);
                        Grid.SetColumn(concreteTemplatesSelector, 1);
                        Grid.SetRow(concreteTemplatesSelector, layerCounter);
                    }

                    layerCounter++;
                }
            }
        }

        public static void GenerateConcreteTableExpander(StackPanel stackPanel,
            string layerName, string templateType, Dictionary<string, List<object>> layerTemplate, List<string> values, List<string> layerPropertyColumnHeaders,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            foreach (KeyValuePair<string, List<object>> entry in layerTemplate)
            {
                Expander layerEstimateExpander = new Expander();
                layerEstimateExpander.Name = "LayerEstimateExpader_";
                TextBlock expanderHeader = new TextBlock();
                expanderHeader.Text = entry.Key;
                expanderHeader.Foreground = Brushes.Black;
                layerEstimateExpander.Header = expanderHeader;
                layerEstimateExpander.FontWeight = FontWeights.DemiBold;
                layerEstimateExpander.Background = Brushes.DarkOrange;
                layerEstimateExpander.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#036fad"));

                /*--- The Grid for setting up the name of the department input---*/
                Grid layerEstimateGrid = new Grid();
                layerEstimateGrid.Margin = new Thickness(2, 5, 2, 0);
                layerEstimateGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f0f0f0"));

                TextBlock quantityName;

                RowDefinition rowDef = new RowDefinition();
                layerEstimateGrid.RowDefinitions.Add(rowDef);

                for (int i = 0; i < values.Count; i++)
                {
                    // Column Definition for Grids
                    ColumnDefinition colDef = new ColumnDefinition();
                    layerEstimateGrid.ColumnDefinitions.Add(colDef);

                    quantityName = new TextBlock();
                    quantityName.Text = values[i];
                    quantityName.FontSize = 20;
                    quantityName.Foreground = Brushes.Black;
                    quantityName.FontWeight = FontWeights.Bold;
                    quantityName.Margin = new Thickness(0, 0, 2, 0);
                    quantityName.HorizontalAlignment = HorizontalAlignment.Center;

                    layerEstimateGrid.Children.Add(quantityName);
                    Grid.SetColumn(quantityName, i);
                    Grid.SetRow(quantityName, 0);
                }

                int counter = 0;
                int valueFontSize = 18;

                foreach (object obj in entry.Value)
                {
                    //Dynamically adding Rows to the Grid
                    rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    layerEstimateGrid.RowDefinitions.Add(rowDef);

                    int count = entry.Value.IndexOf(obj) + 1;

                    if (templateType == "Slab")
                    {
                        UIMethods.GenerateSlabTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Slab";
                        }
                    }

                    else if (templateType == "Footing")
                    {
                        UIMethods.GenerateFootingTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Footing";
                        }
                    }

                    else if (templateType.Contains("Column"))
                    {
                        UIMethods.GenerateColumnTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Column";
                        }
                    }

                    else if (templateType == "Beam")
                    {
                        UIMethods.GenerateBeamTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Beam";
                        }
                    }

                    else if (templateType == "Wall")
                    {
                        UIMethods.GenerateWallTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Wall";
                        }
                    }

                    else if (templateType == "Curb")
                    {
                        UIMethods.GenerateCurbTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Curb";
                        }
                    }

                    else if (templateType == "ContinuousFooting")
                    {
                        UIMethods.GenerateContinuousFootingTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "ContinuousFooting";
                        }
                    }

                    else if (templateType == "Styrofoam")
                    {
                        UIMethods.GenerateStyrofoamTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Styrofoam";
                        }
                    }

                    else if (templateType == "Stair")
                    {
                        UIMethods.GenerateStairTableExpander(obj, count, layerEstimateGrid, valueFontSize,
                            layerPropertyColumnHeaders, SelectObjectActivated, DeselectObjectActivated);

                        if (counter == 0)
                        {
                            layerEstimateExpander.Name += "Stair";
                        }
                    }

                    counter++;
                }

                layerEstimateExpander.Content = layerEstimateGrid;

                stackPanel.Children.Add(layerEstimateExpander);
            }
        }

        static void GenerateSlabTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            SlabTemplate slab = (SlabTemplate)_obj;

            int columnIndex = 0;

            TextBlock slabCount = new TextBlock();
            slabCount.Text = _count.ToString();
            slabCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabCount);
            slabCount.FontSize = _valueFontSize;
            Grid.SetColumn(slabCount, columnIndex);
            Grid.SetRow(slabCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = slab.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock slabName = new TextBlock();
            slabName.Text = slab.nameAbb;
            slabName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabName);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = slab.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock slabGrossVolume = new TextBlock();
            slabGrossVolume.Text = slab.grossVolume.ToString();
            slabGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabGrossVolume);
            slabGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabGrossVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabGrossVolume, _count);
            columnIndex++;

            TextBlock slabNetVolume = new TextBlock();
            slabNetVolume.Uid = slab.id + "_NetVolume";
            slabNetVolume.Text = slab.netVolume.ToString();
            slabNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabNetVolume);
            slabNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabNetVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabNetVolume, _count);
            columnIndex++;

            TextBlock slabTopArea = new TextBlock();
            slabTopArea.Text = slab.topArea.ToString();
            slabTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabTopArea);
            slabTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabTopArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabTopArea, _count);
            columnIndex++;

            TextBlock slabBottomArea = new TextBlock();
            slabBottomArea.Uid = slab.id + "_BottomArea";
            slabBottomArea.Text = slab.bottomArea.ToString();
            slabBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabBottomArea);
            slabBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabBottomArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabBottomArea, _count);
            columnIndex++;

            TextBlock slabEdgeArea = new TextBlock();
            slabEdgeArea.Text = slab.edgeArea.ToString();
            slabEdgeArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabEdgeArea);
            slabEdgeArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabEdgeArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabEdgeArea, _count);
            columnIndex++;

            TextBlock slabPerimeter = new TextBlock();
            slabPerimeter.Text = slab.perimeter.ToString();
            slabPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabPerimeter);
            slabPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabPerimeter, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabPerimeter, _count);
            columnIndex++;

            TextBlock slabOpeningPerimeter = new TextBlock();
            slabOpeningPerimeter.Text = slab.openingPerimeter.ToString();
            slabOpeningPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabOpeningPerimeter);
            slabOpeningPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabOpeningPerimeter, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabOpeningPerimeter, _count);
            columnIndex++;

            ToggleButton slabSelectObject = new ToggleButton();
            slabSelectObject.Uid = slab.id;
            slabSelectObject.Content = "SELECT";
            slabSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            slabSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            slabSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            slabSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(slabSelectObject);
            slabSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(slabSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabSelectObject, _count);
        }

        static void GenerateFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            FootingTemplate footing = (FootingTemplate)_obj;

            int columnIndex = 0;

            TextBlock footingCount = new TextBlock();
            footingCount.Text = _count.ToString();
            footingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingCount);
            footingCount.FontSize = _valueFontSize;
            Grid.SetColumn(footingCount, columnIndex);
            Grid.SetRow(footingCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = footing.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock footingName = new TextBlock();
            footingName.Text = footing.nameAbb;
            footingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingName);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = footing.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock footingVolume = new TextBlock();
            footingVolume.Text = footing.volume.ToString();
            footingVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingVolume);
            footingVolume.FontSize = _valueFontSize;
            Grid.SetColumn(footingVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingVolume, _count);
            columnIndex++;

            TextBlock footingTopArea = new TextBlock();
            footingTopArea.Text = footing.topArea.ToString();
            footingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingTopArea);
            footingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingTopArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingTopArea, _count);
            columnIndex++;

            TextBlock footingBottomArea = new TextBlock();
            footingBottomArea.Text = footing.bottomArea.ToString();
            footingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingBottomArea);
            footingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingBottomArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingBottomArea, _count);
            columnIndex++;

            TextBlock footingSideArea = new TextBlock();
            footingSideArea.Text = footing.sideArea.ToString();
            footingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingSideArea);
            footingSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingSideArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingSideArea, _count);
            columnIndex++;

            ToggleButton footingSelectObject = new ToggleButton();
            footingSelectObject.Uid = footing.id;
            footingSelectObject.Content = "SELECT";
            footingSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            footingSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            footingSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            footingSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(footingSelectObject);
            footingSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(footingSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingSelectObject, _count);
        }

        static void GenerateColumnTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ColumnTemplate column = (ColumnTemplate)_obj;

            int columnIndex = 0;

            TextBlock columnCount = new TextBlock();
            columnCount.Text = _count.ToString();
            columnCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnCount);
            columnCount.FontSize = _valueFontSize;
            Grid.SetColumn(columnCount, columnIndex);
            Grid.SetRow(columnCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = column.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock columnName = new TextBlock();
            columnName.Text = column.nameAbb;
            columnName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnName);
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = column.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock columnVolume = new TextBlock();
            columnVolume.Text = column.volume.ToString();
            columnVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnVolume);
            columnVolume.FontSize = _valueFontSize;
            Grid.SetColumn(columnVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnVolume, _count);
            columnIndex++;

            TextBlock columnHeight = new TextBlock();
            columnHeight.Text = column.height.ToString();
            columnHeight.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnHeight);
            columnHeight.FontSize = _valueFontSize;
            Grid.SetColumn(columnHeight, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnHeight, _count);
            columnIndex++;

            TextBlock columnSideArea = new TextBlock();

            if (column.rectangular)
            {
                columnSideArea.Text = column.sideArea.ToString();
                columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
                _layerEstimateGrid.Children.Add(columnSideArea);
                columnSideArea.FontSize = _valueFontSize;
                Grid.SetColumn(columnSideArea, columnIndex + _layerPropertyColumnHeaders.Count);
                Grid.SetRow(columnSideArea, _count);
                columnIndex++;
            }
            else
            {
                columnSideArea.Text = "N/A";
                columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
                _layerEstimateGrid.Children.Add(columnSideArea);
                columnSideArea.FontSize = _valueFontSize;
                Grid.SetColumn(columnSideArea, columnIndex + _layerPropertyColumnHeaders.Count);
                Grid.SetRow(columnSideArea, _count);
                columnIndex++;
            }

            ToggleButton columnSelectObject = new ToggleButton();
            columnSelectObject.Uid = column.id;
            columnSelectObject.Content = "SELECT";
            columnSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            columnSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            columnSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            columnSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(columnSelectObject);
            columnSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(columnSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnSelectObject, _count);
        }

        static void GenerateBeamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            BeamTemplate beam = (BeamTemplate)_obj;

            int columnIndex = 0;

            TextBlock beamCount = new TextBlock();
            beamCount.Text = _count.ToString();
            beamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamCount);
            beamCount.FontSize = _valueFontSize;
            Grid.SetColumn(beamCount, columnIndex);
            Grid.SetRow(beamCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = beam.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock beamName = new TextBlock();
            beamName.Text = beam.nameAbb;
            beamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamName);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = beam.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock beamGrossVolume = new TextBlock();
            beamGrossVolume.Text = beam.grossVolume.ToString();
            beamGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamGrossVolume);
            beamGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(beamGrossVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamGrossVolume, _count);
            columnIndex++;

            TextBlock beamNetVolume = new TextBlock();
            beamNetVolume.Text = beam.netVolume.ToString();
            beamNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamNetVolume);
            beamNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(beamNetVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamNetVolume, _count);
            columnIndex++;

            TextBlock beamBottomArea = new TextBlock();
            beamBottomArea.Text = beam.bottomArea.ToString();
            beamBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamBottomArea);
            beamBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamBottomArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamBottomArea, _count);
            columnIndex++;

            TextBlock beamEndArea = new TextBlock();
            beamEndArea.Text = beam.endArea.ToString();
            beamEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamEndArea);
            beamEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamEndArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamEndArea, _count);
            columnIndex++;

            TextBlock beamSideArea_1 = new TextBlock();
            beamSideArea_1.Text = beam.sideArea_1.ToString();
            beamSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamSideArea_1);
            beamSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(beamSideArea_1, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamSideArea_1, _count);
            columnIndex++;

            TextBlock beamSideArea_2 = new TextBlock();
            beamSideArea_2.Text = beam.sideArea_2.ToString();
            beamSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamSideArea_2);
            beamSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(beamSideArea_2, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamSideArea_2, _count);
            columnIndex++;

            TextBlock beamLength = new TextBlock();
            beamLength.Text = beam.length.ToString();
            beamLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamLength);
            beamLength.FontSize = _valueFontSize;
            Grid.SetColumn(beamLength, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamLength, _count);
            columnIndex++;

            TextBlock beamOpeningArea = new TextBlock();
            beamOpeningArea.Text = beam.openingArea.ToString();
            beamOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamOpeningArea);
            beamOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamOpeningArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamOpeningArea, _count);
            columnIndex++;

            ToggleButton beamSelectObject = new ToggleButton();
            beamSelectObject.Uid = beam.id;
            beamSelectObject.Content = "SELECT";
            beamSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            beamSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            beamSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            beamSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(beamSelectObject);
            beamSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(beamSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamSelectObject, _count);
        }

        static void GenerateWallTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            WallTemplate wall = (WallTemplate)_obj;

            int columnIndex = 0;

            TextBlock wallCount = new TextBlock();
            wallCount.Text = _count.ToString();
            wallCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallCount);
            wallCount.FontSize = _valueFontSize;
            Grid.SetColumn(wallCount, columnIndex);
            Grid.SetRow(wallCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = wall.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock wallName = new TextBlock();
            wallName.Text = wall.nameAbb;
            wallName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallName);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = wall.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock wallGrossVolume = new TextBlock();
            wallGrossVolume.Text = wall.grossVolume.ToString();
            wallGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallGrossVolume);
            wallGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallGrossVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallGrossVolume, _count);
            columnIndex++;

            TextBlock wallNetVolume = new TextBlock();
            wallNetVolume.Text = wall.netVolume.ToString();
            wallNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallNetVolume);
            wallNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallNetVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallNetVolume, _count);
            columnIndex++;

            TextBlock wallTopArea = new TextBlock();
            wallTopArea.Text = wall.topArea.ToString();
            wallTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallTopArea);
            wallTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallTopArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallTopArea, _count);
            columnIndex++;

            TextBlock wallEndArea = new TextBlock();
            wallEndArea.Text = wall.endArea.ToString();
            wallEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallEndArea);
            wallEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallEndArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallEndArea, _count);
            columnIndex++;

            TextBlock wallSideArea_1 = new TextBlock();
            wallSideArea_1.Text = wall.sideArea_1.ToString();
            wallSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_1);
            wallSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_1, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSideArea_1, _count);
            columnIndex++;

            TextBlock wallSideArea_2 = new TextBlock();
            wallSideArea_2.Text = wall.sideArea_2.ToString();
            wallSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_2);
            wallSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_2, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSideArea_2, _count);
            columnIndex++;

            TextBlock wallLength = new TextBlock();
            wallLength.Text = wall.length.ToString();
            wallLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallLength);
            wallLength.FontSize = _valueFontSize;
            Grid.SetColumn(wallLength, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallLength, _count);
            columnIndex++;

            TextBlock wallOpeningArea = new TextBlock();
            wallOpeningArea.Text = wall.openingArea.ToString();
            wallOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallOpeningArea);
            wallOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallOpeningArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallOpeningArea, _count);
            columnIndex++;

            ToggleButton wallSelectObject = new ToggleButton();
            wallSelectObject.Uid = wall.id;
            wallSelectObject.Content = "SELECT";
            wallSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            wallSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            wallSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            wallSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(wallSelectObject);
            wallSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(wallSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSelectObject, _count);
        }

        static void GenerateCurbTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            CurbTemplate curb = (CurbTemplate)_obj;

            int columnIndex = 0;

            TextBlock curbCount = new TextBlock();
            curbCount.Text = _count.ToString();
            curbCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbCount);
            curbCount.FontSize = _valueFontSize;
            Grid.SetColumn(curbCount, columnIndex);
            Grid.SetRow(curbCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = curb.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock curbName = new TextBlock();
            curbName.Text = curb.nameAbb;
            curbName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbName);
            curbName.FontSize = _valueFontSize;
            Grid.SetColumn(curbName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = curb.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock curbGrossVolume = new TextBlock();
            curbGrossVolume.Text = curb.grossVolume.ToString();
            curbGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbGrossVolume);
            curbGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(curbGrossVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbGrossVolume, _count);
            columnIndex++;

            TextBlock curbNetVolume = new TextBlock();
            curbNetVolume.Text = curb.netVolume.ToString();
            curbNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbNetVolume);
            curbNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(curbNetVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbNetVolume, _count);
            columnIndex++;

            TextBlock curbTopArea = new TextBlock();
            curbTopArea.Text = curb.topArea.ToString();
            curbTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbTopArea);
            curbTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbTopArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbTopArea, _count);
            columnIndex++;

            TextBlock curbEndArea = new TextBlock();
            curbEndArea.Text = curb.endArea.ToString();
            curbEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbEndArea);
            curbEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbEndArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbEndArea, _count);
            columnIndex++;

            TextBlock curbSideArea_1 = new TextBlock();
            curbSideArea_1.Text = curb.sideArea_1.ToString();
            curbSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbSideArea_1);
            curbSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(curbSideArea_1, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSideArea_1, _count);
            columnIndex++;

            TextBlock curbSideArea_2 = new TextBlock();
            curbSideArea_2.Text = curb.sideArea_2.ToString();
            curbSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbSideArea_2);
            curbSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(curbSideArea_2, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSideArea_2, _count);
            columnIndex++;

            TextBlock curbLength = new TextBlock();
            curbLength.Text = curb.length.ToString();
            curbLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbLength);
            curbLength.FontSize = _valueFontSize;
            Grid.SetColumn(curbLength, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbLength, _count);
            columnIndex++;

            TextBlock curbOpeningArea = new TextBlock();
            curbOpeningArea.Text = curb.openingArea.ToString();
            curbOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbOpeningArea);
            curbOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbOpeningArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbOpeningArea, _count);
            columnIndex++;

            ToggleButton curbSelectObject = new ToggleButton();
            curbSelectObject.Uid = curb.id;
            curbSelectObject.Content = "SELECT";
            curbSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            curbSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            curbSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            curbSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(curbSelectObject);
            curbSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(curbSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSelectObject, _count);
        }

        static void GenerateContinuousFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ContinuousFootingTemplate continuousFooting = (ContinuousFootingTemplate)_obj;

            int columnIndex = 0;

            TextBlock continuousFootingCount = new TextBlock();
            continuousFootingCount.Text = _count.ToString();
            continuousFootingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingCount);
            continuousFootingCount.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingCount, columnIndex);
            Grid.SetRow(continuousFootingCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = continuousFooting.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock continuousFootingName = new TextBlock();
            continuousFootingName.Text = continuousFooting.nameAbb;
            continuousFootingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingName);
            continuousFootingName.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = continuousFooting.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock continuousFootingGrossVolume = new TextBlock();
            continuousFootingGrossVolume.Text = continuousFooting.grossVolume.ToString();
            continuousFootingGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingGrossVolume);
            continuousFootingGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingGrossVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingGrossVolume, _count);
            columnIndex++;

            TextBlock continuousFootingNetVolume = new TextBlock();
            continuousFootingNetVolume.Text = continuousFooting.netVolume.ToString();
            continuousFootingNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingNetVolume);
            continuousFootingNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingNetVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingNetVolume, _count);
            columnIndex++;

            TextBlock continuousFootingTopArea = new TextBlock();
            continuousFootingTopArea.Text = continuousFooting.topArea.ToString();
            continuousFootingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingTopArea);
            continuousFootingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingTopArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingTopArea, _count);
            columnIndex++;

            TextBlock continuousFootingBottomArea = new TextBlock();
            continuousFootingBottomArea.Text = continuousFooting.bottomArea.ToString();
            continuousFootingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingBottomArea);
            continuousFootingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingBottomArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingBottomArea, _count);
            columnIndex++;

            TextBlock continuousFootingEndArea = new TextBlock();
            continuousFootingEndArea.Text = continuousFooting.endArea.ToString();
            continuousFootingEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingEndArea);
            continuousFootingEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingEndArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingEndArea, _count);
            columnIndex++;

            TextBlock continuousFootingSideArea_1 = new TextBlock();
            continuousFootingSideArea_1.Text = continuousFooting.sideArea_1.ToString();
            continuousFootingSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingSideArea_1);
            continuousFootingSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingSideArea_1, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingSideArea_1, _count);
            columnIndex++;

            TextBlock continuousFootingSideArea_2 = new TextBlock();
            continuousFootingSideArea_2.Text = continuousFooting.sideArea_2.ToString();
            continuousFootingSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingSideArea_2);
            continuousFootingSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingSideArea_2, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingSideArea_2, _count);
            columnIndex++;

            TextBlock continuousFootingLength = new TextBlock();
            continuousFootingLength.Text = continuousFooting.length.ToString();
            continuousFootingLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingLength);
            continuousFootingLength.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingLength, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingLength, _count);
            columnIndex++;

            TextBlock continuousFootingOpeningArea = new TextBlock();
            continuousFootingOpeningArea.Text = continuousFooting.openingArea.ToString();
            continuousFootingOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingOpeningArea);
            continuousFootingOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingOpeningArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingOpeningArea, _count);
            columnIndex++;

            ToggleButton continuousFootingSelectObject = new ToggleButton();
            continuousFootingSelectObject.Uid = continuousFooting.id;
            continuousFootingSelectObject.Content = "SELECT";
            continuousFootingSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            continuousFootingSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            continuousFootingSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            continuousFootingSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(continuousFootingSelectObject);
            continuousFootingSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingSelectObject, _count);
        }

        static void GenerateStyrofoamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            StyrofoamTemplate styrofoam = (StyrofoamTemplate)_obj;

            int columnIndex = 0;

            TextBlock styrofoamCount = new TextBlock();
            styrofoamCount.Text = _count.ToString();
            styrofoamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamCount);
            styrofoamCount.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamCount, columnIndex);
            Grid.SetRow(styrofoamCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = styrofoam.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock styrofoamName = new TextBlock();
            styrofoamName.Text = styrofoam.nameAbb;
            styrofoamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamName);
            styrofoamName.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(styrofoamName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = styrofoam.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock styrofoamVolume = new TextBlock();
            styrofoamVolume.Text = styrofoam.volume.ToString();
            styrofoamVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamVolume);
            styrofoamVolume.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(styrofoamVolume, _count);
            columnIndex++;

            ToggleButton styrofoamSelectObject = new ToggleButton();
            styrofoamSelectObject.Uid = styrofoam.id;
            styrofoamSelectObject.Content = "SELECT";
            styrofoamSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            styrofoamSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            styrofoamSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            styrofoamSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(styrofoamSelectObject);
            styrofoamSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(styrofoamSelectObject, _count);
        }

        static void GenerateStairTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
    List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            StairTemplate stair = (StairTemplate)_obj;

            int columnIndex = 0;

            TextBlock stairCount = new TextBlock();
            stairCount.Text = _count.ToString();
            stairCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairCount);
            stairCount.FontSize = _valueFontSize;
            Grid.SetColumn(stairCount, columnIndex);
            Grid.SetRow(stairCount, _count);
            columnIndex++;

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = stair.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, columnIndex + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock stairName = new TextBlock();
            stairName.Text = stair.nameAbb;
            stairName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairName);
            stairName.FontSize = _valueFontSize;
            Grid.SetColumn(stairName, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairName, _count);
            columnIndex++;

            TextBlock floor = new TextBlock();
            floor.Text = stair.floor;
            floor.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(floor);
            floor.FontSize = _valueFontSize;
            Grid.SetColumn(floor, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(floor, _count);
            columnIndex++;

            TextBlock stairVolume = new TextBlock();
            stairVolume.Text = stair.volume.ToString();
            stairVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairVolume);
            stairVolume.FontSize = _valueFontSize;
            Grid.SetColumn(stairVolume, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairVolume, _count);
            columnIndex++;

            TextBlock stairTreadArea = new TextBlock();
            stairTreadArea.Text = stair.treadArea.ToString();
            stairTreadArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairTreadArea);
            stairTreadArea.FontSize = _valueFontSize;
            Grid.SetColumn(stairTreadArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairTreadArea, _count);
            columnIndex++;

            TextBlock stairRiserArea = new TextBlock();
            stairRiserArea.Text = stair.riserArea.ToString();
            stairRiserArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairRiserArea);
            stairRiserArea.FontSize = _valueFontSize;
            Grid.SetColumn(stairRiserArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairRiserArea, _count);
            columnIndex++;

            TextBlock stairTreadCount = new TextBlock();
            stairTreadCount.Text = stair.treadCount.ToString();
            stairTreadCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairTreadCount);
            stairTreadCount.FontSize = _valueFontSize;
            Grid.SetColumn(stairTreadCount, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairTreadCount, _count);
            columnIndex++;

            TextBlock stairSideArea = new TextBlock();
            stairSideArea.Text = stair.sideArea.ToString();
            stairSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairSideArea);
            stairSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(stairSideArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairSideArea, _count);
            columnIndex++;

            TextBlock stairBottomArea = new TextBlock();
            stairBottomArea.Text = stair.bottomArea.ToString();
            stairBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(stairBottomArea);
            stairBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(stairBottomArea, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairBottomArea, _count);
            columnIndex++;

            ToggleButton stairSelectObject = new ToggleButton();
            stairSelectObject.Uid = stair.id;
            stairSelectObject.Content = "SELECT";
            stairSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            stairSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            stairSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            stairSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(stairSelectObject);
            stairSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(stairSelectObject, columnIndex + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(stairSelectObject, _count);
        }

        public static void AddElevationInput(Grid elevationInput)
        {
            int gridRow = elevationInput.RowDefinitions.Count;
            double fontSize = 20;

            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(50);
            elevationInput.RowDefinitions.Add(rowDef);

            Grid inputWrapper = new Grid();
            Grid.SetRow(inputWrapper, gridRow);
            inputWrapper.Margin = new Thickness(0, 10, 0, 10);

            ColumnDefinition colDef1 = new ColumnDefinition();
            colDef1.Width = new GridLength(50);
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();

            inputWrapper.ColumnDefinitions.Add(colDef1);
            inputWrapper.ColumnDefinitions.Add(colDef2);
            inputWrapper.ColumnDefinitions.Add(colDef3);

            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(30);

            inputWrapper.RowDefinitions.Add(rowDef);

            Label label = new Label();
            label.Content = gridRow.ToString() + ".";
            label.FontSize = 16;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(label, 0);

            // Add the first text cell to the Grid
            TextBox input1 = new TextBox();
            input1.FontSize = fontSize;
            input1.Margin = new Thickness(0, 0, 10, 0);
            Grid.SetColumn(input1, 1);

            // Add the first text cell to the Grid
            TextBox input2 = new TextBox();
            input2.FontSize = fontSize;
            input2.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(input2, 2);

            inputWrapper.Children.Add(label);
            inputWrapper.Children.Add(input1);
            inputWrapper.Children.Add(input2);

            elevationInput.Children.Add(inputWrapper);
        }
    }
}
