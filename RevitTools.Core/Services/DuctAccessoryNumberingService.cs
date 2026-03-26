using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryNumberingService
    {
        public void PutNumbers(List<FamilyInstance> elements, NumberPool pool, string paramName)
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

      
        public List<int> ExtractUsedNumbers(List<FamilyInstance> elems, string paramName)
        {
            List<int> result = new List<int>();

            foreach (var elem in elems)
            {
                var p = elem.LookupParameter(paramName);
                if (p == null) continue;

                string val = p.AsString();
                if (int.TryParse(val, out int num))
                    result.Add(num);
            }

            return result;
        }
  
    }
    
}