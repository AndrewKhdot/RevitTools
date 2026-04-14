using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;


namespace RevitTools.Core.Services
{
    public class HighlightService
    {
        private readonly Document _doc;
        private readonly List<ElementId> _tempShapes = new List<ElementId>();
        private readonly HashSet<ElementId> _watchedIds;

        public HighlightService(Document doc, IEnumerable<ElementId> watchedIds)
        {
            _doc = doc;
            _watchedIds = new HashSet<ElementId>(watchedIds);
        }

        public void DemoBoundingBoxStep(ElementId id, BoundingBoxXYZ box, string message)
        {
            if (!_watchedIds.Contains(id))
                return;

            ShowBoundingBox(box);
            Pause(message);
            Clear();
        }


        private List<XYZ> GetCorners(BoundingBoxXYZ box)
        {
            var min = box.Min;
            var max = box.Max;

            return new List<XYZ>
            {
                new XYZ(min.X, min.Y, min.Z),
                new XYZ(max.X, min.Y, min.Z),
                new XYZ(max.X, max.Y, min.Z),
                new XYZ(min.X, max.Y, min.Z),

                new XYZ(min.X, min.Y, max.Z),
                new XYZ(max.X, min.Y, max.Z),
                new XYZ(max.X, max.Y, max.Z),
                new XYZ(min.X, max.Y, max.Z)
            };
        }

        public void ShowBoundingBox(BoundingBoxXYZ box)
        {
            if (box == null)
                return;

            Solid solid = CreateSolidFromBoundingBox(box);

            var ds = DirectShape.CreateElement(
                _doc,
                new ElementId(BuiltInCategory.OST_GenericModel)
            );

            ds.SetName("TempBoundingBoxSolid");
            ds.SetShape(new List<GeometryObject> { solid });

            ApplyGreenTransparentOverride(ds.Id);

            _tempShapes.Add(ds.Id);
        }

        private Solid CreateSolidFromBoundingBox(BoundingBoxXYZ box)
        {
            Transform tr = box.Transform;

            XYZ min = tr.OfPoint(box.Min);
            XYZ max = tr.OfPoint(box.Max);

            var p0 = new XYZ(min.X, min.Y, min.Z);
            var p1 = new XYZ(max.X, min.Y, min.Z);
            var p2 = new XYZ(max.X, max.Y, min.Z);
            var p3 = new XYZ(min.X, max.Y, min.Z);

            var loop = CurveLoop.Create(new List<Curve>
            {
                Line.CreateBound(p0, p1),
                Line.CreateBound(p1, p2),
                Line.CreateBound(p2, p3),
                Line.CreateBound(p3, p0)
            });

            double height = max.Z - min.Z;

            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { loop },
                XYZ.BasisZ,
                height
            );
        }

        private void ApplyGreenTransparentOverride(ElementId id)
        {
            var ogs = new OverrideGraphicSettings();

            ogs.SetSurfaceForegroundPatternColor(
                new Color(0, 255, 0)
            );

            ogs.SetSurfaceTransparency(70);

            ElementId solidFillId = GetSolidFillPatternId();
            if (solidFillId != ElementId.InvalidElementId)
            {
                ogs.SetSurfaceForegroundPatternId(solidFillId);
            }

            _doc.ActiveView.SetElementOverrides(id, ogs);
        }

        private ElementId GetSolidFillPatternId()
        {
            var solidFill = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(fp =>
                    fp.GetFillPattern().IsSolidFill
                );

            return solidFill?.Id ?? ElementId.InvalidElementId;
        }


        public void Pause(string message = "Press Continue to proceed")
        {
            TaskDialog td = new TaskDialog("Demo Pause");
            td.MainInstruction = message;
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Continue");
            td.Show();
        }

        public void Clear()
        {
            if (_tempShapes.Count == 0)
                return;



                foreach (var id in _tempShapes)
                {
                    var el = _doc.GetElement(id);
                    if (el != null)
                        _doc.Delete(id);
                }

                _tempShapes.Clear();

        }
    }
}
