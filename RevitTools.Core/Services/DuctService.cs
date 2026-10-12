using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using RevitTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.Core.Services
{


    public class DuctService
    {
        private readonly Document _doc;


        public DuctService(Document doc)
        {
            _doc = doc;

        }

        public Duct GetDuct(ElementId id)
        {
            return _doc.GetElement(id) as Duct;
        }

        public List<Duct> GetDucts(IEnumerable<ElementId> ids)
        {
            return ids.Select(GetDuct).Where(d => d != null).ToList();
        }
    }
}
