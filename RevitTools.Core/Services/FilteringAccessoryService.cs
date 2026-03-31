using Autodesk.Revit.DB;
using RevitTools.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.Revit.Services
{
    public class FilteringAccessoryService
    {
        private readonly Document _doc;
        private readonly EquipmentIdentifier _identifier;

        public FilteringAccessoryService(Document doc, EquipmentIdentifier identifier)
        {
            _doc = doc;
            _identifier = identifier;
        }

        public List<FamilyInstance> FilterFireDampers(List<FamilyInstance> accessories)
        {
            var result = new List<FamilyInstance>();
            LoggingService.Log("Start filtring firedumpers");

            foreach (var ac in accessories)
            {
                var type = _doc.GetElement(ac.GetTypeId()) as Element;
                if (type == null) continue;

                var modelParam = type.LookupParameter("MC Product Code");
                string code = modelParam?.AsString() ?? "";

                if (_identifier.IsFireDamper(code))
                    result.Add(ac);
            }

            return result;
        }

        public List<FamilyInstance> FilterSilencers(List<FamilyInstance> accessories)
        {
            var result = new List<FamilyInstance>();

            LoggingService.Log("Start filtring silencers");

            foreach (var ac in accessories)
            {
                var type = _doc.GetElement(ac.GetTypeId()) as Element;
                if (type == null) continue;                
                string[] names = { "Model", "Группа модели" };
                var modelParam = names.Select(n => type.LookupParameter(n)).FirstOrDefault(p => p != null);
                string code = modelParam?.AsString() ?? "";                
                if (_identifier.IsSilencer(code))
                    result.Add(ac);
            }

            return result;
        }
    }
}