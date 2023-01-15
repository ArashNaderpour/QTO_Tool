using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.StructuralElementsDomain;
using Xbim.Ifc4.TopologyResource;
using Xbim.IO;

namespace QTO_Tool
{
    class IFCMethods
    {
        public static IfcStore CreateandInitIFCModel(string projectName)
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "Digital Charcoal",
                ApplicationFullName = "QTO_TOOL",
                ApplicationIdentifier = "QTO",
                ApplicationVersion = "1.0",
                EditorsFamilyName = "Naderpour",
                EditorsGivenName = "Arash",
                EditorsOrganisationName = "Digital Charcoal"
            };

            var model = IfcStore.Create(editor, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            using (ITransaction transaction = model.BeginTransaction("Initialise Model"))
            {
                IfcProject project = model.Instances.New<IfcProject>();
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = projectName;
                transaction.Commit();
            }

            return model;
        }

        public static IfcBuilding CreateBuilding(IfcStore model, string name)
        {
            using (var txn = model.BeginTransaction("Create Building"))
            {
                var building = model.Instances.New<IfcBuilding>();
                building.Name = name;

                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                IfcLocalPlacement localPlacement = model.Instances.New<IfcLocalPlacement>();
                var placement = model.Instances.New<IfcAxis2Placement3D>();
                localPlacement.RelativePlacement = placement;
                placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
                //get the project there should only be one and it should exist
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                project?.AddBuilding(building);
                txn.Commit();

                return building;
            }
        }

