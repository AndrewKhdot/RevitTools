using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

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

            ShowBoundingBox(box, new Color(255, 0, 0));
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

        public void ShowBoundingBox(BoundingBoxXYZ box, Color color)
        {
            if (box == null)
                return;

            var corners = GetCorners(box);

            using (var t = new Transaction(_doc, "Show BoundingBox"))
            {
                t.Start();

                var ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetName("TempBoundingBox");

                var geomList = new List<GeometryObject>();

                geomList.Add(Line.CreateBound(corners[0], corners[1]));
                geomList.Add(Line.CreateBound(corners[1], corners[2]));
                geomList.Add(Line.CreateBound(corners[2], corners[3]));
                geomList.Add(Line.CreateBound(corners[3], corners[0]));

                geomList.Add(Line.CreateBound(corners[4], corners[5]));
                geomList.Add(Line.CreateBound(corners[5], corners[6]));
                geomList.Add(Line.CreateBound(corners[6], corners[7]));
                geomList.Add(Line.CreateBound(corners[7], corners[4]));

                geomList.Add(Line.CreateBound(corners[0], corners[4]));
                geomList.Add(Line.CreateBound(corners[1], corners[5]));
                geomList.Add(Line.CreateBound(corners[2], corners[6]));
                geomList.Add(Line.CreateBound(corners[3], corners[7]));

                ds.SetShape(geomList);

                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetProjectionLineColor(color);

                var view = _doc.ActiveView;
                view.SetElementOverrides(ds.Id, ogs);

                _tempShapes.Add(ds.Id);

                t.Commit();
            }
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

            using (var t = new Transaction(_doc, "Clear Highlights"))
            {
                t.Start();

                foreach (var id in _tempShapes)
                {
                    var el = _doc.GetElement(id);
                    if (el != null)
                        _doc.Delete(id);
                }

                _tempShapes.Clear();

                t.Commit();
            }
        }
    }
}
