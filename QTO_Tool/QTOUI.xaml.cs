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
        List<BeamTemplate> beams = new List<BeamTemplate>();
        List<ColumnTemplate> columns = new List<ColumnTemplate>();
        List<ContinousFootingTemplate> continousFootings = new List<ContinousFootingTemplate>();
        List<CurbTemplate> curbs = new List<CurbTemplate>();
        List<FootingTemplate> footings = new List<FootingTemplate>();
        List<SlabTemplate> slabs = new List<SlabTemplate>();
        List<StyrofoamTemplate> styrofoams = new List<StyrofoamTemplate>();

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
            double angleThreshold = this.AngleThresholdSlider.Value;

            ComboBox selectedConcreteTemplate;

            for (int i =0; i < this.ConcreteTemplateGrid.RowDefinitions.Count; i++)
            {
                selectedConcreteTemplate = LogicalTreeHelper.FindLogicalNode(this.ConcreteTemplateGrid,
                    "ConcreteTemplates_" + i.ToString()) as ComboBox;

                string selectedTemplate = selectedConcreteTemplate.SelectedItem.ToString().Split(':').Last().Replace(" ", String.Empty);

                string layerName = RunQTO.doc.Layers[i].Name;

                RhinoObject[] rhobjs;

                if (selectedTemplate == "Beam")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        BeamTemplate beam = new BeamTemplate(rhobjs[j]);

                        beams.Add(beam);
                    }
                }

                if (selectedTemplate == "Column")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        ColumnTemplate column = new ColumnTemplate(rhobjs[j]);

                        columns.Add(column);
                    }
                }

                if (selectedTemplate == "Continous Footing")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        ContinousFootingTemplate continousFooting = new ContinousFootingTemplate(rhobjs[j]);

                        continousFootings.Add(continousFooting);
                    }
                }

                if (selectedTemplate == "Curb")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        CurbTemplate curb = new CurbTemplate(rhobjs[j]);

                        curbs.Add(curb);
                    }
                }

                if (selectedTemplate == "Footing")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        FootingTemplate footing = new FootingTemplate(rhobjs[j]);

                        footings.Add(footing);
                    }
                }

                if (selectedTemplate == "Slab")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        SlabTemplate slab = new SlabTemplate(rhobjs[j], layerName, angleThreshold);

                        slabs.Add(slab);
                    }
                }

                if (selectedTemplate == "Styrofoam")
                {
                    rhobjs = RunQTO.doc.Objects.FindByLayer(layerName);

                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        StyrofoamTemplate styrofoam = new StyrofoamTemplate(rhobjs[j], layerName);

                        styrofoams.Add(styrofoam);
                    }
                }
            }
        }

    }
}
