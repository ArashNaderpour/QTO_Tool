﻿using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTO_Tool
{
    class ColumnTemplate
    {
        public Brep geometry { get; set; }
        public System.Drawing.Color color { get; set; }
        public string layerName { get; set; }

        public string floor { get; set; }
        public string nameAbb { get; set; }
        public string id { get; set; }
        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();
        public double volume { get; set; }
        public double height { get; set; }
        public double sideArea { get; set; }
        public bool rectangular = true;

        public string type = "ColumnTemplate";

        Dictionary<string, Point3d> topAndBottomFaceCenters = new Dictionary<string, Point3d>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Foot", "Square Foot", "N/A" };

        public ColumnTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, bool _rectangular, Dictionary<double, string> floorElevations)
        {
            this.layerName = _layerName;

            this.rectangular = _rectangular;

            this.color = layerColor;

            geometry = (Brep)rhobj.Geometry;

            id = rhobj.Id.ToString();

            for (int i = 0; i < layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(geometry);
            volume = Math.Round(mass_properties.Volume * RunQTO.volumeConversionFactor, 2);

            this.sideArea = this.SideArea(geometry);

            if (floorElevations.Count > 0)
            {
                this.floor = Methods.FindFloor(floorElevations, this.topAndBottomFaceCenters["Bottom"].Z);
            }
            else
            {
                this.floor = "-";
            }

            this.height = this.Height(topAndBottomFaceCenters);
        }

        double SideArea(Brep brep)
        {
            double area = 0;

            List<double> faceAreas = new List<double>();
            List<double> faceCentersElevation = new List<double>();
            List<Point3d> faceCenters = new List<Point3d>();

            Point3d center;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                center = area_properties.Centroid;

                faceAreas.Add(area_properties.Area);
                faceCentersElevation.Add(center.Z);
                faceCenters.Add(center);

                if (this.rectangular)
                {
                    area += area_properties.Area;
                }
            }

            int topFaceIndex = faceCentersElevation.IndexOf(faceCentersElevation.Max());
            int bottomFaceIndex = faceCentersElevation.IndexOf(faceCentersElevation.Min());

            topAndBottomFaceCenters.Add("Top", faceCenters[topFaceIndex]);
            topAndBottomFaceCenters.Add("Bottom", faceCenters[bottomFaceIndex]);

            if (this.rectangular)
            {
                area -= (faceAreas[topFaceIndex] + faceAreas[bottomFaceIndex]);

                area = Math.Round(area, 2);
            }

            return area;
        }

        double Height(Dictionary<string, Point3d> _topAndBottomFaceCenters)
        {
            double height = 0;

            height = _topAndBottomFaceCenters["Top"].Z - _topAndBottomFaceCenters["Bottom"].Z;

            height = Math.Round(height, 2);

            return height;
        }
    }
}
