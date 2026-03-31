using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

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

        public string GetSilenserSize(FamilyInstance silenser)
        {
            string annotationSize = "";
            var type = _doc.GetElement(silenser.GetTypeId()) as Element;
            if (type == null) return annotationSize;
            string[] names = { "Model", "Группа модели" };
            var modelParam = names.Select(n => type.LookupParameter(n)).FirstOrDefault(p => p != null);
            string modelValue = modelParam?.AsString() ?? "";
            if (_identifier.IsRect(modelValue))
            {
                LoggingService.Log($"Model value for rec silenser: {modelValue}");
                string[] size = modelValue.Split(new char[] { '/' });
                string[] sizeParam = size[1].Split(new char[] { 'x' });
                annotationSize = sizeParam[0] + "x" + sizeParam[1];
            }
            if (_identifier.IsCircle(modelValue))
            {
                LoggingService.Log($"Model value for cir silenser: {modelValue}");
                string[] size = modelValue.Split(new char[] { '/' });
                string[] sizeParam = size[1].Split(new char[] { 'x' });
                string diameter = sizeParam[0];
                annotationSize = "⌀" + sizeParam[0];
            }
                return annotationSize;
            

        }

        public string GetSilenserLength(FamilyInstance silenser)
        {
            string annotationLength = "";
            var type = _doc.GetElement(silenser.GetTypeId()) as Element;
            if (type == null) return annotationLength;
            string[] names = { "Model", "Группа модели" };
            var modelParam = names.Select(n => type.LookupParameter(n)).FirstOrDefault(p => p != null);
            string modelValue = modelParam?.AsString() ?? "";
            if (_identifier.IsRect(modelValue))
            {
                string[] size = modelValue.Split(new char[] { '/' });
                string[] sizeParam = size[1].Split(new char[] { 'x' });
                annotationLength = sizeParam[2];
            }
            if (_identifier.IsCircle(modelValue))
            {
                string[] size = modelValue.Split(new char[] { '/' });
                string[] sizeParam = size[1].Split(new char[] { 'x' });
                annotationLength = sizeParam[1];
            }
            return annotationLength;


        }

    }
}