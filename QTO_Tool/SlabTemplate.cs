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
        public double grossVolume { get; set; }
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double edgeArea { get; set; }
        public double perimeter { get; set; }
        public double openingPerimeter { get; set; }

        public string type = "SlabTemplate";

        private List<Brep> topBrepFaces = new List<Brep>();

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

            var mass_properties = VolumeMassProperties.Compute(geometry);
            this.netVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            mass_properties = VolumeMassProperties.Compute(geometry.RemoveHoles(0.01));
            this.grossVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            this.topArea = TopArea(geometry, angleThreshold);

            this.bottomArea = BottomArea(geometry, angleThreshold);

            this.edgeArea = EdgeArea(geometry);

            this.PerimeterAndOpeningPerimeter(topBrepFaces);
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

                int bottomFaceIndex = centerZValues.IndexOf(centerZValues.Min());

                area = faceAreas[bottomFaceIndex];

                this.topBrepFaces.Add(brep.Faces[bottomFaceIndex].DuplicateFace(false));
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
            Brep joinedBrep = Brep.JoinBreps(breps, RunQTO.doc.ModelAbsoluteTolerance)[0];

            joinedBrep.MergeCoplanarFaces(RunQTO.doc.ModelAbsoluteTolerance);

            Curve[] brepBoundaryCurves = Curve.JoinCurves(joinedBrep.DuplicateNakedEdgeCurves(true, true));
            //List<Curve> projectedBrepBoundaryCurves = new List<Curve>();

            List<double> projectedBrepBoundaryCurveAreas = new List<double>();
            List<double> projectedBrepBoundaryCurveLengths = new List<double>();

            for (int i = 0; i < brepBoundaryCurves.Length; i++)
            {
                if (brepBoundaryCurves[i].IsClosed)
                {
                    Curve projectedCurve = Curve.ProjectToPlane(brepBoundaryCurves[i], Plane.WorldXY);
                    var areaProps = AreaMassProperties.Compute(projectedCurve);

                    //projectedBrepBoundaryCurves.Add(projectedCurve);
                    projectedBrepBoundaryCurveAreas.Add(areaProps.Area);
                    projectedBrepBoundaryCurveLengths.Add(projectedCurve.GetLength());
                }
            }

            List<int> indexOrder = projectedBrepBoundaryCurveAreas
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(item => item.Value)
                .Select(item => item.Index)
                .ToList();

            List<double> sortedProjectedBrepBoundaryCurveAreas = indexOrder.Select(index => projectedBrepBoundaryCurveAreas[index]).ToList();
            //List<Curve> sortedProjectedBrepBoundaryCurves = indexOrder.Select(index => projectedBrepBoundaryCurves[index]).ToList();
            List<double> sortedProjectedBrepBoundaryCurveLengths = indexOrder.Select(index => projectedBrepBoundaryCurveLengths[index]).ToList();

            this.perimeter = Math.Round(sortedProjectedBrepBoundaryCurveLengths.Last(), 2);

            sortedProjectedBrepBoundaryCurveAreas.RemoveAt(sortedProjectedBrepBoundaryCurveAreas.Count - 1);
            this.openingPerimeter = Math.Round(sortedProjectedBrepBoundaryCurveAreas.Sum(), 2);
        }

        //double OpeningPerimeter(List<Brep> breps)
        //{
        //    double result = 0;

        //    if (brepBoundaryCurveLengths.Count == 1)
        //    {
        //        result = Math.Round(result, 2);

        //        return result;
        //    }

        //    else
        //    {
        //        foreach (double curveLength in brepBoundaryCurveLengths)
        //        {
        //            if (curveLength != brepBoundaryCurveLengths.Max())
        //            {
        //                result += curveLength;
        //            }
        //        }
        //        result = Math.Round(result, 2);

        //        return result;
        //    }
        //}

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
