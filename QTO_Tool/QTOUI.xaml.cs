﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Excel = Microsoft.Office.Interop.Excel;

namespace QTO_Tool
{
    /// <summary>
    /// Interaction logic for QTOUI.xaml
    /// </summary>
    /// 
    public partial class QTOUI : Window
    {
        List<string> layerNames = new List<string>();
        List<string> selectedConcreteTemplatesForLayers = new List<string>();

        List<BeamTemplate> allBeams = new List<BeamTemplate>();
        List<ColumnTemplate> allColumns = new List<ColumnTemplate>();
        List<ContinousFootingTemplate> allContinousFootings = new List<ContinousFootingTemplate>();
        List<CurbTemplate> allCurbs = new List<CurbTemplate>();
        List<FootingTemplate> allFootings = new List<FootingTemplate>();
        List<SlabTemplate> allSlabs = new List<SlabTemplate>();
        List<WallTemplate> allWalls = new List<WallTemplate>();
        List<StyrofoamTemplate> allStyrofoams = new List<StyrofoamTemplate>();

        Dictionary<string, object> allSelectedTemplates = new Dictionary<string, object>();
        Dictionary<string, List<string>> allSelectedTemplateValues = new Dictionary<string, List<string>>();

        List<string> quantityValues = new List<string>();

        List<RhinoObject> selectedObjects = new List<RhinoObject>();

        // Save Dictionary
        Dictionary<string, object> saveData = new Dictionary<string, object>();

        public QTOUI()
        {
            InitializeComponent();
        }

        public void Load_Clicked(object sender, RoutedEventArgs e)
        {

        }

        public void Save_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void StartCheckup_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.ConcreteIsIncluded.IsChecked == true)
            {

                this.CheckupResults.Content = Methods.ConcreteModelSetup();

                this.CheckupResults.Visibility = Visibility.Visible;

                if (this.ConcreteTemplateGrid.Children.Count == 0)
                {
                    UIMethods.GenerateLayerTemplate(this.ConcreteTemplateGrid);

                    CalculateQuantitiesButton.IsEnabled = true;
                    AngleThresholdLabel.IsEnabled = true;
                    AngleThresholdSlider.IsEnabled = true;
                    CombineValuesLabel.IsEnabled = true;
                    CombinedValuesToggle.IsEnabled = true;
                }
                else
                {
                    this.ConcreteTemplateGrid.Children.Clear();
                    this.ConcreteTemplateGrid.RowDefinitions.Clear();
                    UIMethods.GenerateLayerTemplate(this.ConcreteTemplateGrid);
                    this.DissipatedConcreteTablePanel.Children.Clear();

                    this.ExportExcelButton.IsEnabled = false;
                    this.ConcreteSaveButton.IsEnabled = false;

                    this.allSelectedTemplates.Clear();
                    this.allSelectedTemplateValues.Clear();

                    this.layerNames.Clear();
                    this.selectedConcreteTemplatesForLayers.Clear();
                }
            }
            if (this.ExteriorIsIncluded.IsChecked == true)
            {

            }

            if (this.ConcreteIsIncluded.IsChecked == false && this.ExteriorIsIncluded.IsChecked == false)
            {
                MessageBox.Show("Please select atleast one of the methods.");
            }

