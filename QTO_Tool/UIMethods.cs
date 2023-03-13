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
            List<string> concreteTemplateNames = new List<string>() { "N/A", "Footing", "Continuous Footing", "Slab", "Column", "Non-Rectangular Column", "Beam", "Wall", "Curb", "Styrofoam" };
            int layerCounter = 0;

            layerPropertyColumnHeaders.Add("C1");
            layerPropertyColumnHeaders.Add("C2");
            layerPropertyColumnHeaders.Add("C3");

            foreach (Rhino.DocObjects.Layer layer in RunQTO.doc.Layers)
            {
                if (layer.IsDeleted == false)
                {
                    if (!String.IsNullOrWhiteSpace(layer.Name))
                    {
                        if (layer.Name.Split('_').Length > layerPropertyColumnHeaders.Count - 1)
                        {
                            layerPropertyColumnHeaders.Add("C" + (layerPropertyColumnHeaders.Count + 1).ToString());
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
            string layerName, string templateType, List<object> layerTemplate, List<string> values, List<string> layerPropertyColumnHeaders,
            RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            Expander layerEstimateExpander = new Expander();
            layerEstimateExpander.Name = "LayerEstimateExpader_";
            TextBlock expanderHeader = new TextBlock();
            expanderHeader.Text = layerName;
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

            foreach (object obj in layerTemplate)
            {
                //Dynamically adding Rows to the Grid
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                layerEstimateGrid.RowDefinitions.Add(rowDef);

                int count = layerTemplate.IndexOf(obj) + 1;

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

                counter++;
            }

            layerEstimateExpander.Content = layerEstimateGrid;

            stackPanel.Children.Add(layerEstimateExpander);
        }

        static void GenerateSlabTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            SlabTemplate slab = (SlabTemplate)_obj;

            TextBlock slabCount = new TextBlock();
            slabCount.Text = _count.ToString();
            slabCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabCount);
            slabCount.FontSize = _valueFontSize;
            Grid.SetColumn(slabCount, 0);
            Grid.SetRow(slabCount, _count);

            for(int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = slab.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock slabName = new TextBlock();
            slabName.Text = slab.nameAbb;
            slabName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabName);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabName, _count);

            TextBlock slabGrossVolume = new TextBlock();
            slabGrossVolume.Text = slab.grossVolume.ToString();
            slabGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabGrossVolume);
            slabGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabGrossVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabGrossVolume, _count);

            TextBlock slabNetVolume = new TextBlock();
            slabNetVolume.Uid = slab.id + "_NetVolume";
            slabNetVolume.Text = slab.netVolume.ToString();
            slabNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabNetVolume);
            slabNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(slabNetVolume, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabNetVolume, _count);

            TextBlock slabTopArea = new TextBlock();
            slabTopArea.Text = slab.topArea.ToString();
            slabTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabTopArea);
            slabTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabTopArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabTopArea, _count);

            TextBlock slabBottomArea = new TextBlock();
            slabBottomArea.Uid = slab.id + "_BottomArea";
            slabBottomArea.Text = slab.bottomArea.ToString();
            slabBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabBottomArea);
            slabBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabBottomArea, 5 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabBottomArea, _count);

            TextBlock slabEdgeArea = new TextBlock();
            slabEdgeArea.Text = slab.edgeArea.ToString();
            slabEdgeArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabEdgeArea);
            slabEdgeArea.FontSize = _valueFontSize;
            Grid.SetColumn(slabEdgeArea, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabEdgeArea, _count);

            TextBlock slabPerimeter = new TextBlock();
            slabPerimeter.Text = slab.perimeter.ToString();
            slabPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabPerimeter);
            slabPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabPerimeter, 7 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabPerimeter, _count);

            TextBlock slabOpeningPerimeter = new TextBlock();
            slabOpeningPerimeter.Text = slab.openingPerimeter.ToString();
            slabOpeningPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabOpeningPerimeter);
            slabOpeningPerimeter.FontSize = _valueFontSize;
            Grid.SetColumn(slabOpeningPerimeter, 8 + _layerPropertyColumnHeaders.Count);
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
            Grid.SetColumn(slabSelectObject, 9 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(slabSelectObject, _count);
        }

        static void GenerateFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            FootingTemplate footing = (FootingTemplate)_obj;

            TextBlock footingCount = new TextBlock();
            footingCount.Text = _count.ToString();
            footingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingCount);
            footingCount.FontSize = _valueFontSize;
            Grid.SetColumn(footingCount, 0);
            Grid.SetRow(footingCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = footing.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock footingName = new TextBlock();
            footingName.Text = footing.nameAbb;
            footingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingName);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingName, _count);

            TextBlock footingVolume = new TextBlock();
            footingVolume.Text = footing.volume.ToString();
            footingVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingVolume);
            footingVolume.FontSize = _valueFontSize;
            Grid.SetColumn(footingVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingVolume, _count);

            TextBlock footingTopArea = new TextBlock();
            footingTopArea.Text = footing.topArea.ToString();
            footingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingTopArea);
            footingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingTopArea, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingTopArea, _count);

            TextBlock footingBottomArea = new TextBlock();
            footingBottomArea.Text = footing.bottomArea.ToString();
            footingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingBottomArea);
            footingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingBottomArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingBottomArea, _count);

            TextBlock footingSideArea = new TextBlock();
            footingSideArea.Text = footing.sideArea.ToString();
            footingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingSideArea);
            footingSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(footingSideArea, 5 + _layerPropertyColumnHeaders.Count);
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
            Grid.SetColumn(footingSelectObject, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(footingSelectObject, _count);
        }

        static void GenerateColumnTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ColumnTemplate column = (ColumnTemplate)_obj;

            TextBlock columnCount = new TextBlock();
            columnCount.Text = _count.ToString();
            columnCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnCount);
            columnCount.FontSize = _valueFontSize;
            Grid.SetColumn(columnCount, 0);
            Grid.SetRow(columnCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = column.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock columnName = new TextBlock();
            columnName.Text = column.nameAbb;
            columnName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnName);
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnName, _count);

            TextBlock columnVolume = new TextBlock();
            columnVolume.Text = column.volume.ToString();
            columnVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnVolume);
            columnVolume.FontSize = _valueFontSize;
            Grid.SetColumn(columnVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnVolume, _count);

            TextBlock columnHeight = new TextBlock();
            columnHeight.Text = column.height.ToString();
            columnHeight.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnHeight);
            columnHeight.FontSize = _valueFontSize;
            Grid.SetColumn(columnHeight, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnHeight, _count);

            TextBlock columnSideArea = new TextBlock();

            if (column.rectangular)
            {
                columnSideArea.Text = column.sideArea.ToString();
                columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
                _layerEstimateGrid.Children.Add(columnSideArea);
                columnSideArea.FontSize = _valueFontSize;
                Grid.SetColumn(columnSideArea, 4 + _layerPropertyColumnHeaders.Count);
                Grid.SetRow(columnSideArea, _count);
            }
            else
            {
                columnSideArea.Text = "N/A";
                columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
                _layerEstimateGrid.Children.Add(columnSideArea);
                columnSideArea.FontSize = _valueFontSize;
                Grid.SetColumn(columnSideArea, 4 + _layerPropertyColumnHeaders.Count);
                Grid.SetRow(columnSideArea, _count);
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
            Grid.SetColumn(columnSelectObject, 5 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(columnSelectObject, _count);
        }

        static void GenerateBeamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            BeamTemplate beam = (BeamTemplate)_obj;

            TextBlock beamCount = new TextBlock();
            beamCount.Text = _count.ToString();
            beamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamCount);
            beamCount.FontSize = _valueFontSize;
            Grid.SetColumn(beamCount, 0);
            Grid.SetRow(beamCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = beam.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock beamName = new TextBlock();
            beamName.Text = beam.nameAbb;
            beamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamName);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamName, _count);

            TextBlock beamVolume = new TextBlock();
            beamVolume.Text = beam.volume.ToString();
            beamVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamVolume);
            beamVolume.FontSize = _valueFontSize;
            Grid.SetColumn(beamVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamVolume, _count);

            TextBlock beamBottomArea = new TextBlock();
            beamBottomArea.Text = beam.bottomArea.ToString();
            beamBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamBottomArea);
            beamBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamBottomArea, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamBottomArea, _count);

            TextBlock beamSideArea = new TextBlock();
            beamSideArea.Text = beam.sideArea.ToString();
            beamSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamSideArea);
            beamSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(beamSideArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamSideArea, _count);

            TextBlock beamLength = new TextBlock();
            beamLength.Text = beam.length.ToString();
            beamLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamLength);
            beamLength.FontSize = _valueFontSize;
            Grid.SetColumn(beamLength, 5 + _layerPropertyColumnHeaders.Count);
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
            Grid.SetColumn(beamSelectObject, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(beamSelectObject, _count);
        }

        static void GenerateWallTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            WallTemplate wall = (WallTemplate)_obj;

            TextBlock wallCount = new TextBlock();
            wallCount.Text = _count.ToString();
            wallCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallCount);
            wallCount.FontSize = _valueFontSize;
            Grid.SetColumn(wallCount, 0);
            Grid.SetRow(wallCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = wall.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock wallName = new TextBlock();
            wallName.Text = wall.nameAbb;
            wallName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallName);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallName, _count);

            TextBlock wallGrossVolume = new TextBlock();
            wallGrossVolume.Text = wall.grossVolume.ToString();
            wallGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallGrossVolume);
            wallGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallGrossVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallGrossVolume, _count);

            TextBlock wallNetVolume = new TextBlock();
            wallNetVolume.Text = wall.netVolume.ToString();
            wallNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallNetVolume);
            wallNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(wallNetVolume, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallNetVolume, _count);

            TextBlock wallTopArea = new TextBlock();
            wallTopArea.Text = wall.topArea.ToString();
            wallTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallTopArea);
            wallTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallTopArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallTopArea, _count);

            TextBlock wallEndArea = new TextBlock();
            wallEndArea.Text = wall.endArea.ToString();
            wallEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallEndArea);
            wallEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallEndArea, 5 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallEndArea, _count);

            TextBlock wallSideArea_1 = new TextBlock();
            wallSideArea_1.Text = wall.sideArea_1.ToString();
            wallSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_1);
            wallSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_1, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSideArea_1, _count);

            TextBlock wallSideArea_2 = new TextBlock();
            wallSideArea_2.Text = wall.sideArea_2.ToString();
            wallSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallSideArea_2);
            wallSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(wallSideArea_2, 7 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSideArea_2, _count);

            TextBlock wallLength = new TextBlock();
            wallLength.Text = wall.length.ToString();
            wallLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallLength);
            wallLength.FontSize = _valueFontSize;
            Grid.SetColumn(wallLength, 8 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallLength, _count);

            TextBlock wallOpeningArea = new TextBlock();
            wallOpeningArea.Text = wall.openingArea.ToString();
            wallOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallOpeningArea);
            wallOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(wallOpeningArea, 9 + _layerPropertyColumnHeaders.Count);
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
            Grid.SetColumn(wallSelectObject, 10 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(wallSelectObject, _count);
        }

        static void GenerateCurbTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            CurbTemplate curb = (CurbTemplate)_obj;

            TextBlock curbCount = new TextBlock();
            curbCount.Text = _count.ToString();
            curbCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbCount);
            curbCount.FontSize = _valueFontSize;
            Grid.SetColumn(curbCount, 0);
            Grid.SetRow(curbCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = curb.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock curbName = new TextBlock();
            curbName.Text = curb.nameAbb;
            curbName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbName);
            curbName.FontSize = _valueFontSize;
            Grid.SetColumn(curbName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbName, _count);

            TextBlock curbGrossVolume = new TextBlock();
            curbGrossVolume.Text = curb.grossVolume.ToString();
            curbGrossVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbGrossVolume);
            curbGrossVolume.FontSize = _valueFontSize;
            Grid.SetColumn(curbGrossVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbGrossVolume, _count);

            TextBlock curbNetVolume = new TextBlock();
            curbNetVolume.Text = curb.netVolume.ToString();
            curbNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbNetVolume);
            curbNetVolume.FontSize = _valueFontSize;
            Grid.SetColumn(curbNetVolume, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbNetVolume, _count);

            TextBlock curbTopArea = new TextBlock();
            curbTopArea.Text = curb.topArea.ToString();
            curbTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbTopArea);
            curbTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbTopArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbTopArea, _count);

            TextBlock curbEndArea = new TextBlock();
            curbEndArea.Text = curb.endArea.ToString();
            curbEndArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbEndArea);
            curbEndArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbEndArea, 5 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbEndArea, _count);

            TextBlock curbSideArea_1 = new TextBlock();
            curbSideArea_1.Text = curb.sideArea_1.ToString();
            curbSideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbSideArea_1);
            curbSideArea_1.FontSize = _valueFontSize;
            Grid.SetColumn(curbSideArea_1, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSideArea_1, _count);

            TextBlock curbSideArea_2 = new TextBlock();
            curbSideArea_2.Text = curb.sideArea_2.ToString();
            curbSideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbSideArea_2);
            curbSideArea_2.FontSize = _valueFontSize;
            Grid.SetColumn(curbSideArea_2, 7 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSideArea_2, _count);

            TextBlock curbLength = new TextBlock();
            curbLength.Text = curb.length.ToString();
            curbLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbLength);
            curbLength.FontSize = _valueFontSize;
            Grid.SetColumn(curbLength, 8 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbLength, _count);

            TextBlock curbOpeningArea = new TextBlock();
            curbOpeningArea.Text = curb.openingArea.ToString();
            curbOpeningArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(curbOpeningArea);
            curbOpeningArea.FontSize = _valueFontSize;
            Grid.SetColumn(curbOpeningArea, 9 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbOpeningArea, _count);

            ToggleButton curbSelectObject = new ToggleButton();
            curbSelectObject.Uid = curb.id;
            curbSelectObject.Content = "SELECT";
            curbSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            curbSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            curbSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            curbSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(curbSelectObject);
            curbSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(curbSelectObject, 10 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(curbSelectObject, _count);
        }

        static void GenerateContinuousFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            ContinuousFootingTemplate continuousFooting = (ContinuousFootingTemplate)_obj;

            TextBlock continuousFootingCount = new TextBlock();
            continuousFootingCount.Text = _count.ToString();
            continuousFootingCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingCount);
            continuousFootingCount.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingCount, 0);
            Grid.SetRow(continuousFootingCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = continuousFooting.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock continuousFootingName = new TextBlock();
            continuousFootingName.Text = continuousFooting.nameAbb;
            continuousFootingName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingName);
            continuousFootingName.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingName, _count);

            TextBlock continuousFootingVolume = new TextBlock();
            continuousFootingVolume.Text = continuousFooting.volume.ToString();
            continuousFootingVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingVolume);
            continuousFootingVolume.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingVolume, 2 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingVolume, _count);

            TextBlock continuousFootingTopArea = new TextBlock();
            continuousFootingTopArea.Text = continuousFooting.topArea.ToString();
            continuousFootingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingTopArea);
            continuousFootingTopArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingTopArea, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingTopArea, _count);

            TextBlock continuousFootingBottomArea = new TextBlock();
            continuousFootingBottomArea.Text = continuousFooting.bottomArea.ToString();
            continuousFootingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingBottomArea);
            continuousFootingBottomArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingBottomArea, 4 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingBottomArea, _count);

            TextBlock continuousFootingSideArea = new TextBlock();
            continuousFootingSideArea.Text = continuousFooting.sideArea.ToString();
            continuousFootingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingSideArea);
            continuousFootingSideArea.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingSideArea, 5 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingSideArea, _count);

            TextBlock continuousFootingLength = new TextBlock();
            continuousFootingLength.Text = continuousFooting.length.ToString();
            continuousFootingLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(continuousFootingLength);
            continuousFootingLength.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingLength, 6 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingLength, _count);

            ToggleButton continuousFootingSelectObject = new ToggleButton();
            continuousFootingSelectObject.Uid = continuousFooting.id;
            continuousFootingSelectObject.Content = "SELECT";
            continuousFootingSelectObject.Checked += new RoutedEventHandler(SelectObjectActivated);
            continuousFootingSelectObject.Unchecked += new RoutedEventHandler(DeselectObjectActivated);
            continuousFootingSelectObject.HorizontalAlignment = HorizontalAlignment.Stretch;
            continuousFootingSelectObject.Margin = new Thickness(2, 5, 2, 5);
            _layerEstimateGrid.Children.Add(continuousFootingSelectObject);
            continuousFootingSelectObject.FontSize = _valueFontSize;
            Grid.SetColumn(continuousFootingSelectObject, 7 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(continuousFootingSelectObject, _count);
        }

        static void GenerateStyrofoamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize,
            List<string> _layerPropertyColumnHeaders, RoutedEventHandler SelectObjectActivated, RoutedEventHandler DeselectObjectActivated)
        {
            StyrofoamTemplate styrofoam = (StyrofoamTemplate)_obj;
            
            TextBlock styrofoamCount = new TextBlock();
            styrofoamCount.Text = _count.ToString();
            styrofoamCount.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamCount);
            styrofoamCount.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamCount, 0);
            Grid.SetRow(styrofoamCount, _count);

            for (int i = 0; i < _layerPropertyColumnHeaders.Count; i++)
            {
                try
                {
                    TextBlock value = new TextBlock();
                    value.Text = styrofoam.parsedLayerName[_layerPropertyColumnHeaders[i]];
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
                catch
                {
                    TextBlock value = new TextBlock();
                    value.Text = "N/A";
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    _layerEstimateGrid.Children.Add(value);
                    value.FontSize = _valueFontSize;
                    Grid.SetColumn(value, 1 + i);
                    Grid.SetRow(value, _count);
                }
            }

            TextBlock styrofoamName = new TextBlock();
            styrofoamName.Text = styrofoam.nameAbb;
            styrofoamName.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamName);
            styrofoamName.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamName, 1 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(styrofoamName, _count);

            TextBlock styrofoamVolume = new TextBlock();
            styrofoamVolume.Text = styrofoam.volume.ToString();
            styrofoamVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(styrofoamVolume);
            styrofoamVolume.FontSize = _valueFontSize;
            Grid.SetColumn(styrofoamVolume, 2 + _layerPropertyColumnHeaders.Count);
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
            Grid.SetColumn(styrofoamSelectObject, 3 + _layerPropertyColumnHeaders.Count);
            Grid.SetRow(styrofoamSelectObject, _count);
        }
    }
}
