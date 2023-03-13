using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using Rhino;
using Rhino.DocObjects;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Threading;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;
using Xbim.Ifc;
using Xbim.Ifc4.ProductExtension;

namespace QTO_Tool
{
    /// <summary>
    /// Interaction logic for QTOUI.xaml
    /// </summary>
    /// 
    public partial class QTOUI : Window
    {
        Dictionary<string, string> selectedConcreteTemplatesForLayers = new Dictionary<string, string>();

        AllBeams allBeams = new AllBeams();
        AllColumns allColumns = new AllColumns();
        AllContinousFootings allContinuousFootings = new AllContinousFootings();
        AllCurbs allCurbs = new AllCurbs();
        AllFootings allFootings = new AllFootings();
        AllSlabs allSlabs = new AllSlabs();
        AllWalls allWalls = new AllWalls();
        AllStyrofoams allStyrofoams = new AllStyrofoams();

        Dictionary<string, object> allSelectedTemplates = new Dictionary<string, object>();
        Dictionary<string, List<string>> allSelectedTemplateValues = new Dictionary<string, List<string>>();

        List<string> quantityValues = new List<string>();

        List<string> layerPropertyColumnHeaders = new List<string>();

        List<RhinoObject> selectedObjects = new List<RhinoObject>();

        // Save/Load Dictionary
        Dictionary<string, object> saveData = new Dictionary<string, object>();
        Dictionary<string, object> loadData = new Dictionary<string, object>();

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
            // Always get the Active model
            if (RunQTO.doc.IsAvailable == false)
            {
                RunQTO.doc = RhinoDoc.ActiveDoc;
            }

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

            this.layerPropertyColumnHeaders.Clear();

            if (this.ConcreteIsIncluded.IsChecked == true)
            {

                this.CheckupResults.Content = Methods.ConcreteModelSetup();

                this.CheckupResults.Visibility = Visibility.Visible;

                if (this.ConcreteTemplateGrid.Children.Count == 0)
                {
                    UIMethods.GenerateLayerTemplate(this.ConcreteTemplateGrid, this.layerPropertyColumnHeaders);

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
                    UIMethods.GenerateLayerTemplate(this.ConcreteTemplateGrid, this.layerPropertyColumnHeaders);
                    this.DissipatedConcreteTablePanel.Children.Clear();

                    this.ExportExcelButton.IsEnabled = false;
                    this.ConcreteSaveButton.IsEnabled = false;

                    this.allSelectedTemplates.Clear();
                    this.allSelectedTemplateValues.Clear();

                    this.selectedConcreteTemplatesForLayers.Clear();

                    this.saveData.Clear();
                    this.loadData.Clear();
                }
            }
            if (this.ExteriorIsIncluded.IsChecked == true)
            {

            }

            if (this.ConcreteIsIncluded.IsChecked == false && this.ExteriorIsIncluded.IsChecked == false)
            {
                MessageBox.Show("Please select at least one of the methods.");
            }

            RhinoDoc.SelectObjects += OnSelectObjects;
            RhinoDoc.DeselectObjects += OnDeselectObjects;
            RhinoDoc.DeselectAllObjects += OnDeselectAllObjects;

            Dispatcher.FromThread(newWindowThread).InvokeShutdown();
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
                this.allBeams.Clear();
                this.allColumns.Clear();
                this.allCurbs.Clear();
                this.allFootings.Clear();
                this.allWalls.Clear();
                this.allContinuousFootings.Clear();
                this.allSlabs.Clear();
                this.allStyrofoams.Clear();

                this.allSelectedTemplates.Clear();
                this.allSelectedTemplateValues.Clear();

                this.selectedConcreteTemplatesForLayers.Clear();

                this.saveData.Clear();
                this.loadData.Clear();

                this.DissipatedConcreteTablePanel.Children.Clear();
                this.CombinedConcreteTablePanel.Children.Clear();

                double angleThreshold = Methods.CalculateAngleThreshold(this.AngleThresholdSlider.Value);
                
                ComboBox selectedConcreteTemplate;

                RhinoObject[] rhobjs;

                string selectedTemplate;

                string layerName;

                string objFloor;

                string objType;

                List<object> layerTemplates;

