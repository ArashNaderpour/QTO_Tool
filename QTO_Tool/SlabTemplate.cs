using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTO_Tool
{
    class SlabTemplate
    {
        public string name { get; set; }
        public double grossVolume { get; set; }
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double edgeArea { get; set; }
        public double perimeter { get; set; }
        public double openingPerimeter { get; set; }

        public SlabTemplate(RhinoObject rhobj, string layerName, double angleThreshold)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = layerName;

            var mass_properties = VolumeMassProperties.Compute(tempBrep);
            netVolume = Math.Round(mass_properties.Volume, 2);

            mass_properties = VolumeMassProperties.Compute(tempBrep.RemoveHoles(0.01));
            grossVolume = Math.Round(mass_properties.Volume, 2);

            topArea = TopArea(tempBrep, angleThreshold);

            bottomArea = BottomArea(tempBrep, angleThreshold);
            MessageBox.Show("Top = " + topArea.ToString() + "----" + "Bottom = " + bottomArea.ToString());
           
        }

        double TopArea(Brep brep, double angleThreshold)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                Point3d center = area_properties.Centroid;

                double u, v;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    Vector3d normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    double dotProduct = Vector3d.Multiply(normal, Vector3d.ZAxis);

                    if (dotProduct > angleThreshold && dotProduct <= 1)
                    {
                        area = Math.Round(area_properties.Area, 2);
                    }
                }
            }

            return area;
        }

        double BottomArea(Brep brep, double angleThreshold)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                Point3d center = area_properties.Centroid;

                double u, v;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    Vector3d normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    double dotProduct = Vector3d.Multiply(normal, Vector3d.ZAxis);

                    if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        area = Math.Round(area_properties.Area, 2);
                    }
                }
            }

            return area;
        }
    }
}
