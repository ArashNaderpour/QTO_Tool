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
        public double grossVolume = double.MaxValue;
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
        private Brep bottomBrepFace;
        //private Brep boundingBox;
        private List<BrepFace> sideAndEndFaces = new List<BrepFace>();
        private List<double> sideAndEndFaceAreas = new List<double>();
        private List<double> brepBoundaryCurveLengths = new List<double>();
        private List<double> endFaceAreas = new List<double>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Cubic Yard", "Square Foot", "Square Foot",
            "Square Foot", "Square Foot", "Foot", "Foot", "N/A" };

        public WallTemplate(RhinoObject rhobj, string layerName, double angleThreshold)
        {
            Brep tempBrep = (Brep)rhobj.Geometry;

            this.name = rhobj.Name;

            this.id = rhobj.Id.ToString();

            var mass_properties = VolumeMassProperties.Compute(tempBrep);
            this.netVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            Dictionary<string, double> topAndBottomArea = this.TopAndBottomArea(tempBrep, angleThreshold);

            this.topArea = Math.Round(topAndBottomArea["Top Area"], 2);

            this.bottomArea = Math.Round(topAndBottomArea["Bottom Area"], 2);

            // Using the method that calculates Sides and End areas
            SidesAndOpeingArea();

            this.endArea = Math.Round(this.endFaceAreas.Sum(), 2);

            this.length = Math.Round(Length(), 2);
        }

        Dictionary<string, double> TopAndBottomArea(Brep brep, double angleThreshold)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            double topArea = 0;

            double bottomArea = 0;

            List<double> upfacingFaceElevations = new List<double>();
            List<double> upfacingFaceAreas = new List<double>();
            List<Brep> upfacingFaces = new List<Brep>();

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

                    //Calculating Gross Volume
                    Plane frame;
                    brep.Faces[i].FrameAt(u, v, out frame);

                    Brep tempBoundingBox = brep.GetBoundingBox(frame).ToBrep();

                    var mass_properties = VolumeMassProperties.Compute(tempBoundingBox);
                    double tempBoundingBoxVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

                    if (tempBoundingBoxVolume < this.grossVolume)
                    {
                        this.grossVolume = tempBoundingBoxVolume;
                        //this.boundingBox = tempBoundingBox;
                    }

                    double dotProduct = Math.Round(Vector3d.Multiply(normal, Vector3d.ZAxis), 2);

                    if (dotProduct > angleThreshold && dotProduct <= 1)
                    {
                        upfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        upfacingFaceAreas.Add(area_properties.Area);
                        upfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }

                    else if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        downfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        downfacingFaceAreas.Add(area_properties.Area);
                        downfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }

                    else
                    {
                        this.sideAndEndFaces.Add(brep.Faces[i]);
                        this.sideAndEndFaceAreas.Add(area_properties.Area);
                    }
                }
            }

            double tempFaceElevation = upfacingFaceElevations.Max();
            int topFaceIndex = upfacingFaceElevations.IndexOf(tempFaceElevation);

            this.topBrepFace = upfacingFaces[topFaceIndex];

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

        void SidesAndOpeingArea()
        {
            double netSideArea = sideAndEndFaceAreas.Max();

            BrepFace bigSideFace = sideAndEndFaces[sideAndEndFaceAreas.IndexOf(netSideArea)];

            var area_properties = AreaMassProperties.Compute(bigSideFace);

            Point3d center = area_properties.Centroid;

            double u, v;

            if (bigSideFace.ClosestPoint(center, out u, out v))
            {
                Plane frame;
                bigSideFace.FrameAt(u, v, out frame);

                BoundingBox bigSideFaceBoundingBox = bigSideFace.GetMesh(new MeshType()).GetBoundingBox(true);

                this.sideArea_1 = Math.Round(bigSideFaceBoundingBox.Area/2, 2);

                this.openingArea = Math.Abs(Math.Round(this.sideArea_1 - netSideArea, 2));

            }

            sideAndEndFaceAreas.Remove(sideAndEndFaceAreas.Max());
            this.sideArea_2 = Math.Round(sideAndEndFaceAreas.Max() + this.openingArea, 2);

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
                double edgeLength = this.topBrepFace.Edges[i].GetLength();

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
