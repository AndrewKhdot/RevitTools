using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.Core.Services
{
    public class LinkService
    {
        private readonly Document _doc;

        public LinkService(Document doc)
        {
            _doc = doc;
        }

        public List<RevitLinkInstance> GetRevitLinks()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(l => l.GetLinkDocument() != null) // исключаем выгруженные связи
                .ToList();
        }


        public Document GetLinkDocument(RevitLinkInstance link)
        {
            return link?.GetLinkDocument();
        }


        public Transform GetTransform(RevitLinkInstance link)
        {
            return link.GetTransform();
        }


        public BoundingBoxXYZ TransformBoundingBox(BoundingBoxXYZ box, Transform transform)
        {
            if (box == null)
                return null;

            XYZ min = transform.OfPoint(box.Min);
            XYZ max = transform.OfPoint(box.Max);

            return new BoundingBoxXYZ
            {
                Min = new XYZ(
                    System.Math.Min(min.X, max.X),
                    System.Math.Min(min.Y, max.Y),
                    System.Math.Min(min.Z, max.Z)
                ),
                Max = new XYZ(
                    System.Math.Max(min.X, max.X),
                    System.Math.Max(min.Y, max.Y),
                    System.Math.Max(min.Z, max.Z)
                )
            };
        }


        public XYZ TransformPoint(XYZ point, Transform transform)
        {
            return transform.OfPoint(point);
        }

    }
}
