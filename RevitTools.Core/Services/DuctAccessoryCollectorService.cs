using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RevitTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryCollectorService
    {
        private readonly Document _doc;

        public DuctAccessoryCollectorService (Document doc)
        {
            _doc = doc;
        }

        
        public List<FamilyInstance> GetAccessories()
        {
            return new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_DuctAccessory)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();
        }

        public FamilyInstance GetAccessory(ElementId id)
        {
            return _doc.GetElement(id) as FamilyInstance;
        }

    }
}