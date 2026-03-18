using Autodesk.Revit.DB;
using RevitTools.Core.Models;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class CeilingService
    {
        private readonly Document _doc;

        public CeilingService(Document doc)
        {
            _doc = doc;
        }

        public void AttachCeilings(RoomInfo roomInfo)
        {
            // Здесь позже добавим логику поиска потолков
            // Пока просто создаём каркас
        }
    }
}