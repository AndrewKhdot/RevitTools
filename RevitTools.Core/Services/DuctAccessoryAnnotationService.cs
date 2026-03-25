using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryAnnotationService
    {
        private readonly EquipmentIdentifier _identifier;
        public DuctAccessoryAnnotationService (Document doc, EquipmentIdentifier identifier)
        {
            
            _identifier = identifier;
        }
        public void FireDampersAnnotation(List<FamilyInstance> elements, NumberPool pool, string paramName)
        {
            foreach (var elem in elements)
            {
                var param = elem.LookupParameter(paramName);
                if (param == null || param.IsReadOnly)
                    continue;

                string val = param.AsString();
                if (!string.IsNullOrEmpty(val))
                    continue; // номер уже есть

                int next = pool.GetNext();
                param.Set(next.ToString());
            }
        }

    }
}