                for (int i = 0; i < RunQTO.doc.Layers.Count; i++)
                {
                    if (RunQTO.doc.Layers[i].IsDeleted == false)
                    {
                        selectedConcreteTemplate = LogicalTreeHelper.FindLogicalNode(this.ConcreteTemplateGrid,
                            "ConcreteTemplates_" + i.ToString()) as ComboBox;

                        selectedTemplate = selectedConcreteTemplate.SelectedItem.ToString().Split(':').Last().Replace(" ", string.Empty);

                        layerName = RunQTO.doc.Layers[i].Name;

                        if (layerName.Split('_').Length >= 3)
                        {
                            objFloor = layerName.Split('_')[2];

                            this.selectedConcreteTemplatesForLayers.Add(layerName, selectedTemplate);

                            layerTemplates = new List<object>();

                            if (selectedTemplate == "Beam")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    BeamTemplate beam = new BeamTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allBeams.allTemplates.ContainsKey(objFloor))
                                    {
                                        allBeams.allTemplates[objFloor].Add(beam);
                                    }
                                    else
                                    {
                                        allBeams.allTemplates.Add(objFloor, new List<object> { beam });
                                    }

                                    layerTemplates.Add(beam);

                                    if (allSlabs.allTemplates.ContainsKey(objFloor))
                                    {
                                        foreach (var item in allSlabs.allTemplates[objFloor])
                                        {
                                            SlabTemplate slabTemplate = (SlabTemplate)item;

                                            if (!slabTemplate.beams.ContainsKey(beam.id))
                                            {
                                                slabTemplate.beams.Add(beam.id, beam);
                                            }
                                        }
                                    }
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "BOTTOM AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                            }

                            if (selectedTemplate == "Column")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    ColumnTemplate column = new ColumnTemplate(rhobjs[j], layerName, true);

                                    if (allColumns.allTemplates.ContainsKey(objFloor))
                                    {
                                        allColumns.allTemplates[objFloor].Add(column);
                                    }
                                    else
                                    {
                                        allColumns.allTemplates.Add(objFloor, new List<object> { column });
                                    }

                                    layerTemplates.Add(column);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" };
                            }

                            if (selectedTemplate.Contains("Non-Rectangular"))
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    ColumnTemplate column = new ColumnTemplate(rhobjs[j], layerName, false);

                                    if (allColumns.allTemplates.ContainsKey(objFloor))
                                    {
                                        allColumns.allTemplates[objFloor].Add(column);
                                    }
                                    else
                                    {
                                        allColumns.allTemplates.Add(objFloor, new List<object> { column });
                                    }

                                    layerTemplates.Add(column);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" };
                            }

                            if (selectedTemplate == "ContinuousFooting")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    ContinuousFootingTemplate continuousFooting = new ContinuousFootingTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allContinuousFootings.allTemplates.ContainsKey(objFloor))
                                    {
                                        allContinuousFootings.allTemplates[objFloor].Add(continuousFooting);
                                    }
                                    else
                                    {
                                        allContinuousFootings.allTemplates.Add(objFloor, new List<object> { continuousFooting });
                                    }

