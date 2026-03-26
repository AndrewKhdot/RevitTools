using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.Revit.Services;
using RevitTools.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.DuctAccessoryAnnotation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FireDampersCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;


            
        // Загрузка каталога
            string jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Config",
                "EquipmentCatalog.json"
            );
            const string paramNumber = "MC Object Variable 1";
            var config = new ConfigService(jsonPath).Load();
            var identifier = new EquipmentIdentifier(config);

            var collector = new DuctAccessoryCollectorService(doc);
            var filtering = new FilteringAccessoryService(doc, identifier);

            var allAccessories = collector.GetAccessories();
            var fireDampers = filtering.FilterFireDampers(allAccessories);
            var numbering = new DuctAccessoryNumberingService();
            var infoService = new DuctAccessoryInfoService(doc, identifier);
            var spaceLookup = new SpaceLookupService(doc);
            var annotationService = new DuctAccessoryAnnotationService(infoService, spaceLookup);
            // 1. Собираем уже используемые номера
            List<int> used = numbering.ExtractUsedNumbers(fireDampers, paramNumber);

            // 2. Создаём NumberPool
            var pool = new NumberPool(used);

            // 3. Нумеруем
            using (var t = new Transaction(doc, "Set firedumpes numbers"))
            {
                t.Start();
                numbering.PutNumbers(fireDampers, pool, paramNumber);
                t.Commit();
            }

            // 4. Аннотируем
            using (var t = new Transaction(doc, "Set firedumpes names"))
            {
                t.Start();
                annotationService.FireDampersAnnotation(fireDampers);
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
