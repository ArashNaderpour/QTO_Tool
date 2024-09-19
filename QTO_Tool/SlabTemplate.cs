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
        public Brep geometry { get; set; }
        public System.Drawing.Color color { get; set; }
        public string layerName { get; set; }
        public string nameAbb { get; set; }
        public string id { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();
        public string floor { get; set; }
        public double grossVolume { get; set; }
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double edgeArea { get; set; }
        public double perimeter { get; set; }
        public double openingPerimeter { get; set; }

        public string type = "SlabTemplate";

        private List<Brep> topBrepFaces = new List<Brep>();

        private List<double> downfacingFaceElevations = new List<double>();

        public Dictionary<string, BeamTemplate> beams = new Dictionary<string, BeamTemplate>();
        //public List<BeamTemplate> beams = new List<BeamTemplate>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Cubic Yard", "Square Foot", "Square Foot", "Square Foot", "Foot", "Foot", "N/A" };

        public SlabTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, double angleThreshold, Dictionary<double, string> floorElevations)
        {
            this.color = layerColor;

            this.geometry = (Brep)rhobj.Geometry;

            this.layerName = _layerName;

            this.id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            this.nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(this.geometry);
            this.netVolume = Math.Round(mass_properties.Volume * RunQTO.volumeConversionFactor, 2);

            this.grossVolume = Math.Round(Methods.CalculateGrossVolume(this.geometry) * RunQTO.volumeConversionFactor, 2);

            this.topArea = TopArea(geometry, angleThreshold);

            this.bottomArea = BottomArea(geometry, angleThreshold);

            if (floorElevations.Count > 0)
            {
                this.floor = Methods.FindFloor(floorElevations, this.downfacingFaceElevations.Min());
            }
            else
            {
                this.floor = "-";
            }

            this.edgeArea = EdgeArea(geometry);

            this.PerimeterAndOpeningPerimeter(this.topBrepFaces);
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
                        area += Math.Round(area_properties.Area, 2);

                        this.topBrepFaces.Add(brep.Faces[i].DuplicateFace(false));
                    }
                }
            }

            if (area == 0 && brep.Faces.Count > 0)
            {
                List<double> centerZValues = new List<double>();
                List<double> faceAreas = new List<double>();

                for (int i = 0; i < brep.Faces.Count; i++)
                {
                    var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                    Point3d center = area_properties.Centroid;

                    centerZValues.Add(center.Z);

                    faceAreas.Add(Math.Round(area_properties.Area, 2));
                }

                int topFaceIndex = centerZValues.IndexOf(centerZValues.Max());

                area = faceAreas[topFaceIndex];

                this.topBrepFaces.Add(brep.Faces[topFaceIndex].DuplicateFace(false));
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
                        area += Math.Round(area_properties.Area, 2);

                        this.downfacingFaceElevations.Add(center.Z);
                    }
                }
            }

            if (area == 0 && brep.Faces.Count > 0)
            {
                List<double> centerZValues = new List<double>();
                List<double> faceAreas = new List<double>();

                for (int i = 0; i < brep.Faces.Count; i++)
                {
                    var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                    Point3d center = area_properties.Centroid;

                    centerZValues.Add(center.Z);
                    faceAreas.Add(Math.Round(area_properties.Area, 2));
                }

                int bottomFaceIndex = centerZValues.IndexOf(centerZValues.Min());

                this.downfacingFaceElevations.Add(centerZValues.Min());

                area = faceAreas[bottomFaceIndex];
            }

            return area;
        }

        double EdgeArea(Brep brep)
        {
            double area = 0;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                double faceArea = Math.Round(area_properties.Area, 2);

                area += faceArea;
            }

            area -= (this.topArea + this.bottomArea);

            area = Math.Round(area, 2);

            return area;
        }

        void PerimeterAndOpeningPerimeter(List<Brep> breps)
        {
            List<Brep> projectedBreps = new List<Brep>();

            // Create the XY plane
            Plane xyPlane = Plane.WorldXY;

            // Define the projection direction (in this case, along the Z axis)
            Vector3d projectionDirection = new Vector3d(0, 0, -1); // or use (0, 0, 1) depending on desired direction

            // Get the transformation that projects geometry along the specified direction onto the plane
            Transform projectionTransform = Transform.ProjectAlong(xyPlane, projectionDirection);

            // Iterate through each brep and apply the projection transform
            foreach (Brep brep in breps)
            {
                // Duplicate the brep to avoid modifying the original
                Brep projectedBrep = brep.DuplicateBrep();

                // Apply the projection transform
                projectedBrep.Transform(projectionTransform);

                // Add the projected brep to the result list
                projectedBreps.Add(projectedBrep);
            }

            Brep joinedBrep = Brep.JoinBreps(projectedBreps, 0.01)[0];

            joinedBrep.MergeCoplanarFaces(0.01);

            this.openingPerimeter = 0;

            foreach (BrepLoop loop in joinedBrep.Loops)
            {
                if (loop.LoopType == BrepLoopType.Inner)
                {

                    Curve innerLoopCurve = loop.To3dCurve();
                    this.openingPerimeter += innerLoopCurve.GetLength();
                }
                else
                {
                    this.perimeter = Math.Round(joinedBrep.Faces[0].OuterLoop.To3dCurve().GetLength(), 2);
                }
            }

            this.openingPerimeter = Math.Round(this.openingPerimeter, 2);
        }

        public void UpdateNetVolumeAndBottomAreaWithBeams()
        {
            if (this.beams.Count > 0)
            {
                Double intersectionVolume = 0;
                Double intersectedBeamBottomArea = 0;

                foreach (var item in this.beams)
                {
                    Brep[] intersectionBreps = Brep.CreateBooleanIntersection(this.geometry, item.Value.geometry, RunQTO.doc.ModelAbsoluteTolerance);

                    if (intersectionBreps != null && intersectionBreps.Length > 0)
                    {
                        foreach (Brep intersectionBrep in intersectionBreps)
                        {
                            var intersection_mass_properties = VolumeMassProperties.Compute(intersectionBrep);
                            var beam_mass_properties = VolumeMassProperties.Compute(item.Value.geometry);

                            if (intersection_mass_properties != null && intersection_mass_properties.Volume > 5 && intersection_mass_properties.Volume < beam_mass_properties.Volume)
                            {
                                intersectionVolume += intersection_mass_properties.Volume * 0.037037;

                                intersectedBeamBottomArea += item.Value.bottomArea;
                            }
                        }
                    }
                }

                this.netVolume -= intersectionVolume;
                Math.Round(this.netVolume, 2);

                this.bottomArea -= intersectedBeamBottomArea;
            }
        }
    }
}
