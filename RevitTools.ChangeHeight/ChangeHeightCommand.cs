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
            var floorService = new FloorService(doc);
            var ceilinService =  new CeilingService(doc);
            var floors = floorService.GetFloors();
            var rooms = roomService.GetRooms();
            var ceilings = ceilinService.GetCelings();      
            var roomInfoList = roomService.CreateRoomInfosList(rooms);

            foreach (var roomInfo in roomInfoList)
            {
                roomInfo.SlabBottomElevation = floorService.FindFullHeightRoom(roomInfo, floors);
            }



            using (var t = new Transaction(doc, "Change Room Height First Time For Looking For Ceilings"))
            {
                t.Start();
                
               foreach (var roomInfo in roomInfoList)
               {
                roomService.ApplyRoomOffset(roomInfo);
               }

                t.Commit();
            }

            var roomCeilingList = ceilinService.FindCeilingsForRoom();
            roomService.ApplyCeilingsInRooms(roomCeilingList, roomInfoList);
            


            return Result.Succeeded;
        }
    }
}
