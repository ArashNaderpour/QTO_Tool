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
    class WallTemplate
    {
        public string name { get; set; }
        public string id { get; set; }
        public double grossVolume { get; set; }
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double endArea { get; set; }
        public double sideArea_1 { get; set; }
        public double sideArea_2 { get; set; }
        public double length { get; set; }
        public double openingArea { get; set; }

        private double bottomArea;

        static string type = "WallTemplate";
        private Brep topBrepFace;
        private Plane topBrepFaceFrame;
        private Brep bottomBrepFace;
        private Brep boundingBox;
        private List<Brep> sideAndEndFaces = new List<Brep>();
        private List<double> sideAndEndFaceAreas = new List<double>();
        private List<double> brepBoundaryCurveLengths = new List<double>();
        private List<double> endFaceAreas = new List<double>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Cubic Yard", "Square Foot", "Square Foot",
            "Square Foot", "Square Foot", "Foot", "Foot", "N/A" };

        public WallTemplate(RhinoObject rhobj, string layerName, double angleThreshold)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            name = rhobj.Name;

            id = rhobj.Id.ToString();

            var mass_properties = VolumeMassProperties.Compute(tempBrep);
            netVolume = Math.Round(mass_properties.Volume, 2);

            Dictionary<string, double> topAndBottomArea = this.TopAndBottomArea(tempBrep, angleThreshold);

            topArea = topAndBottomArea["Top Area"];

            bottomArea = topAndBottomArea["Bottom Area"];

            this.boundingBox = tempBrep.GetBoundingBox(this.topBrepFaceFrame).ToBrep();

            mass_properties = VolumeMassProperties.Compute(this.boundingBox);
            grossVolume = Math.Round(mass_properties.Volume, 2);

            // Using the method that calculates Sides and End areas
            SidesAndOpeingArea(boundingBox, sideAndEndFaceAreas, angleThreshold);

            endArea = Math.Round(this.endFaceAreas.Sum(), 2);

            length = Length();
        }

        Dictionary<string, double> TopAndBottomArea(Brep brep, double angleThreshold)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            double topArea = 0;

            double bottomArea = 0;

            List<double> upfacingFaceElevations = new List<double>();
            List<double> upfacingFaceAreas = new List<double>();
            List<Brep> upfacingFaces = new List<Brep>();
            List<Plane> upfacingFacesFrame = new List<Plane>();

            List<double> downfacingFaceElevations = new List<double>();
            List<double> downfacingFaceAreas = new List<double>();
            List<Brep> downfacingFaces = new List<Brep>();

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
                        upfacingFaceElevations.Add(center.Z);
                        upfacingFaceAreas.Add(Math.Round(area_properties.Area, 2));
                        upfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                        Plane frame;
                        brep.Faces[i].FrameAt(u, v, out frame);
                        upfacingFacesFrame.Add(frame);
                    }

                   else if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        downfacingFaceElevations.Add(center.Z);
                        downfacingFaceAreas.Add(Math.Round(area_properties.Area, 2));
                        downfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }

                    else
                    {
                        this.sideAndEndFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.sideAndEndFaceAreas.Add(Math.Round(area_properties.Area, 2));
                    }
                }
            }

            double tempFaceElevation = upfacingFaceElevations.Max();
            int topFaceIndex = upfacingFaceElevations.IndexOf(tempFaceElevation);

            this.topBrepFace = upfacingFaces[topFaceIndex];
            this.topBrepFaceFrame = upfacingFacesFrame[topFaceIndex];

            while (upfacingFaceElevations.Contains(tempFaceElevation))
            {
                topArea += upfacingFaceAreas[upfacingFaceElevations.IndexOf(tempFaceElevation)];
                upfacingFaceAreas.Remove(upfacingFaceAreas[upfacingFaceElevations.IndexOf(tempFaceElevation)]);

                upfacingFaceElevations.Remove(tempFaceElevation);
            }

            tempFaceElevation = downfacingFaceElevations.Min();
            int bottomFaceIndex = downfacingFaceElevations.IndexOf(tempFaceElevation);

            this.bottomBrepFace = downfacingFaces[bottomFaceIndex];

            while (downfacingFaceElevations.Contains(tempFaceElevation))
            {
                bottomArea += downfacingFaceAreas[downfacingFaceElevations.IndexOf(tempFaceElevation)];
                downfacingFaceAreas.Remove(downfacingFaceAreas[downfacingFaceElevations.IndexOf(tempFaceElevation)]);
                downfacingFaceElevations.Remove(tempFaceElevation);
            }

            result.Add("Top Area", topArea);
            result.Add("Bottom Area", bottomArea);

            upfacingFaceAreas.Remove(topArea);
            this.endFaceAreas.AddRange(upfacingFaceAreas);

            downfacingFaceAreas.Remove(bottomArea);
            this.endFaceAreas.AddRange(downfacingFaceAreas);

            return result;
        }

        void SidesAndOpeingArea(Brep brep, List<double> sideAndEndFaceAreas, double angleThreshold)
        {
            List<double> bbsideAndEndFaceAreas = new List<double>();

            double netSideArea = sideAndEndFaceAreas.Max();

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

                    if ((dotProduct > angleThreshold && dotProduct <= 1) == false &&
                        (dotProduct < -angleThreshold && dotProduct >= -1) == false)
                    {
                        bbsideAndEndFaceAreas.Add(Math.Round(area_properties.Area, 2));
                    }
                }
            }

            this.openingArea = bbsideAndEndFaceAreas.Max() - netSideArea;
            
            this.sideArea_1 = sideAndEndFaceAreas.Max() + this.openingArea;

            sideAndEndFaceAreas.Remove(sideAndEndFaceAreas.Max());
            this.sideArea_2 = sideAndEndFaceAreas.Max() + this.openingArea;

            //adding end face areas to the list
            sideAndEndFaceAreas.Remove(sideAndEndFaceAreas.Max());
            this.endFaceAreas.AddRange(sideAndEndFaceAreas);
        }

        double Length()
        {
            double length = 0;

            List<double> edgeLengths = new List<double>();

            for (int i = 0; i < this.topBrepFace.Edges.Count; i++)
            {
                double edgeLength = Math.Round(this.topBrepFace.Edges[i].GetLength(), 2);

                edgeLengths.Add(edgeLength);

                length += edgeLength;
            }

            edgeLengths.Sort();

            length -= (edgeLengths[0] + edgeLengths[1]);

            length /= 2;

            return length;
        }
    }
}
