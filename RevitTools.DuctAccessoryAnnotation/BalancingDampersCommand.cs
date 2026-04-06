using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTools.Core.Services;
using RevitTools.Revit.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.DuctAccessoryAnnotation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class BalancingDampersCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                LoggingService.Log("Start FireDampersCommand");
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;



                // Загрузка каталога
                string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string jsonPath = Path.Combine(pluginFolder, "Config", "EquipmentCatalog.json");
                var config = new ConfigService(jsonPath).Load();
                var identifier = new EquipmentIdentifier(config);

                var collector = new DuctAccessoryCollectorService(doc);
                var filtering = new FilteringAccessoryService(doc, identifier);

                var allAccessories = collector.GetAccessories();
                var balancingDampers = filtering.FilterBalancingDampers(allAccessories);
                var infoService = new DuctAccessoryInfoService(doc, identifier);
                var spaceLookup = new SpaceLookupService(doc);
                var annotationService = new DuctAccessoryAnnotationService(infoService, spaceLookup);

                using (var t = new Transaction(doc, "Set balancing dampers places"))
                {
                    t.Start();
                    annotationService.FireDampersAnnotation(balancingDampers);
                    t.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Показываем ошибку пользователю
                TaskDialog.Show("Ошибка", ex.Message);

                // Возвращаем ошибку Revit
                message = ex.ToString();
                return Result.Failed;
            }
        }
    }
}