            RhinoDoc.SelectObjects += OnSelectObjects;
            RhinoDoc.DeselectObjects += OnDeselectObjects;
            RhinoDoc.DeselectAllObjects += OnDeselectAllObjects;
        }

        /*---------------- Handeling Select Object Event ----------------*/
        private void ObjectSelection_Activated(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;

            RhinoObject rhobj = RunQTO.doc.Objects.FindId(new Guid(btn.Uid));

            selectedObjects.Add(rhobj);

            rhobj.Select(true);

            RunQTO.doc.Views.Redraw();
        }

        /*---------------- Handeling Deselect Object Event ----------------*/
        private void ObjectDeselection_Activated(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;

            RhinoObject rhobj = RunQTO.doc.Objects.FindId(new Guid(btn.Uid));

            selectedObjects.Remove(rhobj);

            rhobj.Select(false);

            RunQTO.doc.Views.Redraw();
        }

        private void Calculate_Concrete_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                allBeams.Clear();
                allColumns.Clear();
                allCurbs.Clear();
                allFootings.Clear();
                allWalls.Clear();
                allContinousFootings.Clear();
                allSlabs.Clear();
                allStyrofoams.Clear();

                this.allSelectedTemplates.Clear();
                this.allSelectedTemplateValues.Clear();

                this.layerNames.Clear();
                this.selectedConcreteTemplatesForLayers.Clear();

                this.DissipatedConcreteTablePanel.Children.Clear();
                this.CombinedConcreteTablePanel.Children.Clear();

                double angleThreshold = this.AngleThresholdSlider.Value;

                ComboBox selectedConcreteTemplate;

                RhinoObject[] rhobjs;

                string selectedTemplate;

                string layerName;

                string objType;

                List<object> layerTemplates;

                for (int i = 0; i < RunQTO.doc.Layers.Count; i++)
                {
                    if (RunQTO.doc.Layers[i].IsDeleted == false)
                    {
                        selectedConcreteTemplate = LogicalTreeHelper.FindLogicalNode(this.ConcreteTemplateGrid,
                            "ConcreteTemplates_" + i.ToString()) as ComboBox;

                        selectedTemplate = selectedConcreteTemplate.SelectedItem.ToString().Split(':').Last().Replace(" ", string.Empty);
                        this.selectedConcreteTemplatesForLayers.Add(selectedTemplate);

                        layerName = RunQTO.doc.Layers[i].Name;
                        this.layerNames.Add(layerName);

                        layerTemplates = new List<object>();

                        if (selectedTemplate == "Beam")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                BeamTemplate beam = new BeamTemplate(rhobjs[j], layerName, angleThreshold);

                                allBeams.Add(beam);
                                layerTemplates.Add(beam);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                        }

                        if (selectedTemplate == "Column")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                ColumnTemplate column = new ColumnTemplate(rhobjs[j], layerName);

                                allColumns.Add(column);
                                layerTemplates.Add(column);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" };
                        }

                        if (selectedTemplate == "ContinousFooting")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                ContinousFootingTemplate continousFooting = new ContinousFootingTemplate(rhobjs[j], layerName, angleThreshold);

                                allContinousFootings.Add(continousFooting);
                                layerTemplates.Add(continousFooting);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                        }

                        if (selectedTemplate == "Curb")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                CurbTemplate curb = new CurbTemplate(rhobjs[j], layerName, angleThreshold);

                                allCurbs.Add(curb);
                                layerTemplates.Add(curb);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                        }

                        if (selectedTemplate == "Footing")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                FootingTemplate footing = new FootingTemplate(rhobjs[j], layerName, angleThreshold);

                                allFootings.Add(footing);
                                layerTemplates.Add(footing);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "ISOLATE" };
                        }

                        if (selectedTemplate == "Wall")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                WallTemplate wall = new WallTemplate(rhobjs[j], layerName, angleThreshold);

                                allWalls.Add(wall);
                                layerTemplates.Add(wall);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                        }

                        if (selectedTemplate == "Slab")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                objType = rhobjs[j].GetType().ToString().Split('.').Last<string>();

                                SlabTemplate slab = new SlabTemplate(rhobjs[j], layerName, angleThreshold);

                                allSlabs.Add(slab);
                                layerTemplates.Add(slab);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "EDGE AREA", "PERIMETER", "OPENING PERIMETER", "ISOLATE" };
                        }

                        if (selectedTemplate == "Styrofoam")
                        {
                            rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                            for (int j = 0; j < rhobjs.Length; j++)
                            {
                                StyrofoamTemplate styrofoam = new StyrofoamTemplate(rhobjs[j], layerName);

                                allStyrofoams.Add(styrofoam);
                                layerTemplates.Add(styrofoam);
                            }

                            quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "ISOLATE" };
                        }

                        // Generate Dissipated Value Table
                        UIMethods.GenerateDissipatedTableExpander(this.DissipatedConcreteTablePanel, layerName, selectedTemplate,
                            layerTemplates, quantityValues, ObjectSelection_Activated, ObjectDeselection_Activated);

                        quantityValues.Clear();

                        if (selectedTemplate == "N/A")
                        {
                            continue;
                        }
                    }
                }

                this.allSelectedTemplates.Add("Beam", allBeams);
                this.allSelectedTemplateValues.Add("Beam", new List<string>() { "COUNT", "NAME", "VOLUME", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Column", allColumns);
                this.allSelectedTemplateValues.Add("Column", new List<string>() { "COUNT", "NAME", "VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" });
                this.allSelectedTemplates.Add("Curb", allCurbs);
                this.allSelectedTemplateValues.Add("Curb", new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Footing", allFootings);
                this.allSelectedTemplateValues.Add("Footing", new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "ISOLATE" });
                this.allSelectedTemplates.Add("Wall", allWalls);
                this.allSelectedTemplateValues.Add("Wall", new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" });
                this.allSelectedTemplates.Add("Continous Footing", allContinousFootings);
                this.allSelectedTemplateValues.Add("Continous Footing", new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Slab", allSlabs);
                this.allSelectedTemplateValues.Add("Slab", new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "EDGE AREA", "PERIMETER", "OPENING PERIMETER", "ISOLATE" });
                this.allSelectedTemplates.Add("Styrofoam", allStyrofoams);
                this.allSelectedTemplateValues.Add("Styrofoam", new List<string>() { "COUNT", "NAME", "VOLUME", "ISOLATE" });

                // Generate Combined Value Table
                UIMethods.GenerateCombinedTableExpander(this.CombinedConcreteTablePanel, this.allSelectedTemplates,
                    this.allSelectedTemplateValues, ObjectSelection_Activated, ObjectDeselection_Activated);

                if (CombinedValuesToggle.IsChecked == true)
                {
                    this.DissipatedConcreteTablePanel.Visibility = Visibility.Collapsed;
                    this.CombinedConcreteTablePanel.Visibility = Visibility.Visible;
                }

                else
                {
                    this.DissipatedConcreteTablePanel.Visibility = Visibility.Visible;
                    this.CombinedConcreteTablePanel.Visibility = Visibility.Collapsed;
                }

                this.ExportExcelButton.IsEnabled = true;
                this.ConcreteSaveButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Something went Wrong!");

                MessageBox.Show(ex.ToString());
            }
        }

        private void Concrete_Save_Clicked(object sender, RoutedEventArgs e)
        {
            Stream stream = null;
            StreamWriter streamWriter = null;

            this.saveData.Add("LayerNames", this.layerNames);
            this.saveData.Add("SelectedConcreteTemplatesForLayers", this.selectedConcreteTemplatesForLayers);
            this.saveData.Add("AllSelectedTemplates", this.allSelectedTemplates);
            this.saveData.Add("AllSelectedTemplateValues", this.allSelectedTemplateValues);

            // Json String Of The Save Data
            string projectData = JsonConvert.SerializeObject(saveData, Formatting.Indented);

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (stream = File.Open(saveFileDialog.FileName, FileMode.Create))
                {
                    using (streamWriter = new StreamWriter(stream))
                    {
                        streamWriter.Write(projectData);
                    }
                }

                MessageBox.Show("Save was successful.");
            }

            else
            {
                MessageBox.Show("Something went wrong, please try again.");
            }
        }

        private void Concrete_Load_Clicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Implemented Yet!");
        }

        private void Export_Excel_Clicked(object sender, RoutedEventArgs e)
        {
            ExcelMethods.ExportExcel(this.DissipatedConcreteTablePanel, this.CombinedConcreteTablePanel);
        }

        private void Combined_Values_Toggle_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.DissipatedConcreteTablePanel.Visibility == Visibility.Collapsed)
            {
                this.DissipatedConcreteTablePanel.Visibility = Visibility.Visible;

                this.CombinedConcreteTablePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.CombinedConcreteTablePanel.Visibility = Visibility.Visible;

                this.DissipatedConcreteTablePanel.Visibility = Visibility.Collapsed;
            }
        }

        void OnSelectObjects(object sender, RhinoObjectSelectionEventArgs args)
        {
            if (args.Selected && this.ExportExcelButton.IsEnabled) // objects were selected
            {
                foreach (RhinoObject obj in args.RhinoObjects)
                {
                    ToggleButton dissipatedSelectToggleButton = (ToggleButton)(Methods.GetByUid(this.DissipatedConcreteTablePanel, obj.Id.ToString()));

                    ToggleButton combinedSelectToggleButton = (ToggleButton)(Methods.GetByUid(this.CombinedConcreteTablePanel, obj.Id.ToString()));

                    if (dissipatedSelectToggleButton != null)
                    {
                        dissipatedSelectToggleButton.IsChecked = true;
                    }

                    if (combinedSelectToggleButton != null)
                    {
                        combinedSelectToggleButton.IsChecked = true;
                    }
                }
            }
        }

        void OnDeselectObjects(object sender, RhinoObjectSelectionEventArgs args)
        {
            if (!args.Selected && this.ExportExcelButton.IsEnabled) // objects were selected
            {
                // do something
                foreach (RhinoObject obj in args.RhinoObjects)
                {
                    ToggleButton dissipatedSelectToggleButton = (ToggleButton)(Methods.GetByUid(this.DissipatedConcreteTablePanel, obj.Id.ToString()));

                    ToggleButton combinedSelectToggleButton = (ToggleButton)(Methods.GetByUid(this.CombinedConcreteTablePanel, obj.Id.ToString()));

                    if (dissipatedSelectToggleButton != null)
                    {
                        dissipatedSelectToggleButton.IsChecked = false;
                    }

                    if (combinedSelectToggleButton != null)
                    {
                        combinedSelectToggleButton.IsChecked = false;
                    }
                }
            }
        }

        void OnDeselectAllObjects(object sender, RhinoDeselectAllObjectsEventArgs args)
        {
            if (this.ExportExcelButton.IsEnabled)
            {
                Grid contentGrid;
                string elementType;

                foreach (UIElement expander in this.DissipatedConcreteTablePanel.Children)
                {
                    contentGrid = (Grid)(((Expander)expander).Content);

                    foreach (UIElement element in contentGrid.Children)
                    {
                        elementType = (element.GetType().ToString().Split('.')).Last().ToLower();

                        if (elementType == "togglebutton")
                        {
                            ((ToggleButton)element).IsChecked = false;
                        }
                    }
                }

                foreach (UIElement expander in this.CombinedConcreteTablePanel.Children)
                {
                    contentGrid = (Grid)(((Expander)expander).Content);

                    foreach (UIElement element in contentGrid.Children)
                    {
                        elementType = (element.GetType().ToString().Split('.')).Last().ToLower();

                        if (elementType == "togglebutton")
                        {
                            ((ToggleButton)element).IsChecked = false;
                        }
                    }
                }
            }
        }
    }
}
