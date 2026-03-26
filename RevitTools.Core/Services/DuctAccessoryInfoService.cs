using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryInfoService
    {
        private readonly EquipmentIdentifier _identifier;
        private readonly Document _doc;
        public DuctAccessoryInfoService (Document doc, EquipmentIdentifier identifier)
        {
            _doc = doc;
            _identifier = identifier;
        }
        public string GetFireDamperSize (FamilyInstance firedamper)
        {   
            string annotationSize = "";
            var type = _doc.GetElement(firedamper.GetTypeId()) as Element;
            if (type == null) return annotationSize;

            if (_identifier.IsRect(type.LookupParameter("MC Product Code")?.AsString() ?? ""))
                {
                double widthFeet = type.LookupParameter("MC Width")?.AsDouble() ?? 0;
                double widthMm = UnitUtils.ConvertFromInternalUnits(widthFeet, DisplayUnitType.DUT_MILLIMETERS);
                double heightFeet = type.LookupParameter("MC Height")?.AsDouble() ?? 0;
                double heightMm = UnitUtils.ConvertFromInternalUnits(heightFeet, DisplayUnitType.DUT_MILLIMETERS);
                annotationSize = $"jednopłaszczyznowa przeciwpożarowa klapa prostokątna {widthMm}x{heightMm}";
                    return annotationSize;
            }
            if (_identifier.IsCircle(type.LookupParameter("MC Product Code")?.AsString() ?? ""))
            {
                double widthFeet = type.LookupParameter("MC Width")?.AsDouble() ?? 0;
                double widthMm = UnitUtils.ConvertFromInternalUnits(widthFeet, DisplayUnitType.DUT_MILLIMETERS);
                annotationSize = $"jednopłaszczyznowa przeciwpożarowa klapa okrągła d={widthMm}";
                return annotationSize;
            }
            return annotationSize;
        }

    }
}