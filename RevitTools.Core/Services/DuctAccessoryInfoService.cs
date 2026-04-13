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
                string[] size = modelValue.Split(new char[] { '/' });
                string[] sizeParam = size[1].Split(new char[] { 'x' });
                annotationSize = sizeParam[0] + "x" + sizeParam[1];
            }
            if (_identifier.IsCircle(modelValue))
            {
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


        public string GetSilenserPressureLoss(FamilyInstance silenser)
        {
            if (silenser == null)
                return string.Empty;

            string value = GetPressureLossParam(silenser, "TX_Δpst");
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            value = GetPressureLossParam(silenser, "TX_Static_differential_pressure");
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            return string.Empty;
        }



        public string GetAccessoryConSize (FamilyInstance accessory)
        {
            string sizeString = "";            
        if (accessory.MEPModel == null)
            return sizeString;

        ConnectorSet connectors = accessory.MEPModel.ConnectorManager.Connectors;

        foreach (Connector c in connectors)
        {
            if (c.Domain != Domain.DomainHvac)
                continue;

            if (c.Shape == ConnectorProfileType.Round)
            {
                double diameterFt = c.Radius * 2;
                
                int diameterMm = RoundMm(diameterFt);
                sizeString =$"⌀{diameterMm}";

                // диаметр присоединения
            }
            else if (c.Shape == ConnectorProfileType.Rectangular)
            {
                double widthFt = c.Width;
                double heightFt = c.Height;

                int widthMm =RoundMm(widthFt);
                int heightMm = RoundMm(heightFt);
                sizeString = $"{widthMm}x{heightMm}";
                // ширина и высота присоединения
            }
        }
            return sizeString;
        }

        
        int RoundMm(double valueInFeet)
        {
            double mm = UnitUtils.ConvertFromInternalUnits(
                valueInFeet,
                DisplayUnitType.DUT_MILLIMETERS);

            return (int)System.Math.Round(mm, System.MidpointRounding.AwayFromZero);
        }

        private static string GetPressureLossParam(Element element, string paramName)
        {
            Parameter p = element.LookupParameter(paramName);
            if (p == null || !p.HasValue)
                return string.Empty;

            // ✅ Для Double-параметров давления
            return p.AsValueString() ?? string.Empty;
        }


    }

    
}