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

        public RoomInfo CreateRoomInfo(Room room)
        {
            var info = new RoomInfo(room.Id, room.Name);

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
    }
}
