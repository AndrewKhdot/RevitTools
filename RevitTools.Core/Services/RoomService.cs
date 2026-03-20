using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RevitTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.Core.Services
{
    public class RoomService
    {
        private readonly Document _doc;

        public RoomService(Document doc)
        {
            _doc = doc;
        }

        public List<RoomInfo> CreateRoomInfosList (List<Room> rooms)
        {   
            List<RoomInfo> roomInfoList = new List<RoomInfo>();
             foreach (var room in rooms)
                {
                    var info = CreateRoomInfo(room);   
                    roomInfoList.Add(info);                
                }
            return roomInfoList;

        }

        public List<Room> GetRooms ()
        {   
            var rooms = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .ToList();
            return rooms;

        }

        public Room GetRoom(ElementId id)
        {
            return  _doc.GetElement(id) as Room;
        }

        public RoomInfo CreateRoomInfo(Room room)
        {
            var info = new RoomInfo(room.Id, room.Name, room.Number);

            // Уровень
            info.LevelId = room.LevelId;
            var level = _doc.GetElement(room.LevelId) as Level;
            info.LevelName = level?.Name;

            // Высота помещения
            var heightParam = room.get_Parameter(BuiltInParameter.ROOM_HEIGHT);
            if (heightParam != null)
                info.StartHeight = heightParam.AsDouble();

            // Пока потолки не ищем — это будет в CeilingService
            // info.CeilingIds = ...

            return info;
        }

        public void ApplyRoomOffset (RoomInfo roomInfo)
        {
            Room room = GetRoom(roomInfo.Id);
            Parameter upperOffset = room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                if (!upperOffset.IsReadOnly)
                {
                    upperOffset.Set(roomInfo.SlabBottomElevation);
                }
        }

    }
}
