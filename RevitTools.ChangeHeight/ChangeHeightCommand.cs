using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitTools.Core.Services;
using System.Linq;
using Autodesk.Revit.DB.Architecture;

namespace RevitTools.ChangeHeight
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ChangeHeightCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;

            // Создаём сервисы
            var roomService = new RoomService(doc);

            // Получаем помещения
            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>();

            using (var t = new Transaction(doc, "Change Room Height"))
            {
                t.Start();

               

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
