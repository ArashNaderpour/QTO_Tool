using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTO_Tool
{
    class BeamTemplate
    {
        public string nameAbb { get; set; }
        public string id { get; set; }

        public string layerName { get; set; }

        public Dictionary<string, string> parsedLayerName = new Dictionary<string, string>();

        public string floor { get; set; }

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

        public string type = "BeamTemplate";

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

        public BeamTemplate(RhinoObject rhobj, string _layerName, System.Drawing.Color layerColor, double angleThreshold, Dictionary<double, string> floorElevations)
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
            this.netVolume = Math.Round(mass_properties.Volume * RunQTO.volumeConversionFactor, 2);

            Dictionary<string, double> topAndBottomArea = this.TopAndBottomArea(this.geometry, angleThreshold);

            this.topArea = Math.Round(topAndBottomArea["Top Area"], 2);

            this.bottomArea = Math.Round(topAndBottomArea["Bottom Area"], 2);

            if (floorElevations.Count > 0)
            {
                this.floor = Methods.FindFloor(floorElevations, this.downfacingFaceElevations.Min());
            }
            else
            {
                this.floor = "-";
            }

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

            Rhino.Geometry.Plane frame;

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
        }

        void SidesAndEndAndOpeingArea()
        {
            Rhino.Geometry.Plane projectPlane = new Rhino.Geometry.Plane(new Point3d(0, 0, this.upfacingFaceElevations.Max()), Vector3d.ZAxis);
            List<Curve> boundaries = new List<Curve>();

            Polyline mergedBoundaryPolyline = new Polyline();
            Curve joinedProjectedCenterLine = null;

            List<Point3d> corners;
            List<Point3d> tempPoints;

            double wallThickness = new double();

            List<Point3d> centers = new List<Point3d>();

            List<Curve> centerLines = new List<Curve>();

            double extensionValue = 9999;

            Rhino.Geometry.Intersect.CurveIntersections intersectionEvents;

            Vector3d normal;
            double u, v;
            Point3d center;

            Rhino.Geometry.Plane frame;

            double dotProduct;

            double curveParameter;

            Vector3d curveTangent;

            Mesh meshedTopFaces = new Mesh();
            MeshingParameters mp = new MeshingParameters();

            Brep[] joinedSideFaces;

            Curve[] tempMergedBoundaries;

            List<double> sideFaceBoundingBoxAreas = new List<double>();

            if (this.topFaces.Count > 1)
            {
                //for (int i = 0; i < this.topFaces.Count; i++)
                //{
                //    Curve curveBoundary = Curve.ProjectToPlane(Curve.JoinCurves(this.topFaces[i].Edges)[0], projectPlane);
                //    boundaries.Add(curveBoundary);
                //}

                //tempMergedBoundaries = Curve.CreateBooleanUnion(boundaries, RunQTO.doc.ModelAbsoluteTolerance);
                List<Brep> projectedBreps = new List<Brep>();
                Transform projectionTransform = Transform.ProjectAlong(Rhino.Geometry.Plane.WorldXY, Rhino.Geometry.Vector3d.ZAxis);
                for (int i = 0; i < this.topFaces.Count; i++)
                {
                    Brep projectedBrep = this.topFaces[i].DuplicateBrep();

                    projectedBrep.Transform(projectionTransform);

                    projectedBreps.Add(projectedBrep);
                }
                Brep joinedBrep = Brep.JoinBreps(projectedBreps, RunQTO.doc.ModelAbsoluteTolerance)[0];

                joinedBrep.MergeCoplanarFaces(RunQTO.doc.ModelAbsoluteTolerance);

                tempMergedBoundaries = Curve.JoinCurves(joinedBrep.Edges);
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

            // <-----------If it's a closed boundary----------->
            if (tempMergedBoundaries.Length > 1)
            {
                curveParameter = tempMergedBoundaries[0].Domain.Mid * 0.66;

                Point3d pointOnCurve0 = tempMergedBoundaries[0].PointAt(curveParameter);

                tempMergedBoundaries[1].ClosestPoint(pointOnCurve0, out curveParameter);

                Point3d pointOnCurve1 = tempMergedBoundaries[1].PointAt(curveParameter);

                wallThickness = Math.Round(pointOnCurve0.DistanceTo(pointOnCurve1), 2);

                Vector3d offsetDirection = pointOnCurve1 - pointOnCurve0;

                offsetDirection /= 2;

                Point3d offsetDirectionPoint = pointOnCurve0 + offsetDirection;

                joinedProjectedCenterLine = tempMergedBoundaries[1].Offset(offsetDirectionPoint,
                  new Vector3d(0, 0, 1), wallThickness / 2, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            }

            else
            {
                Curve mergedBoundary = tempMergedBoundaries[0];

                if (mergedBoundary.Degree == 1)
                {
                    mergedBoundary = mergedBoundary.Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);
                }

                Curve[] mergedBoundarySegments = mergedBoundary.DuplicateSegments();

                if (mergedBoundarySegments.Length == 4)
                {
                    List<Tuple<Curve, double>> mergedBoundarySegmentsSorted = mergedBoundarySegments
                        .Select(segment => new Tuple<Curve, double>(segment, segment.GetLength()))
                        .OrderByDescending(cl => cl.Item2)
                        .ToList();

                    curveParameter = mergedBoundarySegmentsSorted[0].Item1.Domain.Mid;

                    Point3d pointOnCurve0 = mergedBoundarySegmentsSorted[0].Item1.PointAt(curveParameter);

                    mergedBoundarySegmentsSorted[1].Item1.ClosestPoint(pointOnCurve0, out curveParameter);

                    Point3d pointOnCurve1 = mergedBoundarySegmentsSorted[1].Item1.PointAt(curveParameter);

                    wallThickness = Math.Round(pointOnCurve0.DistanceTo(pointOnCurve1), 2);

                    Vector3d offsetDirection = pointOnCurve1 - pointOnCurve0;

                    offsetDirection /= 2;

                    Point3d offsetDirectionPoint = pointOnCurve0 + offsetDirection;

                    joinedProjectedCenterLine = mergedBoundarySegmentsSorted[1].Item1.Offset(offsetDirectionPoint,
                      new Vector3d(0, 0, 1), wallThickness / 2, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                }
                else
                {
                    List<double> sampleWallThicknesses = new List<double>();

                    for (int i = 0; i < 5; i++)
                    {
                        double randomCurveParameter = Methods.random.NextDouble() * (mergedBoundary.Domain.Max - mergedBoundary.Domain.Min) + mergedBoundary.Domain.Min;

                        Point3d samplePoint = mergedBoundary.PointAt(randomCurveParameter);
                        Vector3d perpendicularDirection = Vector3d.CrossProduct(mergedBoundary.TangentAt(randomCurveParameter), Vector3d.ZAxis);

                        Curve perpendicularLine1 = new Line(samplePoint, perpendicularDirection * extensionValue).ToNurbsCurve();
                        Curve perpendicularLine2 = new Line(samplePoint, perpendicularDirection * -extensionValue).ToNurbsCurve();

                        var intersectionEvents1 = Rhino.Geometry.Intersect.Intersection.CurveCurve(mergedBoundary, perpendicularLine1.ToNurbsCurve(),
                            RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAbsoluteTolerance);
                        var intersectionEvents2 = Rhino.Geometry.Intersect.Intersection.CurveCurve(mergedBoundary, perpendicularLine2.ToNurbsCurve(),
                            RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAbsoluteTolerance);

                        Point3d closestIntersectionPoint1 = new Point3d();
                        Point3d closestIntersectionPoint2 = new Point3d();
                        Point3d midIntersectionPoint1 = new Point3d();
                        Point3d midIntersectionPoint2 = new Point3d();
                        double closestDistance = double.MaxValue;

                        for (int j = 0; j < intersectionEvents1.Count; j++)
                        {
                            double distance = intersectionEvents1[j].PointA.DistanceTo(samplePoint);
                            if (distance < closestDistance && distance > RunQTO.doc.ModelAbsoluteTolerance)
                            {
                                closestIntersectionPoint1 = intersectionEvents1[j].PointA;
                                midIntersectionPoint1 = (closestIntersectionPoint1 + samplePoint) / 2;
                                closestDistance = distance;
                            }
                        }

                        for (int j = 0; j < intersectionEvents2.Count; j++)
                        {
                            double distance = intersectionEvents2[j].PointA.DistanceTo(samplePoint);
                            if (distance < closestDistance && distance > RunQTO.doc.ModelAbsoluteTolerance)
                            {
                                closestIntersectionPoint2 = intersectionEvents2[j].PointA;
                                midIntersectionPoint2 = (closestIntersectionPoint2 + samplePoint) / 2;
                                closestDistance = distance;
                            }
                        }

                        double sampleWallThickness;

                        if (mergedBoundary.Contains(midIntersectionPoint1, Rhino.Geometry.Plane.WorldXY, RunQTO.doc.ModelAbsoluteTolerance) == PointContainment.Inside)
                        {
                            sampleWallThickness = Math.Round(samplePoint.DistanceTo(closestIntersectionPoint1), 2);
                        }
                        else
                        {
                            sampleWallThickness = Math.Round(samplePoint.DistanceTo(closestIntersectionPoint2), 2);
                        }

                        sampleWallThicknesses.Add(sampleWallThickness);
                    }

                    wallThickness = sampleWallThicknesses
                        .GroupBy(x => x)
                        .OrderByDescending(g => g.Count())
                        .First()
                        .Key;

                    Curve curveOffset1 = mergedBoundary.Offset(Rhino.Geometry.Plane.WorldXY, wallThickness * 0.45, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
                    Curve curveOffset2 = mergedBoundary.Offset(Rhino.Geometry.Plane.WorldXY, wallThickness * -0.45, RunQTO.doc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];

                    Curve mergedBoundaryOffset;

                    if (Rhino.Geometry.AreaMassProperties.Compute(curveOffset1, RunQTO.doc.ModelAbsoluteTolerance).Area < Rhino.Geometry.AreaMassProperties.Compute(curveOffset2, RunQTO.doc.ModelAbsoluteTolerance).Area)
                    {
                        mergedBoundaryOffset = curveOffset1.Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);
                    }
                    else
                    {
                        mergedBoundaryOffset = curveOffset2.Simplify(CurveSimplifyOptions.All, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAngleToleranceRadians);
                    }

                    double t0 = mergedBoundaryOffset.Domain.Min;
                    double t1 = mergedBoundaryOffset.Domain.Max;
                    double t;

                    corners = new List<Point3d>();

                    do
                    {
                        if (!mergedBoundaryOffset.GetNextDiscontinuity(Continuity.G1_locus_continuous, t0, t1, out t)) { break; }

                        corners.Add(mergedBoundaryOffset.PointAt(t));

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

                                Point3d centerLineMidPoint = (centers[i] + tempPoints[j]) / 2;

                                double closestPointParameter;

                                mergedBoundary.ClosestPoint(centerLineMidPoint, out closestPointParameter);

                                double centerLineMidPointDistanceToCurve = centerLineMidPoint.DistanceTo(mergedBoundary.PointAt(closestPointParameter));

                                intersectionEvents = Rhino.Geometry.Intersect.Intersection.CurveCurve(mergedBoundaryOffset, centerLine, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAbsoluteTolerance);

                                if (intersectionEvents.Count < 2 && Math.Abs(Math.Round(centerLineMidPointDistanceToCurve, 2) - (Math.Round(wallThickness / 2, 2))) <= 0.1)
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

                    joinedProjectedCenterLine = Curve.JoinCurves(centerLines)[0];

                    joinedProjectedCenterLine = joinedProjectedCenterLine.Extend(CurveEnd.Both, wallThickness, CurveExtensionStyle.Line);

                    intersectionEvents = Rhino.Geometry.Intersect.Intersection.CurveCurve(joinedProjectedCenterLine, mergedBoundary, RunQTO.doc.ModelAbsoluteTolerance, RunQTO.doc.ModelAbsoluteTolerance);

                    joinedProjectedCenterLine.Trim(intersectionEvents[0].ParameterA, intersectionEvents[1].ParameterA);
                }
            }

            Brep centerLineExtrusion = Extrusion.Create(joinedProjectedCenterLine, extensionValue, false).ToBrep();

            centerLineExtrusion.Join(Extrusion.Create(joinedProjectedCenterLine, -extensionValue, false).ToBrep(), RunQTO.doc.ModelAbsoluteTolerance, true);

            centerLineExtrusion.MergeCoplanarFaces(RunQTO.doc.ModelAbsoluteTolerance);

            Curve[] intersectionCurves;
            Point3d[] intersectionPoints;

            //Calculate Length
            foreach (Brep topFace in this.topFaces)
            {
                Rhino.Geometry.Intersect.Intersection.BrepBrep(topFace, centerLineExtrusion, RunQTO.doc.ModelAbsoluteTolerance, out intersectionCurves, out intersectionPoints);
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

            result = Math.Round(mass_properties.Volume * RunQTO.volumeConversionFactor, 2);

            return result;
        }
    }
}