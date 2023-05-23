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
        public string nameAbb { get; set; }

        public string id { get; set; }

        public string layerName { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();

        public double grossVolume = double.MaxValue;
        public double netVolume { get; set; }
        public double topArea { get; set; }
        public double bottomArea { get; set; }
        public double endArea { get; set; }
        public double sideArea_1 { get; set; }
        public double sideArea_2 { get; set; }
        public double length { get; set; }
        public double openingArea { get; set; }

        public Brep geometry { get; set; }

        public System.Drawing.Color color { get; set; }

        public string type = "WallTemplate";

        private List<double> upfacingFaceElevations = new List<double>();
        private List<double> upfacingFaceAreas = new List<double>();
        private List<Brep> upfacingFaces = new List<Brep>();
        private List<Point3d> upfacingFacesCenters = new List<Point3d>();
        private List<Vector3d> upfacingFacesNormals = new List<Vector3d>();

        private List<Brep> topFaces = new List<Brep>();

        private List<double> downfacingFaceElevations = new List<double>();
        private List<double> downfacingFaceAreas = new List<double>();
        private List<Brep> downfacingFaces = new List<Brep>();
        private List<Point3d> downfacingFacesCenters = new List<Point3d>();
        private List<Vector3d> downfacingFacesNormals = new List<Vector3d>();

        private List<Brep> bottomFaces = new List<Brep>();

        private List<Brep> sideAndEndFaces = new List<Brep>();
        private List<Brep> endFaces = new List<Brep>();
        private List<Brep> sideFaces = new List<Brep>();
        private List<double> sideAndEndFaceAreas = new List<double>();
        private List<Curve> sideEdges = new List<Curve>();
        private List<double> endFaceAreas = new List<double>();
        private List<double> sideFaceAreas = new List<double>();

        public static string[] units = { "N/A", "N/A", "Cubic Yard", "Cubic Yard", "Square Foot", "Square Foot",
            "Square Foot", "Square Foot", "Foot", "Foot", "N/A" };

        public WallTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, double angleThreshold)
        {
            this.layerName = _layerName;

            this.color = layerColor;

            this.geometry = (Brep)rhobj.Geometry;

            this.id = rhobj.Id.ToString();

            for (int i = 0; i < _layerName.Split('_').ToList().Count; i++)
            {
                parsedLayerName.Add("C" + (1 + i).ToString(), _layerName.Split('_').ToList()[i]);
            }

            nameAbb = parsedLayerName["C1"] + " " + parsedLayerName["C2"];

            var mass_properties = VolumeMassProperties.Compute(this.geometry);
            this.netVolume = Math.Round(mass_properties.Volume * 0.037037, 2);

            Dictionary<string, double> topAndBottomArea = this.TopAndBottomArea(this.geometry, angleThreshold);

            this.topArea = Math.Round(topAndBottomArea["Top Area"], 2);

            this.bottomArea = Math.Round(topAndBottomArea["Bottom Area"], 2);

            this.SidesAndEndAndOpeingArea();

            this.grossVolume = this.GrossVolume();
        }

        Dictionary<string, double> TopAndBottomArea(Brep brep, double angleThreshold)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            double topArea = 0;
            double bottomArea = 0;

            Vector3d normal;
            double u, v;
            Point3d center;

            Plane frame;

            double dotProduct;

            Ray3d ray;
            Mesh mesh;

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(brep.Faces[i]);

                center = area_properties.Centroid;

                if (brep.Faces[i].ClosestPoint(center, out u, out v))
                {
                    normal = brep.Faces[i].NormalAt(u, v);

                    normal.Unitize();

                    brep.Faces[i].FrameAt(u, v, out frame);

                    dotProduct = Math.Round(Vector3d.Multiply(normal, Vector3d.ZAxis), 2);

                    if (dotProduct > angleThreshold && dotProduct <= 1)
                    {
                        this.upfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        this.upfacingFaceAreas.Add(area_properties.Area);
                        this.upfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.upfacingFacesCenters.Add(center);
                        this.upfacingFacesNormals.Add(normal);
                    }

                    else if (dotProduct < -angleThreshold && dotProduct >= -1)
                    {
                        this.downfacingFaceElevations.Add(Math.Round(center.Z, 2));
                        this.downfacingFaceAreas.Add(area_properties.Area);
                        this.downfacingFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.downfacingFacesCenters.Add(center);
                        this.downfacingFacesNormals.Add(normal);
                    }

                    else
                    {
                        this.sideAndEndFaces.Add(brep.Faces[i].DuplicateFace(false));
                        this.sideAndEndFaceAreas.Add(area_properties.Area);
                    }
                }
            }

            List<double> tempDownfacingFaceElevations = this.downfacingFaceElevations;
            List<double> tempDownfacingFaceAreas = this.downfacingFaceAreas;

            for (int i = 0; i < this.upfacingFaceElevations.Count; i++)
            {
                ray = new Ray3d(this.upfacingFacesCenters[i], this.upfacingFacesNormals[i]);

                bool isEndFace = false;

                List<Brep> tempUpfacingFaces = new List<Brep>(this.upfacingFaces);

                while (tempUpfacingFaces.Count > 0 && !isEndFace)
                {
                    mesh = Mesh.CreateFromBrep(tempUpfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

                    if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        isEndFace = true;
                    }

                    tempUpfacingFaces.Remove(tempUpfacingFaces[0]);
                }

                if (isEndFace)
                {
                    this.endFaceAreas.Add(this.upfacingFaceAreas[i]);
                }
                else
                {
                    topArea += this.upfacingFaceAreas[i];

                    this.topFaces.Add(this.upfacingFaces[i]);
                }
            }

            for (int i = 0; i < this.downfacingFaceElevations.Count; i++)
            {
                ray = new Ray3d(this.downfacingFacesCenters[i], this.downfacingFacesNormals[i]);

                bool isEndFace = false;

                List<Brep> tempDownfacingFaces = new List<Brep>(this.downfacingFaces);

                while (tempDownfacingFaces.Count > 0 && !isEndFace)
                {
                    mesh = Mesh.CreateFromBrep(tempDownfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

                    if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
                    {
                        isEndFace = true;
                    }

                    tempDownfacingFaces.Remove(tempDownfacingFaces[0]);
                }

                if (isEndFace)
                {
                    this.endFaceAreas.Add(this.downfacingFaceAreas[i]);
                }
                else
                {
                    bottomArea += this.downfacingFaceAreas[i];

                    this.bottomFaces.Add(this.downfacingFaces[i]);
                }
            }

            result.Add("Top Area", topArea);
            result.Add("Bottom Area", bottomArea);

            return result;

            //for (int i = 0; i < this.upfacingFaceElevations.Count; i++)
            //{
            //    ray = new Ray3d(this.upfacingFacesCenters[i], this.upfacingFacesNormals[i]);

            //    bool isEndFace = false;

            //    List<Brep> tempUpfacingFaces = new List<Brep>(this.upfacingFaces);

            //    while (tempUpfacingFaces.Count > 0 && !isEndFace)
            //    {
            //        mesh = Mesh.CreateFromBrep(tempUpfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

            //        if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
            //        {
            //            isEndFace = true;
            //        }

            //        tempUpfacingFaces.Remove(tempUpfacingFaces[0]);
            //    }

            //    if (isEndFace)
            //    {
            //        this.endFaceAreas.Add(this.upfacingFaceAreas[i]);
            //    }
            //    else
            //    {
            //        topArea += this.upfacingFaceAreas[i];

            //        this.topFaces.Add(this.upfacingFaces[i]);
            //    }
            //}

            //for (int i = 0; i < this.downfacingFaceElevations.Count; i++)
            //{
            //    ray = new Ray3d(this.downfacingFacesCenters[i], this.downfacingFacesNormals[i]);

            //    bool isEndFace = false;

            //    List<Brep> tempDownfacingFaces = new List<Brep>(this.downfacingFaces);

            //    while (tempDownfacingFaces.Count > 0 && !isEndFace)
            //    {
            //        mesh = Mesh.CreateFromBrep(tempDownfacingFaces[0], Rhino.Geometry.MeshingParameters.FastRenderMesh)[0];

            //        if (Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray) > RunQTO.doc.ModelAbsoluteTolerance)
            //        {
            //            isEndFace = true;
            //        }

            //        tempDownfacingFaces.Remove(tempDownfacingFaces[0]);
            //    }

            //    if (isEndFace)
            //    {
            //        this.endFaceAreas.Add(this.downfacingFaceAreas[i]);
            //    }
            //    else
            //    {
            //        bottomArea += this.downfacingFaceAreas[i];

            //        this.bottomFaces.Add(this.downfacingFaces[i]);
            //    }
            //}
        }

        void SidesAndEndAndOpeingArea()
        {
            Plane projectPlane = new Plane(new Point3d(0, 0, this.upfacingFaceElevations.Max()), Vector3d.ZAxis);
            List<Curve> boundaries = new List<Curve>();

            Curve mergedBoundary;
            Polyline mergedBoundaryPolyline = new Polyline();
            Curve joinedProjectedCenterLine;

            List<Point3d> corners;
            List<Point3d> tempPoints;

            List<Point3d> centers = new List<Point3d>();

            List<Curve> centerLines = new List<Curve>();

            double extrusionHeight = 9999;

            Vector3d normal;
            double u, v;
            Point3d center;

            Plane frame;

            double dotProduct, curveParameter;
            Vector3d curveTangent;

            Mesh meshedTopFaces = new Mesh();
            MeshingParameters mp = new MeshingParameters();

            Brep[] joinedSideFaces;

            Curve[] tempMergedBoundaries;

            List<double> sideFaceBoundingBoxAreas = new List<double>();

            if (this.topFaces.Count > 1)
            {
                for (int i = 0; i < this.topFaces.Count; i++)
                {
                    Curve curveBoundary = Curve.ProjectToPlane(Curve.JoinCurves(this.topFaces[i].Edges)[0], projectPlane);
                    boundaries.Add(curveBoundary);
                }

                tempMergedBoundaries = Curve.CreateBooleanUnion(boundaries, RunQTO.doc.ModelAbsoluteTolerance);
            }
            else
            {
                Curve[] curveBoundaries = Curve.JoinCurves(this.topFaces[0].Edges);

                if (curveBoundaries.Length > 1)
                {
                    for (int i = 0; i < curveBoundaries.Length; i++)
                    {
                        Curve curveBoundary = Curve.ProjectToPlane(curveBoundaries[i], projectPlane);
                        boundaries.Add(curveBoundary);
                    }
                }
                else
                {
                    Curve curveBoundary = Curve.ProjectToPlane(curveBoundaries[0], projectPlane);
                    boundaries.Add(curveBoundary);
                }

                tempMergedBoundaries = boundaries.ToArray();
            }

            if (tempMergedBoundaries.Length > 1)
            {
                Point3d pointOnCurve1, pointOnCurve2;

                tempMergedBoundaries[0].ClosestPoints(tempMergedBoundaries[1], out pointOnCurve1, out pointOnCurve2);

                double minDistance = pointOnCurve1.DistanceTo(pointOnCurve2) / 2;

                if (tempMergedBoundaries[0].GetLength() > tempMergedBoundaries[1].GetLength())
                {
                    tempMergedBoundaries[0] = tempMergedBoundaries[0].Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);

                    Curve curveOffset1 = tempMergedBoundaries[0].Offset(Plane.WorldXY, minDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                    Curve curveOffset2 = tempMergedBoundaries[0].Offset(Plane.WorldXY, -minDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];

                    if (curveOffset1.GetLength() > curveOffset2.GetLength())
                    {
                        centerLines.Add(curveOffset2);
                    }
                    else
                    {
                        centerLines.Add(curveOffset1);
                    }
                }
                else
                {
                    tempMergedBoundaries[1] = tempMergedBoundaries[1].Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);

                    Curve curveOffset1 = tempMergedBoundaries[1].Offset(Plane.WorldXY, minDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                    Curve curveOffset2 = tempMergedBoundaries[1].Offset(Plane.WorldXY, -minDistance, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];

                    if (curveOffset1.GetLength() > curveOffset2.GetLength())
                    {
                        centerLines.Add(curveOffset2);
                    }
                    else
                    {
                        centerLines.Add(curveOffset1);
                    }
                }
            }

            else
            {
                mergedBoundary = tempMergedBoundaries[0].
                    Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);

                double t0 = mergedBoundary.Domain.Min;
                double t1 = mergedBoundary.Domain.Max;
                double t;

                corners = new List<Point3d>();

                do
                {
                    if (!mergedBoundary.GetNextDiscontinuity(Continuity.G1_locus_continuous, t0, t1, out t)) { break; }

                    corners.Add(mergedBoundary.PointAt(t));

                    t0 = t;
                } while (true);

                for (int i = 0; i < corners.Count; i++)
                {
                    tempPoints = new List<Point3d>(corners);

                    tempPoints.Remove(tempPoints[i]);

                    Point3d closest = Rhino.Collections.Point3dList.ClosestPointInList(tempPoints, corners[i]);

                    centers.Add(Point3d.Divide(Point3d.Add(closest, corners[i]), 2));
                }

                centers = Point3d.SortAndCullPointList(centers, RunQTO.doc.ModelAbsoluteTolerance).ToList();

                if (centers.Count == 2)
                {
                    centerLines.Add(NurbsCurve.CreateFromLine(new Line(centers[0], centers[1])));
                }

                else
                {
                    for (int i = 0; i < centers.Count; i++)
                    {
                        tempPoints = new List<Point3d>(centers);
                        tempPoints.Remove(tempPoints[i]);

                        for (int j = 0; j < tempPoints.Count; j++)
                        {
                            Curve centerLine = NurbsCurve.CreateFromLine(new Line(centers[i], tempPoints[j]));

                            var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(mergedBoundary, centerLine, 0.01, 0.01);

                            if (events.Count < 2)
                            {
                                if (centerLines.Count > 0)
                                {
                                    bool dup = false;

                                    for (int k = 0; k < centerLines.Count; k++)
                                    {
                                        if (GeometryBase.GeometryEquals(centerLines[k], centerLine))
                                        {
                                            dup = true;
                                            break;
                                        }
                                    }

                                    if (!dup)
                                    {
                                        centerLines.Add(centerLine);
                                    }
                                }
                                else
                                {
                                    centerLines.Add(centerLine);
                                }
                            }
                        }
                    }
                }
            }

            joinedProjectedCenterLine = Curve.JoinCurves(centerLines)[0];

            Brep centerLineExtrusion = Extrusion.Create(joinedProjectedCenterLine, extrusionHeight, false).ToBrep();

            centerLineExtrusion.Join(Extrusion.Create(joinedProjectedCenterLine, -extrusionHeight, false).ToBrep(), 0.01, true);

            centerLineExtrusion.MergeCoplanarFaces(0.01);

            Curve[] intersectionCurves;
            Point3d[] intersectionPoints;

            //Calculate Length
            foreach (Brep topFace in this.topFaces)
            {
                Rhino.Geometry.Intersect.Intersection.BrepBrep(topFace, centerLineExtrusion, 0.01, out intersectionCurves, out intersectionPoints);

                this.length += Math.Round(Curve.JoinCurves(intersectionCurves)[0].GetLength(), 2);
            }

            //Side and Edges Calculation
            for (int i = 0; i < this.sideAndEndFaces.Count; i++)
            {
                var area_properties = AreaMassProperties.Compute(this.sideAndEndFaces[i]);

                center = area_properties.Centroid;

                joinedProjectedCenterLine.ClosestPoint(center, out curveParameter);

                curveTangent = joinedProjectedCenterLine.TangentAt(curveParameter);

                if (this.sideAndEndFaces[i].Faces[0].ClosestPoint(center, out u, out v))
                {
                    normal = this.sideAndEndFaces[i].Faces[0].NormalAt(u, v);

                    normal.Unitize();

                    this.sideAndEndFaces[i].Faces[0].FrameAt(u, v, out frame);

                    dotProduct = Math.Round(Vector3d.Multiply(normal, curveTangent), 2);

                    if (dotProduct > -0.1 && dotProduct < 0.1)
                    {
                        this.sideFaces.Add(this.sideAndEndFaces[i]);
                        this.sideFaceAreas.Add(this.sideAndEndFaceAreas[i]);
                        sideFaceBoundingBoxAreas.Add(this.sideAndEndFaces[i].GetBoundingBox(frame).Area / 2);
                    }

                    else
                    {
                        this.endFaces.Add(this.sideAndEndFaces[i]);
                        this.endFaceAreas.Add(this.sideAndEndFaceAreas[i]);
                    }
                }
            }

            //Total End Area
            this.endArea = Math.Round(this.endFaceAreas.Sum(), 2);

            joinedSideFaces = Brep.JoinBreps(this.sideFaces, RunQTO.doc.ModelAbsoluteTolerance);

            this.sideArea_1 = Math.Round(joinedSideFaces[0].GetArea(), 2);
            this.sideArea_2 = Math.Round(joinedSideFaces[1].GetArea(), 2);

            double noHoleSideArea_1 = Math.Round(joinedSideFaces[0].RemoveHoles(RunQTO.doc.ModelAbsoluteTolerance).GetArea(), 2);
            double noHoleSideArea_2 = Math.Round(joinedSideFaces[1].RemoveHoles(RunQTO.doc.ModelAbsoluteTolerance).GetArea(), 2);

            this.openingArea = Math.Round(((noHoleSideArea_1 + noHoleSideArea_2) - (this.sideArea_1 + this.sideArea_2)) / 2, 2);
        }

        double GrossVolume()
        {
            double result = 0;

            List<Brep> brepFaces = new List<Brep>();

            Brep grossVolumeGeometry;

            for (int i = 0; i < this.sideFaces.Count; i++)
            {
                brepFaces.Add(this.sideFaces[i].RemoveHoles(RunQTO.doc.ModelAbsoluteTolerance));
            }

            brepFaces.AddRange(this.topFaces);
            brepFaces.AddRange(this.bottomFaces);
            brepFaces.AddRange(this.endFaces);

            grossVolumeGeometry = Brep.JoinBreps(brepFaces, RunQTO.doc.ModelAbsoluteTolerance)[0];

            var mass_properties = VolumeMassProperties.Compute(grossVolumeGeometry);

            result = Math.Round(mass_properties.Volume * 0.037037, 2);

            return result;
        }
    }
}
