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

            foreach (Rhino.DocObjects.Layer layer in RunQTO.doc.Layers)
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
                    //concreteTemplatesSelector.SelectionChanged += ComboBox_SelectionChanged;

                    grid.Children.Add(concreteTemplatesSelector);
                    Grid.SetColumn(concreteTemplatesSelector, 1);
                    Grid.SetRow(concreteTemplatesSelector, layerCounter);

                    layerCounter++;
                }
            }
        }

        public static void GenerateAccumulatedSlabTableExpander(StackPanel stackPanel, string layerName, string templateType, List<object> layerTemplate, List<string> values)
        {
            Expander layerEstimateExpander = new Expander();
            layerEstimateExpander.Name = "LayerEstimateExpader";
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

            foreach (object obj in layerTemplate)
            {
                //Dynamically adding Rows to the Grid
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                layerEstimateGrid.RowDefinitions.Add(rowDef);

                int count = layerTemplate.IndexOf(obj) + 1;
                int valueFontSize = 18;

                if (templateType == "Slab")
                {
                    UIMethods.GenerateSlabTableExpander(obj, count, layerEstimateGrid, valueFontSize);
                }

                else if (templateType == "Footing")
                {
                    UIMethods.GenerateFootingTableExpander(obj, count, layerEstimateGrid, valueFontSize);
                }

                else if (templateType == "Column")
                {
                    UIMethods.GenerateColumnTableExpander(obj, count, layerEstimateGrid, valueFontSize);
                }

                else if (templateType == "Beam")
                {
                    UIMethods.GenerateBeamTableExpander(obj, count, layerEstimateGrid, valueFontSize);
                }

                else if (templateType == "Wall")
                {
                    UIMethods.GenerateWallTableExpander(obj, count, layerEstimateGrid, valueFontSize);
                }
            }

            layerEstimateExpander.Content = layerEstimateGrid;

            stackPanel.Children.Add(layerEstimateExpander);
        }

        public static void GenerateCumulatedSlabTableExpander(StackPanel stackPanel)
        {

        }

        static void GenerateSlabTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize)
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
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabGrossVolume, 2);
            Grid.SetRow(slabGrossVolume, _count);

            Label slabNetVolume = new Label();
            slabNetVolume.Content = slab.netVolume.ToString();
            slabNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabNetVolume);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabNetVolume, 3);
            Grid.SetRow(slabNetVolume, _count);

            Label slabTopArea = new Label();
            slabTopArea.Content = slab.topArea.ToString();
            slabTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabTopArea);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabTopArea, 4);
            Grid.SetRow(slabTopArea, _count);

            Label slabBottomArea = new Label();
            slabBottomArea.Content = slab.bottomArea.ToString();
            slabBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabBottomArea);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabBottomArea, 5);
            Grid.SetRow(slabBottomArea, _count);

            Label slabEdgeArea = new Label();
            slabEdgeArea.Content = slab.edgeArea.ToString();
            slabEdgeArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabEdgeArea);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabEdgeArea, 6);
            Grid.SetRow(slabEdgeArea, _count);

            Label slabPerimeter = new Label();
            slabPerimeter.Content = slab.perimeter.ToString();
            slabPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabPerimeter);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabPerimeter, 7);
            Grid.SetRow(slabPerimeter, _count);

            Label slabOpeningPerimeter = new Label();
            slabOpeningPerimeter.Content = slab.openingPerimeter.ToString();
            slabOpeningPerimeter.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(slabOpeningPerimeter);
            slabName.FontSize = _valueFontSize;
            Grid.SetColumn(slabOpeningPerimeter, 8);
            Grid.SetRow(slabOpeningPerimeter, _count);
        }

        static void GenerateFootingTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize)
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
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingVolume, 2);
            Grid.SetRow(footingVolume, _count);

            Label footingTopArea = new Label();
            footingTopArea.Content = footing.topArea.ToString();
            footingTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingTopArea);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingTopArea, 3);
            Grid.SetRow(footingTopArea, _count);

            Label footingBottomArea = new Label();
            footingBottomArea.Content = footing.bottomArea.ToString();
            footingBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingBottomArea);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingBottomArea, 4);
            Grid.SetRow(footingBottomArea, _count);

            Label footingSideArea = new Label();
            footingSideArea.Content = footing.sideArea.ToString();
            footingSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(footingSideArea);
            footingName.FontSize = _valueFontSize;
            Grid.SetColumn(footingSideArea, 5);
            Grid.SetRow(footingSideArea, _count);
        }

        static void GenerateColumnTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize)
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
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnVolume, 2);
            Grid.SetRow(columnVolume, _count);

            Label columnHeight = new Label();
            columnHeight.Content = column.height.ToString();
            columnHeight.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnHeight);
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnHeight, 3);
            Grid.SetRow(columnHeight, _count);

            Label columnSideArea = new Label();
            columnSideArea.Content = column.sideArea.ToString();
            columnSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(columnSideArea);
            columnName.FontSize = _valueFontSize;
            Grid.SetColumn(columnSideArea, 4);
            Grid.SetRow(columnSideArea, _count);
        }

        static void GenerateBeamTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize)
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
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamVolume, 2);
            Grid.SetRow(beamVolume, _count);

            Label beamBottomArea = new Label();
            beamBottomArea.Content = beam.bottomArea.ToString();
            beamBottomArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamBottomArea);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamBottomArea, 3);
            Grid.SetRow(beamBottomArea, _count);

            Label beamSideArea = new Label();
            beamSideArea.Content = beam.sideArea.ToString();
            beamSideArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamSideArea);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamSideArea, 4);
            Grid.SetRow(beamSideArea, _count);

            Label beamLength = new Label();
            beamLength.Content = beam.length.ToString();
            beamLength.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(beamLength);
            beamName.FontSize = _valueFontSize;
            Grid.SetColumn(beamLength, 5);
            Grid.SetRow(beamLength, _count);
        }

        static void GenerateWallTableExpander(object _obj, int _count, Grid _layerEstimateGrid, int _valueFontSize)
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
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallGrossVolume, 2);
            Grid.SetRow(wallGrossVolume, _count);

            Label wallNetVolume = new Label();
            wallNetVolume.Content = wall.netVolume.ToString();
            wallNetVolume.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallNetVolume);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallNetVolume, 3);
            Grid.SetRow(wallNetVolume, _count);

            Label wallTopArea = new Label();
            wallTopArea.Content = wall.topArea.ToString();
            wallTopArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(wallTopArea);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(wallTopArea, 4);
            Grid.SetRow(wallTopArea, _count);

            Label endArea = new Label();
            endArea.Content = wall.endArea.ToString();
            endArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(endArea);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(endArea, 5);
            Grid.SetRow(endArea, _count);

            Label sideArea_1 = new Label();
            sideArea_1.Content = wall.sideArea_1.ToString();
            sideArea_1.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(sideArea_1);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(sideArea_1, 6);
            Grid.SetRow(sideArea_1, _count);

            Label sideArea_2 = new Label();
            sideArea_2.Content = wall.sideArea_2.ToString();
            sideArea_2.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(sideArea_2);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(sideArea_2, 7);
            Grid.SetRow(sideArea_2, _count);

            Label length = new Label();
            length.Content = wall.length.ToString();
            length.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(length);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(length, 8);
            Grid.SetRow(length, _count);

            Label openingArea = new Label();
            openingArea.Content = wall.openingArea.ToString();
            openingArea.HorizontalAlignment = HorizontalAlignment.Center;
            _layerEstimateGrid.Children.Add(openingArea);
            wallName.FontSize = _valueFontSize;
            Grid.SetColumn(openingArea, 9);
            Grid.SetRow(openingArea, _count);
        }
    }
}
