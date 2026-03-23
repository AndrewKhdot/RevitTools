using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            var ceilings = ceilinService.GetCeilings();      
            var roomInfoList = roomService.CreateRoomInfosList(rooms);
            List<RoomInfo> roomInfoSelected = new List<RoomInfo>();


            var levelElements = new Dictionary<ElementId, string>();

            foreach (var roomInf in roomInfoList)
            {
                if (!levelElements.ContainsKey(roomInf.LevelId))
                {
                    levelElements.Add(roomInf.LevelId, roomInf.LevelName);
                }
            }
            var formForLevels = new ElementSelectorForm(levelElements);

            if (formForLevels.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return Result.Cancelled;

            var selectedLevels = formForLevels.SelectedElements;

                foreach (var roomInfo in roomInfoList)
                {
                    if (selectedLevels.ContainsKey(roomInfo.LevelId))
                    {
                        roomInfoSelected.Add(roomInfo);
                    }
                }
    
                if (roomInfoSelected.Count == 0)
                {
                    MessageBox.Show("No rooms selected. Please select at least one level.");
                    return Result.Cancelled;
                }
    
                roomInfoList = roomInfoSelected;

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
            ceilinService.AttachBiggestCeilingForRoomInfo(roomInfoList);

            var roomElements = new Dictionary<ElementId, string>();

            foreach (var roomInf in roomInfoList)
            {

                double height = (roomInf.SlabBottomElevation - roomService.GetLevel(roomInf.LevelId).Elevation) * 0.3048;

                string roomHeader = roomInf.Number + " " + roomInf.Name + " – " + roomInf.LevelName + " - " + height.ToString();
                roomElements.Add(roomInf.Id, roomHeader);
            }

            var form = new ElementSelectorForm(roomElements);

            if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                foreach (var roomInfo in roomInfoList)
                    {
                        roomInfo.WillBeChanged = false;
                    }
                }

            // Получаем выбранные элементы
            var selected = form.SelectedElements;

            using (var t = new Transaction(doc, "Change Room Height First Time For Looking For Ceilings"))
            {
                t.Start();

                foreach (var roomInfo in roomInfoList)
                {
                    roomService.ApplyRoomOffset(roomInfo, false);
                }

                t.Commit();
            }



            return Result.Succeeded;
        }
    }
}
