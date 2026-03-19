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
    public class FloorService
    {
        private readonly Document _doc;

        public FloorService(Document doc)
        {
            _doc = doc;
        }


        
        public double FindFullHeightRoom (RoomInfo info, IEnumerable<Floor> floors)
        {
            Room room = _doc.GetElement(info.Id) as Room;
            BoundingBoxXYZ bb = room.get_BoundingBox(null);
			double roomTop = bb.Max.Z;				
			double currentFloor = double.MaxValue;
            foreach (var floor in floors) {
				
				BoundingBoxXYZ fbb = floor.get_BoundingBox(null);
                
                if (fbb == null) continue;
	
				bool intersectsXY =
				    fbb.Max.X > bb.Min.X &&
				    fbb.Min.X < bb.Max.X &&
				    fbb.Max.Y > bb.Min.Y &&
				    fbb.Min.Y < bb.Max.Y;	
				if(!intersectsXY)
				{
					continue;
				}
				Level lvl = _doc.GetElement(floor.LevelId) as Level;
				double levelElevation = lvl.Elevation;
				
				// Offset
				Parameter offsetParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
				double offset = offsetParam != null ? offsetParam.AsDouble() : 0;
				
				// Thickness
				Parameter thickParam = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
				double thickness = thickParam != null ? thickParam.AsDouble() : 0;
				
				// Абсолютная отметка низа перекрытия
				double floorBottom = levelElevation + offset - thickness;
				
				if(floorBottom <= roomTop)
				{
					continue;
				}				
				if(floorBottom < currentFloor)
				{
					currentFloor = floorBottom;				
				}

            }
            // Если перекрытия нет — помещение на последнем этаже
            if (currentFloor == double.MaxValue)
            {
                // Можно оставить 0 или не менять SlabBottomElevation
                    return 0;
            }
            else
            {
                return currentFloor;
            }
            
        }

        public IEnumerable<Floor> GetFloors ()
        {
            var floors = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .Cast<Floor>()
                .ToList();  

            return floors;
        }

    }
}
