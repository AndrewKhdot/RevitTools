using Autodesk.Revit.DB;
using RevitTools.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.Core.Services
{
    public class LinkedCeilingService
    {
        private readonly LinkService _linkService;
        private readonly Document _doc;

        public LinkedCeilingService(Document doc, LinkService linkService)
        {
            _doc = doc;
            _linkService = linkService;
        }

        public List<CeilingInfo> GetLinkedCeilings()
        {
            List<CeilingInfo> ceilings = new List<CeilingInfo>();

            foreach (var link in _linkService.GetRevitLinks())
            {
                var linkDoc = _linkService.GetLinkDocument(link);
                if (linkDoc == null)
                    continue;

                var transform = _linkService.GetTransform(link);

                var linkedCeilings = new FilteredElementCollector(linkDoc)
                    .OfCategory(BuiltInCategory.OST_Ceilings)
                    .WhereElementIsNotElementType()
                    .Cast<Element>()
                    .ToList();

                foreach (var ceiling in linkedCeilings)
                {
                    var info = CreateCeilingInfo(ceiling, transform, link);
                    if (info != null)
                        ceilings.Add(info);
                }
            }

            return ceilings;
        }

        private CeilingInfo CreateCeilingInfo(Element ceiling, Transform transform, RevitLinkInstance link)
        {
            var box = ceiling.get_BoundingBox(null);
            if (box == null)
                return null;

            var transformedBox = _linkService.TransformBoundingBox(box, transform);
            if (transformedBox == null)
                return null;

            double elevation = transformedBox.Min.Z;

            return new CeilingInfo(ceiling.Id, transformedBox, elevation, link);
        }
    }
}
