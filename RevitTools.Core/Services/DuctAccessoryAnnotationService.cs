using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryAnnotationService
    {
        const string paramFireDampTypeAndSize = "MC Object Variable 2";
        const string paramFireDamPlace = "MC Object Variable 3";
        private readonly DuctAccessoryInfoService _infoservice;
        private readonly SpaceLookupService _spaceLookupService;

        public DuctAccessoryAnnotationService (DuctAccessoryInfoService infoservice, SpaceLookupService spaceLookupService)
        {

            _infoservice = infoservice;
            _spaceLookupService = spaceLookupService;
        }
        public void FireDampersAnnotation(List<FamilyInstance> elements)
        {

            foreach (var elem in elements)
            {
                var param = elem.LookupParameter(paramFireDampTypeAndSize);
                if (param == null || param.IsReadOnly)
                    continue;             
                param.Set(_infoservice.GetFireDamperSize(elem));

                var paramTwo = elem.LookupParameter(paramFireDamPlace);
                if (paramTwo == null || paramTwo.IsReadOnly)
                    continue;
                Space spase = _spaceLookupService.GetSpaceFor(elem);
                if (spase == null)
                    continue;
                string spaceInfo = $"{spase.Number} {spase.Name}";
                paramTwo.Set(spaceInfo);
            }
        }

    }
}