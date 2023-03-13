using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
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
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
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
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, wallTemplate);

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
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, beamTemplate);

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
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, columnTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllFootings))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllFootings)templates).allTemplates)
                {
                    foreach (FootingTemplate footingTemplate in entry.Value)
                    {
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, footingTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllContinousFootings))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllContinousFootings)templates).allTemplates)
                {
                    foreach (ContinuousFootingTemplate continousFootingTemplate in entry.Value)
                    {
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, continousFootingTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllCurbs))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllCurbs)templates).allTemplates)
                {
                    foreach (CurbTemplate curbTemplate in entry.Value)
                    {
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, curbTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllSlabs))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllSlabs)templates).allTemplates)
                {
                    foreach (SlabTemplate slabTemplate in entry.Value)
                    {
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, slabTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }

            else if (templates.GetType() == typeof(QTO_Tool.AllStyrofoams))
            {
                foreach (KeyValuePair<string, List<object>> entry in ((AllStyrofoams)templates).allTemplates)
                {
                    foreach (StyrofoamTemplate styrofoamTemplate in entry.Value)
                    {
                        //begin a transaction
                        using (var txn = model.BeginTransaction("Add IFC Element"))
                        {
                            List<IfcBuildingElement> buildingElements = IFCMethods.ToBuildingElementIfc(model, styrofoamTemplate);

                            building.AddElement(buildingElements[0]);

                            txn.Commit();
                        }
                    }
                }
            }
        }

        public static List<IfcBuildingElement> ToBuildingElementIfc(IfcStore model, object template)
        {
            IfcRelAssociatesMaterial ifcRelAssociatesMaterial = IFCMethods.CreateIfcRelAssociatesMaterial(model, "Concrete", "Undefined");

            List<IfcBuildingElement> buildingElements = IFCMethods.CreateBuildingElements(model, template, ifcRelAssociatesMaterial);

            return buildingElements;
        }

        public static List<IfcBuildingElement> CreateBuildingElements(IfcStore model, object template, IfcRelAssociatesMaterial relAssociatesMaterial)
        {
            List<IfcBuildingElement> buildingElements = new List<IfcBuildingElement>();

            List<IfcCartesianPoint> ifcVertices = new List<IfcCartesianPoint>();

            Plane insertPlane = Plane.WorldXY;

            Mesh meshGeometry = new Mesh();

            MeshFaceList faces;

            MeshVertexList vertices;

            IfcShapeRepresentation shape;

            IfcFaceBasedSurfaceModel faceBasedSurfaceModel;

            if (template.GetType() == typeof(QTO_Tool.WallTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((WallTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((WallTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                IfcWall ifcWall = IFCMethods.CreateWall(model, (WallTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(ifcWall);
                buildingElements.Add(ifcWall);
            }
            else if (template.GetType() == typeof(QTO_Tool.BeamTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((BeamTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((BeamTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                IfcBeam ifcBeam = IFCMethods.CreateBeam(model, (BeamTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(ifcBeam);
                buildingElements.Add(ifcBeam);
            }
            else if (template.GetType() == typeof(QTO_Tool.ColumnTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((ColumnTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((ColumnTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                IfcColumn column = IFCMethods.CreateColumn(model, (ColumnTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(column);
                buildingElements.Add(column);
            }
            else if (template.GetType() == typeof(QTO_Tool.FootingTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((FootingTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((FootingTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                IfcFooting footing = IFCMethods.CreateFooting(model, (FootingTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(footing);
                buildingElements.Add(footing);
            }
            else if (template.GetType() == typeof(QTO_Tool.ContinuousFootingTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((ContinuousFootingTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((ContinuousFootingTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                IfcFooting continuousFooting = IFCMethods.CreateContinuousFooting(model, (ContinuousFootingTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(continuousFooting);
                buildingElements.Add(continuousFooting);
            }
            else if (template.GetType() == typeof(QTO_Tool.CurbTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((CurbTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((CurbTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                var curb = IFCMethods.CreateCurb(model, (CurbTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(curb);
                buildingElements.Add(curb);
            }
            else if (template.GetType() == typeof(QTO_Tool.SlabTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((SlabTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((SlabTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                var slab = IFCMethods.CreateSlab(model, (SlabTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(slab);
                buildingElements.Add(slab);
            }
            else if (template.GetType() == typeof(QTO_Tool.StyrofoamTemplate))
            {
                meshGeometry.Append(Mesh.CreateFromBrep(((StyrofoamTemplate)template).geometry, MeshingParameters.QualityRenderMesh));

                faces = meshGeometry.Faces;
                vertices = meshGeometry.Vertices;

                ifcVertices = IFCMethods.VerticesToIfcCartesianPoints(model, vertices);

                faceBasedSurfaceModel = IFCMethods.CreateIfcFaceBasedSurfaceModel(model, faces, ifcVertices);

                shape = IFCMethods.CreateIfcShapeRepresentation(model, "Body", ((StyrofoamTemplate)template).layerName);
                shape.Items.Add(faceBasedSurfaceModel);

                var styrofoam = IFCMethods.CreateStyrofoam(model, (StyrofoamTemplate)template, shape, insertPlane);
                relAssociatesMaterial.RelatedObjects.Add(styrofoam);
                buildingElements.Add(styrofoam);
            }

            return buildingElements;
        }

        private static IfcWall CreateWall(IfcStore model, WallTemplate wallTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var wall = model.Instances.New<IfcWall>();
            wall.Name = wallTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(wall);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(wall.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.grossVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NET VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.netVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "OPENING AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.openingArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "TOP AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.topArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "END AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.endArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-1";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.sideArea_1, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-2";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.sideArea_2, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "LENGTH";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(wallTemplate.length, 2).ToString());
                    })
                    });
                });
            });

            wall.PredefinedType = IfcWallTypeEnum.STANDARD;

            IFCMethods.ApplyRepresentationAndPlacement(model, wall, shape, insertPlane);

            return wall;
        }

        private static IfcBeam CreateBeam(IfcStore model, BeamTemplate beamTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var beam = model.Instances.New<IfcBeam>();
            beam.Name = beamTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(beam);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(beam.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.grossVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NET VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.netVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "OPENING AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.openingArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "BOTTOM AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.bottomArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "END AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.endArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-1";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.sideArea_1, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-2";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.sideArea_2, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "LENGTH";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(beamTemplate.length, 2).ToString());
                    })
                    });
                });
            });

            beam.PredefinedType = IfcBeamTypeEnum.BEAM;

            IFCMethods.ApplyRepresentationAndPlacement(model, beam, shape, insertPlane);

            return beam;
        }

        private static IfcColumn CreateColumn(IfcStore model, ColumnTemplate columnTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var column = model.Instances.New<IfcColumn>();
            column.Name = columnTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(column);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(column.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(columnTemplate.volume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(columnTemplate.sideArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "HEIGHT";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(columnTemplate.height, 2).ToString());
                    })
                    });
                });
            });

            column.PredefinedType = IfcColumnTypeEnum.COLUMN;

            IFCMethods.ApplyRepresentationAndPlacement(model, column, shape, insertPlane);

            return column;
        }

        private static IfcWall CreateCurb(IfcStore model, CurbTemplate curbTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var curb = model.Instances.New<IfcWall>();
            curb.Name = curbTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(curb);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(curb.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.grossVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NET VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.netVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "OPENING AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.openingArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "TOP AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.topArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "END AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.endArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-1";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.sideArea_1, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-2";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.sideArea_2, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "LENGTH";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(curbTemplate.length, 2).ToString());
                    })
                    });
                });
            });

            curb.PredefinedType = IfcWallTypeEnum.STANDARD;

            IFCMethods.ApplyRepresentationAndPlacement(model, curb, shape, insertPlane);

            return curb;
        }

        private static IfcFooting CreateFooting(IfcStore model, FootingTemplate footingTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var footing = model.Instances.New<IfcFooting>();
            footing.Name = footingTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(footing);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(footing.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(footingTemplate.volume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "TOP AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(footingTemplate.topArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "BOTTOM AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(footingTemplate.bottomArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(footingTemplate.sideArea, 2).ToString());
                    })
                    });
                });
            });

            footing.PredefinedType = IfcFootingTypeEnum.PAD_FOOTING;

            IFCMethods.ApplyRepresentationAndPlacement(model, footing, shape, insertPlane);

            return footing;
        }

        private static IfcFooting CreateContinuousFooting(IfcStore model, ContinuousFootingTemplate continuousFootingTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var continuousFooting = model.Instances.New<IfcFooting>();
            continuousFooting.Name = continuousFootingTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(continuousFooting);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(continuousFooting.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.grossVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NET VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.netVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "OPENING AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.openingArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "BOTTOM AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.bottomArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "END AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.endArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-1";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.sideArea_1, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "SIDE-2";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.sideArea_2, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "LENGTH";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(continuousFootingTemplate.length, 2).ToString());
                    })
                    });
                });
            });

            continuousFooting.PredefinedType = IfcFootingTypeEnum.STRIP_FOOTING;

            IFCMethods.ApplyRepresentationAndPlacement(model, continuousFooting, shape, insertPlane);

            return continuousFooting;
        }

        private static IfcSlab CreateSlab(IfcStore model, SlabTemplate slabTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var slab = model.Instances.New<IfcSlab>();
            slab.Name = slabTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(slab);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(slab.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.grossVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NET VOLUME";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.netVolume, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "TOP AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.topArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "BOTTOM AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.bottomArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "EDGE AREA";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.edgeArea, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "PERIMETER";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.perimeter, 2).ToString());
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "OPENING PERIMETER";
                        p.NominalValue = new IfcNumericMeasure(Math.Round(slabTemplate.openingPerimeter, 2).ToString());
                    })
                    });
                });
            });

            slab.PredefinedType = IfcSlabTypeEnum.FLOOR;

            IFCMethods.ApplyRepresentationAndPlacement(model, slab, shape, insertPlane);

            return slab;
        }

        private static IfcSlab CreateStyrofoam(IfcStore model, StyrofoamTemplate styrofoamTemplate, IfcShapeRepresentation shape, Plane insertPlane)
        {
            var styrofoam = model.Instances.New<IfcSlab>();
            styrofoam.Name = styrofoamTemplate.nameAbb;

            //set a few basic properties
            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(styrofoam);

                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "QTO Properties";

                    pset.HasProperties.AddRange(new[] {
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "NAME ABB.";
                        p.NominalValue = new IfcText(styrofoam.Name);
                    }),
                        model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "GROSS VOLUME";
                        p.NominalValue = new IfcNumericMeasure(styrofoamTemplate.volume);
                    })
                    });
                });
            });

            styrofoam.PredefinedType = IfcSlabTypeEnum.NOTDEFINED;

            IFCMethods.ApplyRepresentationAndPlacement(model, styrofoam, shape, insertPlane);

            return styrofoam;
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