                                    layerTemplates.Add(continuousFooting);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                            }

                            if (selectedTemplate == "Curb")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    CurbTemplate curb = new CurbTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allCurbs.allTemplates.ContainsKey(objFloor))
                                    {
                                        allCurbs.allTemplates[objFloor].Add(curb);
                                    }
                                    else
                                    {
                                        allCurbs.allTemplates.Add(objFloor, new List<object> { curb });
                                    }

                                    layerTemplates.Add(curb);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                            }

                            if (selectedTemplate == "Footing")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    FootingTemplate footing = new FootingTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allFootings.allTemplates.ContainsKey(objFloor))
                                    {
                                        allFootings.allTemplates[objFloor].Add(footing);
                                    }
                                    else
                                    {
                                        allFootings.allTemplates.Add(objFloor, new List<object> { footing });
                                    }

                                    layerTemplates.Add(footing);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "ISOLATE" };
                            }

                            if (selectedTemplate == "Wall")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    WallTemplate wall = new WallTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allWalls.allTemplates.ContainsKey(objFloor))
                                    {
                                        allWalls.allTemplates[objFloor].Add(wall);
                                    }
                                    else
                                    {
                                        allWalls.allTemplates.Add(objFloor, new List<object> { wall });
                                    }

                                    layerTemplates.Add(wall);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                            }

                            if (selectedTemplate == "Slab")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    objType = rhobjs[j].GetType().ToString().Split('.').Last<string>();

                                    SlabTemplate slab = new SlabTemplate(rhobjs[j], layerName, angleThreshold);

                                    if (allSlabs.allTemplates.ContainsKey(objFloor))
                                    {
                                        allSlabs.allTemplates[objFloor].Add(slab);
                                    }
                                    else
                                    {
                                        allSlabs.allTemplates.Add(objFloor, new List<object> { slab });
                                    }

                                    layerTemplates.Add(slab);

                                    if (allBeams.allTemplates.ContainsKey(objFloor))
                                    {
                                        foreach (var item in allBeams.allTemplates[objFloor])
                                        {
                                            BeamTemplate beamTemplate = (BeamTemplate)item;

                                            if (!slab.beams.ContainsKey(beamTemplate.id))
                                            {
                                                slab.beams.Add(beamTemplate.id, beamTemplate);
                                            }
                                        }
                                    }
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "EDGE AREA", "PERIMETER", "OPENING PERIMETER", "ISOLATE" };
                            }

                            if (selectedTemplate == "Styrofoam")
                            {
                                rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                                for (int j = 0; j < rhobjs.Length; j++)
                                {
                                    StyrofoamTemplate styrofoam = new StyrofoamTemplate(rhobjs[j], layerName);

                                    if (allStyrofoams.allTemplates.ContainsKey(objFloor))
                                    {
                                        allStyrofoams.allTemplates[objFloor].Add(styrofoam);
                                    }
                                    else
                                    {
                                        allStyrofoams.allTemplates.Add(objFloor, new List<object> { styrofoam });
                                    }

                                    layerTemplates.Add(styrofoam);
                                }

                                quantityValues = new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "ISOLATE" };
                            }

                            if (quantityValues.Count > 0)
                            {
                                quantityValues.InsertRange(1, this.layerPropertyColumnHeaders);
                            }

                            // Generate Dissipated Value Table
                            UIMethods.GenerateConcreteTableExpander(this.DissipatedConcreteTablePanel, layerName, selectedTemplate,
                                layerTemplates, quantityValues, this.layerPropertyColumnHeaders, ObjectSelection_Activated, ObjectDeselection_Activated);

                            quantityValues.Clear();

                            if (selectedTemplate == "N/A")
                            {
                                continue;
                            }
                        }
                    }
                }

                foreach (var item in allSlabs.allTemplates)
                {
                    foreach (SlabTemplate slab in item.Value)
                    {
                        slab.UpdateNetVolumeAndBottomAreaWithBeams();

                        ((TextBlock)(Methods.GetByUid(this.DissipatedConcreteTablePanel, slab.id + "_NetVolume"))).Text = slab.netVolume.ToString();

                        ((TextBlock)(Methods.GetByUid(this.DissipatedConcreteTablePanel, slab.id + "_BottomArea"))).Text = slab.bottomArea.ToString();
                    }
                }

                this.allSelectedTemplates.Add("Beam", allBeams);
                this.allSelectedTemplateValues.Add("Beam", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Column", allColumns);
                this.allSelectedTemplateValues.Add("Column", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" });
                this.allSelectedTemplates.Add("Curb", allCurbs);
                this.allSelectedTemplateValues.Add("Curb", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "TOP AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Footing", allFootings);
                this.allSelectedTemplateValues.Add("Footing", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "ISOLATE" });
                this.allSelectedTemplates.Add("Wall", allWalls);
                this.allSelectedTemplateValues.Add("Wall", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" });
                this.allSelectedTemplates.Add("Continuous Footing", allContinuousFootings);
                this.allSelectedTemplateValues.Add("Continuous Footing", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" });
                this.allSelectedTemplates.Add("Slab", allSlabs);
                this.allSelectedTemplateValues.Add("Slab", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "EDGE AREA", "PERIMETER", "OPENING PERIMETER", "ISOLATE" });
                this.allSelectedTemplates.Add("Styrofoam", allStyrofoams);
                this.allSelectedTemplateValues.Add("Styrofoam", new List<string>() { "COUNT", "NAME ABB.", "GROSS VOLUME", "ISOLATE" });

                // Generate Combined Value Table
                //UIMethods.GenerateCombinedTableExpander(this.CombinedConcreteTablePanel, this.allSelectedTemplates,
                //    this.allSelectedTemplateValues, ObjectSelection_Activated, ObjectDeselection_Activated);

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
                this.SendToMySql.IsEnabled = true;
                this.ExportIFC.IsEnabled = true;

                Dispatcher.FromThread(newWindowThread).InvokeShutdown();
            }

            catch (Exception ex)
            {
                Dispatcher.FromThread(newWindowThread).InvokeShutdown();

                MessageBox.Show("Something went wrong!");

                MessageBox.Show(ex.ToString());
            }
        }

        private void Concrete_Save_Clicked(object sender, RoutedEventArgs e)
        {
            Stream stream = null;
            StreamWriter streamWriter = null;

            this.saveData.Add("SelectedConcreteTemplatesForLayers", this.selectedConcreteTemplatesForLayers);
            this.saveData.Add("AllSelectedTemplates", this.allSelectedTemplates);
            this.saveData.Add("AllSelectedTemplateValues", this.allSelectedTemplateValues);
            this.saveData.Add("DissipatedConcreteTablePanel", this.DissipatedConcreteTablePanel);
            this.saveData.Add("CombinedConcreteTablePanel", this.CombinedConcreteTablePanel);
            this.saveData.Add("LayerPropertyColumnHeaders", this.layerPropertyColumnHeaders);

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
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            Stream stream = null;

            string pathToFile = "";

            // Read The File
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    stream = openFileDialog.OpenFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. original error: " + ex.Message);
                    return;
                }

                if (stream != null)
                {
                    pathToFile = openFileDialog.FileName;

                    try
                    {
                        loadData = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(pathToFile));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not open the file. original error: " + ex.Message);
                        return;
                    }

                    // Load The Project
                    try
                    {
                        Dictionary<string, string> tempSelectedConcreteTemplatesForLayers =
                            ((JObject)loadData["SelectedConcreteTemplatesForLayers"]).ToObject<Dictionary<string, string>>();

                        for (int i = 0; i < this.ConcreteTemplateGrid.Children.Count; i++)
                        {
                            if (this.ConcreteTemplateGrid.Children[i].GetType().ToString().Split('.').Last() == "DockPanel")
                            {
                                DockPanel tempDockPanel = (DockPanel)this.ConcreteTemplateGrid.Children[i];

                                Label tempLabel = (Label)tempDockPanel.Children[0];

                                if (tempSelectedConcreteTemplatesForLayers.Keys.Contains(tempLabel.Content.ToString()))
                                {
                                    ComboBox tempComboBox = LogicalTreeHelper.FindLogicalNode(this.ConcreteTemplateGrid,
                                            "ConcreteTemplates_" + tempLabel.Name.Split('_').Last()) as ComboBox;

                                    tempComboBox.Text = tempSelectedConcreteTemplatesForLayers[tempLabel.Content.ToString()];
                                }

                                else
                                {
                                    MessageBox.Show("No saved data exists for layer " + "\"" + tempLabel.Content.ToString() + "\"" + ".");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Data is corupted, " + ex.Message);
                    }
                }

                // Handeling Selected File Not Exist.
                else
                {
                    MessageBox.Show("Error: File not found.");
                    return;
                }
            }
        }

        private void Export_Excel_Clicked(object sender, RoutedEventArgs e)
        {
            ExcelMethods.ExportExcel(this.DissipatedConcreteTablePanel, this.CombinedConcreteTablePanel, this.layerPropertyColumnHeaders);
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

        //------------------------------------SQL------------------------------------

        private void Send_To_MySql(object sender, RoutedEventArgs e)
        {
            string connStr = @"server=172.18.30.54;userid=TurnerUser;password=VDCTurner2021";

            MySqlConnection conn = null;

            try
            {
                string mySqlTableName = Interaction.InputBox("Please enter project's name.", "MYSQL", RunQTO.doc.Name.Replace(".3dm", "")).Replace('-', '_');

                if (mySqlTableName == string.Empty)
                {
                    throw new ArgumentException("Project name has to be selected.", "mySqlProjectName");
                }

                string mySqlProjectName = "concrete_" + mySqlTableName;

                conn = new MySqlConnection(connStr);
                conn.Open();

                MySqlMethods.CreateMySqlDatabase(mySqlProjectName, conn);

                MySqlMethods.CreateMySqlTable(mySqlProjectName, mySqlTableName, this.DissipatedConcreteTablePanel, conn);

                MySqlMethods.CreateMySqlTable(mySqlProjectName, mySqlTableName + "_ProjectBased", this.CombinedConcreteTablePanel, conn);

                MessageBox.Show("Successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        private void Export_IFC_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "IFC |*.ifc";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string outputPath = saveFileDialog.FileName;

                    IfcStore testProject = IFCMethods.CreateandInitIFCModel("Test Project");

                    IfcBuilding building = IFCMethods.CreateBuilding(testProject, "Test Building");

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allWalls);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allBeams);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allColumns);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allContinuousFootings);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allFootings);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allSlabs);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allCurbs);

                    IFCMethods.CreateAndAddIFCElement(testProject, building, this.allStyrofoams);

                    testProject.SaveAs(outputPath);

                    MessageBox.Show("Export was successful.");

                }

                else
                {
                    MessageBox.Show("Export was canceled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
    }
}
