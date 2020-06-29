using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
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
    /// Interaction logic for QTOUI.xaml
    /// </summary>
    /// 
    public partial class QTOUI : Window
    {
        List<BeamTemplate> allBeams = new List<BeamTemplate>();
        List<ColumnTemplate> allColumns = new List<ColumnTemplate>();
        List<ContinousFootingTemplate> allContinousFootings = new List<ContinousFootingTemplate>();
        List<CurbTemplate> allCurbs = new List<CurbTemplate>();
        List<FootingTemplate> allFootings = new List<FootingTemplate>();
        List<SlabTemplate> allSlabs = new List<SlabTemplate>();
        List<WallTemplate> allWalls = new List<WallTemplate>();
        List<StyrofoamTemplate> allStyrofoams = new List<StyrofoamTemplate>();

        List<string> quantityValues = new List<string>();

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
                    CombineValuesToggle.IsEnabled = true;

                }
                else
                {
                    this.ConcreteTemplateGrid.Children.Clear();
                    this.ConcreteTemplateGrid.RowDefinitions.Clear();
                    UIMethods.GenerateLayerTemplate(this.ConcreteTemplateGrid);
                }
            }
            if (this.ExteriorIsIncluded.IsChecked == true)
            {

            }

            if (this.ConcreteIsIncluded.IsChecked == false && this.ExteriorIsIncluded.IsChecked == false)
            {
                MessageBox.Show("Please select atleast one of the methods.");
            }
        }

        private void Calculate_Concrete_Clicked(object sender, RoutedEventArgs e)
        {
            ConcreteTablePanel.Children.Clear();

            double angleThreshold = this.AngleThresholdSlider.Value;

            ComboBox selectedConcreteTemplate;

            RhinoObject[] rhobjs;

            string selectedTemplate;

            string layerName;

            string objType;

            List<object> layerTemplates;

            for (int i = 0; i < this.ConcreteTemplateGrid.RowDefinitions.Count; i++)
            {
                selectedConcreteTemplate = LogicalTreeHelper.FindLogicalNode(this.ConcreteTemplateGrid,
                    "ConcreteTemplates_" + i.ToString()) as ComboBox;

                selectedTemplate = selectedConcreteTemplate.SelectedItem.ToString().Split(':').Last().Replace(" ", String.Empty);

                layerName = RunQTO.doc.Layers[i].Name;

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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                    }
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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "HEIGHT", "SIDE AREA", "ISOLATE" };
                    }
                }

                if (selectedTemplate == "Continous Footing")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        ContinousFootingTemplate continousFooting = new ContinousFootingTemplate(rhobjs[j]);

                        allContinousFootings.Add(continousFooting);
                        layerTemplates.Add(continousFooting);
                    }

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                    }
                }

                if (selectedTemplate == "Curb")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        CurbTemplate curb = new CurbTemplate(rhobjs[j]);

                        allCurbs.Add(curb);
                        layerTemplates.Add(curb);
                    }

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "SIDE AREA", "LENGTH", "ISOLATE" };
                    }
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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "TOP AREA", "BOTTOM AREA", "SIDE AREA", "ISOLATE" };
                    }
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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "END AREA",
                            "SIDE-1", "SIDE-2", "LENGTH", "OPENING AREA" ,"ISOLATE" };
                    }
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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "GROSS VOLUME", "NET VOLUME", "TOP AREA", "BOTTOM AREA", "EDGE AREA", "PERIMETER", "OPENING PERIMETER", "ISOLATE" };
                    }
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

                    if (CombineValuesToggle.IsChecked == true)
                    {
                        quantityValues = new List<string>() { "COUNT", "NAME", "VOLUME", "ISOLATE" };
                    }
                }

                if (CombineValuesToggle.IsChecked == true)
                {
                    UIMethods.GenerateAccumulatedSlabTableExpander(this.ConcreteTablePanel, layerName, selectedTemplate, layerTemplates, quantityValues);

                    quantityValues.Clear();
                }
            }

            if (CombineValuesToggle.IsChecked == false)
            {
                MessageBox.Show("Project Based");
            }

        }
    }
}
