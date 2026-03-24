using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
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

            var config = new ConfigService(jsonPath).Load();
            var identifier = new EquipmentIdentifier(config);

            var collector = new DuctAccessoryCollectorService(doc);
            var filtering = new FilteringAccessoryService(doc, identifier);

            var allAccessories = collector.GetAccessories();
            var dampers = filtering.FilterFireDampers(allAccessories);

            
            return Result.Succeeded;
        }
    }
}
