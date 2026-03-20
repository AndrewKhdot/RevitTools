using Autodesk.Revit.DB;
using RevitTools.Core.Models;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class CeilingService
    {

        private readonly Document _doc;
        private readonly RoomService _roomService;        

        public CeilingService(Document doc, RoomService roomService)
        {
            _doc = doc;
            _roomService = roomService;           
        }


        public List<Ceiling> GetCeilings ()
        {   
            var ceilings = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Ceilings)
                .WhereElementIsNotElementType()
                .Cast<Ceiling>() 
                .ToList();
            return ceilings;

        }

        public Ceiling GetCeiling(ElementId id)
            {
                return  _doc.GetElement(id) as Ceiling;
            }

        public Dictionary<ElementId, List<ElementId>> FindCeilingsForRoom()
        {
            Dictionary<ElementId, List<ElementId>> roomCeilingList = new Dictionary<ElementId, List<ElementId>>();
            var ceilings = GetCelings();
            foreach(var ceiling in ceilings ){
                BoundingBoxXYZ bb = ceiling.get_BoundingBox(null);
                    if (bb == null)
                    {
                        continue;
                    }
            
                    XYZ center = (bb.Min + bb.Max) * 0.5;
                    
                    XYZ testPoint = center - new XYZ(0, 0, 0.1); // опускаем точку вниз
                    Room room = _doc.GetRoomAtPoint(testPoint);
                    if (room == null)
                    {
                        continue;  
                    }
                    
                    if(roomCeilingList.ContainsKey(room.Id))
                    {
                        roomCeilingList[room.Id].Add(ceiling.Id);
                    }
                    else
                    {
                        List<ElementId> ceilingsForRoom = new List<ElementId>();
                        ceilingsForRoom.Add(ceiling.Id);
                        roomCeilingList.Add(room.Id, ceilingsForRoom);
                    }
                    
            }
            return roomCeilingList;

        }

        public double GetCeilingArea(ElementId ceilingId)
            {
                Ceiling ceiling = GetCeiling(ceilingId);
                if (ceiling == null)
                    return 0;
                Parameter p = ceiling.LookupParameter("Area");
                if (p == null) p = ceiling.LookupParameter("Площадь");
                if (p == null) p = ceiling.LookupParameter("Powierzchnia");
            
                if (p != null)
                    return p.AsDouble();
            
                return 0;
            }

        public void AttachCeilingForRoomInfo (List<RoomInfo> roomInfoList)
        {
            
            foreach (var ri in roomInfoList)
            {
             if (ri.CeilingIds.Count == 0)
	            return;
		
                double maxArea = double.MinValue;
                ElementId biggestCeiling = null;  
                foreach (var ceiling in Ceilings)
                {
                    double area = GetCeilingArea(ceiling);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        biggestCeiling = ceiling;
                    }
                }
                if (biggestCeiling == null)
                    return;
                CurrentCeiling = biggestCeiling; 
            }
        }
    }
}