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
    public class ChangeHeightCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;


            // 1. Путь к JSON (вариант 2: JSON в репозитории)
            string jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Config",
                "EquipmentCatalog.json"
            );

            // 2. Создаём сервис загрузки конфигурации
            var configService = new ConfigService(jsonPath);

            // 3. Загружаем EquipmentCatalog
            var catalog = configService.Load();

            // 4. Создаём идентификатор оборудования
            var identifier = new EquipmentIdentifier(catalog);


            // Создаём сервисы
            var roomService = new RoomService(doc);
            
            return Result.Succeeded;
        }
    }
}
