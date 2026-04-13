using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class DuctAccessoryAnnotationService
    {
        const string paramFireDampTypeAndSize = "MC Object Variable 2";
        const string paramFireDamPlace = "MC Object Variable 3";
        const string paramSilencerSize = "MC Object Variable 1";
        const string paramSilencerLength = "MC Object Variable 2";
        const string paramSilencerPressLost = "MC Object Variable 3";
        const string paramBalancDamPlace = "MC Object Variable 1";
        const string paramBalancDamMark = "MC Object Variable 2";
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

        public void BalancingDampersAnnotation(List<FamilyInstance> elements)
        {

            foreach (var elem in elements)
            {                
                var param = elem.LookupParameter(paramBalancDamPlace);
                var paramTwo = elem.LookupParameter(paramBalancDamMark);
                
                if (param == null || param.IsReadOnly)
                    continue;
                Space spase = _spaceLookupService.GetSpaceFor(elem);
                if (spase == null)
                    continue;
                string spaceInfo = $"{spase.Number} {spase.Name}";
                param.Set(spaceInfo);
                if (paramTwo == null || paramTwo.IsReadOnly)
                    continue;
                paramTwo.Set("Przepustnica");
            }
        }

        public void SilencersAnnotation(List<FamilyInstance> elements)
        {

            foreach (var elem in elements)
            {
                var param = elem.LookupParameter(paramSilencerSize);
                if (param == null || param.IsReadOnly)
                    continue;
                param.Set(_infoservice.GetSilenserSize(elem));

                var paramTwo = elem.LookupParameter(paramSilencerLength);
                if (paramTwo == null || paramTwo.IsReadOnly)
                    continue;        
                paramTwo.Set(_infoservice.GetSilenserLength(elem));

                var paramThree = elem.LookupParameter(paramSilencerPressLost);
                if (paramThree == null || paramThree.IsReadOnly)
                    continue;        
                paramThree.Set(_infoservice.GetSilenserPressureLoss(elem));
            }
        }
    }
}