        public static void CreateAndAddIFCElement(IfcStore model, IfcBuilding building, object templates)
        {
            if (templates.GetType() == typeof(QTO_Tool.AllWalls))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllWalls)templates).allTemplates)
                {
                    foreach (WallTemplate wallTemplate in entry.Value)
                    {
                        Mesh meshGeometry = new Mesh();

                        meshGeometry.Append(Mesh.CreateFromBrep(wallTemplate.geometry, MeshingParameters.QualityRenderMesh));
                        Plane insertPlane = Plane.WorldXY;

                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, meshGeometry, wallTemplate.type, wallTemplate.nameAbb, insertPlane, wallTemplate.layerName);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllBeams))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllBeams)templates).allTemplates)
                {
                    foreach (BeamTemplate beamTemplate in entry.Value)
                    {
                        Mesh meshGeometry = new Mesh();

                        meshGeometry.Append(Mesh.CreateFromBrep(beamTemplate.geometry, MeshingParameters.QualityRenderMesh));
                        Plane insertPlane = Plane.WorldXY;

                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, meshGeometry, beamTemplate.type, beamTemplate.nameAbb, insertPlane, beamTemplate.layerName);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllColumns))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllColumns)templates).allTemplates)
                {
                    foreach (ColumnTemplate columnTemplate in entry.Value)
                    {
                        Mesh meshGeometry = new Mesh();

                        meshGeometry.Append(Mesh.CreateFromBrep(columnTemplate.geometry, MeshingParameters.QualityRenderMesh));
                        Plane insertPlane = Plane.WorldXY;

                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, meshGeometry, columnTemplate.type, columnTemplate.nameAbb, insertPlane, columnTemplate.layerName);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }
        }

        public static List<IfcBuildingElement> ToBuildingElementIfc(IfcStore model, Mesh meshGeometry, string elementType, string nameAbb, Plane insertPlane, string layerName)
        {
            MeshFaceList faces = meshGeometry.Faces;
            MeshVertexList vertices = meshGeometry.Vertices;

            List<IfcCartesianPoint> ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

            IfcFaceBasedSurfaceModel faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

            IfcShapeRepresentation shape = IFCMethods.CreateIfcShapeRepresentation(model, "Brep", layerName);
            shape.Items.Add(faceBasedSurfaceModel);
            IfcRelAssociatesMaterial ifcRelAssociatesMaterial = IFCMethods.CreateIfcRelAssociatesMaterial(model, "Concrete", "Undefined");

            List<IfcBuildingElement> buildingElements = IFCMethods.CreateBuildingElements(model, elementType, nameAbb, shape, insertPlane,
                ifcRelAssociatesMaterial);

            return buildingElements;
        }

        public static List<IfcBuildingElement> CreateBuildingElements(IfcStore model, string type, string nameAbb,
            IfcShapeRepresentation shape, Plane insertPlane, IfcRelAssociatesMaterial relAssociatesMaterial)
        {
            var buildingElements = new List<IfcBuildingElement>();

            if (type == "WallTemplate")
            {
                var ifcWall = IFCMethods.CreateWall(model, nameAbb, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(ifcWall);
                buildingElements.Add(ifcWall);
            }
            else if (type == "BeamTemplate")
            {
                var ifcBeam = IFCMethods.CreateBeam(model, nameAbb, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(ifcBeam);
                buildingElements.Add(ifcBeam);
            }
            else if (type == "ColumnTemplate")
            {
                var column = IFCMethods.CreateColumn(model, nameAbb, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(column);
                buildingElements.Add(column);
            }

            return buildingElements;
        }

        private static IfcWall CreateWall(IfcStore model, string nameAbb, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var wall = model.Instances.New<IfcWall>();
            wall.Name = nameAbb;

            wall.PredefinedType = IfcWallTypeEnum.STANDARD;

            IFCMethods.ApplyRepresentationAndPlacement(model, wall, shape, insertPlane);

            return wall;
        }

        private static IfcBeam CreateBeam(IfcStore model, string nameAbb, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var beam = model.Instances.New<IfcBeam>();
            beam.Name = nameAbb;

            beam.PredefinedType = IfcBeamTypeEnum.BEAM;

            IFCMethods.ApplyRepresentationAndPlacement(model, beam, shape, insertPlane);

            return beam;
        }

        private static IfcColumn CreateColumn(IfcStore model, string nameAbb, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var column = model.Instances.New<IfcColumn>();
            column.Name = nameAbb;

            column.PredefinedType = IfcColumnTypeEnum.COLUMN;

            IFCMethods.ApplyRepresentationAndPlacement(model, column, shape, insertPlane);

            return column;
        }

        public static List<IfcCartesianPoint> VerticesToIfcCartesianPoints(IfcStore model, MeshVertexList vertices)
        {
            List<IfcCartesianPoint> ifcCartesianPoints = new List<IfcCartesianPoint>();

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var vertex in vertices)
            {
                IfcCartesianPoint currentVertex = model.Instances.New<IfcCartesianPoint>();

                if (RunQTO.doc.GetUnitSystemName(true, true, true, true) == "mm")
                {
                    x = (double)vertex.X;
                    y = (double)vertex.Y;
                    z = (double)vertex.Z;
                }

                else if (RunQTO.doc.GetUnitSystemName(true, true, true, true) == "ft")
                {
                    x = (double)vertex.X * 304.8;
                    y = (double)vertex.Y * 304.8;
                    z = (double)vertex.Z * 304.8;
                }

                else if (RunQTO.doc.GetUnitSystemName(true, true, true, true) == "in")
                {
                    x = (double)vertex.X * 25.4;
                    y = (double)vertex.Y * 25.4;
                    z = (double)vertex.Z * 25.4;
                }

                else if (RunQTO.doc.GetUnitSystemName(true, true, true, true) == "m")
                {
                    x = (double)vertex.X * 1000;
                    y = (double)vertex.Y * 1000;
                    z = (double)vertex.Z * 1000;
                }

                currentVertex.SetXYZ(x, y, z);

                ifcCartesianPoints.Add(currentVertex);
            }

            return ifcCartesianPoints;
        }

        //public static List<IfcCartesianPoint> PointsToIfcCartesianPoints(IfcStore model, List<Point3d> points, bool closeShape)
        //{
        //    List<IfcCartesianPoint> ifcCartesianPoints = new List<IfcCartesianPoint>();

        //    foreach (var point in points)
        //    {
        //        IfcCartesianPoint currentVertex = model.Instances.New<IfcCartesianPoint>();
        //        currentVertex.SetXYZ(point.X, point.Y, point.Z);
        //        ifcCartesianPoints.Add(currentVertex);
        //    }

        //    if (closeShape)
        //    {
        //        IfcCartesianPoint currentVertex = model.Instances.New<IfcCartesianPoint>();
        //        currentVertex.SetXYZ(points[0].X, points[0].Y, points[0].Z);
        //        ifcCartesianPoints.Add(currentVertex);
        //    }

        //    return ifcCartesianPoints;
        //}

        public static IfcFaceBasedSurfaceModel CreateIfcFaceBasedSurfaceModel(IfcStore model, MeshFaceList faces, List<IfcCartesianPoint> ifcVertices)
        {
            IfcConnectedFaceSet faceSet = model.Instances.New<IfcConnectedFaceSet>();

            foreach (MeshFace meshFace in faces)
            {
                List<IfcCartesianPoint> points = new List<IfcCartesianPoint>
                {
                    ifcVertices[meshFace.A], ifcVertices[meshFace.B], ifcVertices[meshFace.C]
                };
                if (meshFace.C != meshFace.D)
                {
                    points.Add(ifcVertices[meshFace.D]);
                }

                var polyLoop = model.Instances.New<IfcPolyLoop>();
                polyLoop.Polygon.AddRange(points);
                var bound = model.Instances.New<IfcFaceOuterBound>();
                bound.Bound = polyLoop;
                var face = model.Instances.New<IfcFace>();
                face.Bounds.Add(bound);

                faceSet.CfsFaces.Add(face);
            }

            var faceBasedSurfaceModel = model.Instances.New<IfcFaceBasedSurfaceModel>();
            faceBasedSurfaceModel.FbsmFaces.Add(faceSet);

            return faceBasedSurfaceModel;
        }

        public static IfcShapeRepresentation CreateIfcShapeRepresentation(IfcStore model, string representationType, string layerName)
        {
            var shape = model.Instances.New<IfcShapeRepresentation>();
            var modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            shape.ContextOfItems = modelContext;
            shape.RepresentationType = representationType;
            shape.RepresentationIdentifier = representationType;

            // IfcPresentationLayerAssignment is required for CAD presentation in IfcWall or IfcWallStandardCase
            var ifcPresentationLayerAssignment = model.Instances.New<IfcPresentationLayerAssignment>();
            ifcPresentationLayerAssignment.Name = layerName;
            ifcPresentationLayerAssignment.AssignedItems.Add(shape);

            return shape;
        }

        public static IfcRelAssociatesMaterial CreateIfcRelAssociatesMaterial(IfcStore model, string name, string grade)
        {
            var material = model.Instances.New<IfcMaterial>();
            material.Category = name;
            material.Name = grade;
            IfcRelAssociatesMaterial ifcRelAssociatesMaterial = model.Instances.New<IfcRelAssociatesMaterial>();
            ifcRelAssociatesMaterial.RelatingMaterial = material;

            return ifcRelAssociatesMaterial;
        }

        private static void ApplyRepresentationAndPlacement(IfcStore model, IfcBuildingElement element, IfcShapeRepresentation shape, Plane insertPlane)
        {
            IfcProductDefinitionShape representation = model.Instances.New<IfcProductDefinitionShape>();
            representation.Representations.Add(shape);
            element.Representation = representation;

            IfcLocalPlacement localPlacement = IFCMethods.CreateLocalPlacement(model, insertPlane);
            element.ObjectPlacement = localPlacement;
        }

        private static IfcLocalPlacement CreateLocalPlacement(IfcStore model, Plane insertPlane)
        {
            var localPlacement = model.Instances.New<IfcLocalPlacement>();
            var ax3D = model.Instances.New<IfcAxis2Placement3D>();

            var location = model.Instances.New<IfcCartesianPoint>();
            location.SetXYZ(insertPlane.OriginX, insertPlane.OriginY, insertPlane.OriginZ);
            ax3D.Location = location;

            ax3D.RefDirection = model.Instances.New<IfcDirection>();
            ax3D.RefDirection.SetXYZ(insertPlane.XAxis.X, insertPlane.XAxis.Y, insertPlane.XAxis.Z);
            ax3D.Axis = model.Instances.New<IfcDirection>();
            ax3D.Axis.SetXYZ(insertPlane.ZAxis.X, insertPlane.ZAxis.Y, insertPlane.ZAxis.Z);
            localPlacement.RelativePlacement = ax3D;

            return localPlacement;
        }
    }
}
