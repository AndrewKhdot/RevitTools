
using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace MyMacros
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class RoomInfo
	{
		public Document Doc {get; private set;}
		public Room RoomElement {get; private set;}
		public ElementId Id {get; private set;}
		public double StartHeight { get; set; }
		public double SlabBottomElevation { get; set; }
		public List<Ceiling> Ceilings {get; set;}
		public Ceiling CurrentCeiling {get; set; }
		public string InsideInfo {get; set;}
		public bool WillBeChanged {get; set;}
		public Level Level { get; private set; }
		public ElementId LevelId { get; private set; }		
		public string LevelName
		{
		    get
		    {
		        if (Level != null)
		            return Level.Name;
		        else
		            return "Этаж не найден";
		    }
		}

		
		public RoomInfo(Document doc, Room room)
    	{
			Doc = doc;
	        RoomElement = room;
	        Id = room.Id;
	        StartHeight = room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
	        Ceilings = new List<Ceiling>();
	        WillBeChanged = false;      

		    // Получаем LevelId из параметра помещения
		    LevelId = room.get_Parameter(BuiltInParameter.ROOM_LEVEL_ID).AsElementId();
		
		    // Получаем сам Level
		    Level = doc.GetElement(LevelId) as Level;


    	}
		
		public void PutSlabBottomElevation(IEnumerable<Floor> floors)
		{
			BoundingBoxXYZ bb = RoomElement.get_BoundingBox(null);
			double roomTop = bb.Max.Z;				
			double currentFloor = double.MaxValue;
			
			foreach (var floor in floors) {
				
				BoundingBoxXYZ fbb = floor.get_BoundingBox(null);	
				bool intersectsXY =
				    fbb.Max.X > bb.Min.X &&
				    fbb.Min.X < bb.Max.X &&
				    fbb.Max.Y > bb.Min.Y &&
				    fbb.Min.Y < bb.Max.Y;	
				if(!intersectsXY)
				{
					continue;
				}
				Level lvl = Doc.GetElement(floor.LevelId) as Level;
				double levelElevation = lvl.Elevation;
				
				// Offset
				Parameter offsetParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
				double offset = offsetParam != null ? offsetParam.AsDouble() : 0;
				
				// Thickness
				Parameter thickParam = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
				double thickness = thickParam != null ? thickParam.AsDouble() : 0;
				
				// Абсолютная отметка низа перекрытия
				double floorBottom = levelElevation + offset - thickness;
				this.InsideInfo = thickness.ToString();

				
				if(floorBottom <= roomTop)
				{
					continue;
				}				
				if(floorBottom < currentFloor)
				{
					currentFloor = floorBottom;				
				}
				// Если перекрытия нет — помещение на последнем этаже
			    if (currentFloor == double.MaxValue)
			    {
			        // Можно оставить 0 или не менять SlabBottomElevation
			        this.SlabBottomElevation = 0;
			    }
			    else
			    {
			        this.SlabBottomElevation = currentFloor;
			    }							
				
			}
		}
		
		public void AddCelling(Ceiling ceiling)
		{			
			Ceilings.Add(ceiling);
		}
		public double GetCeilingArea(Ceiling ceiling)
		{
		    Parameter p = ceiling.LookupParameter("Area");
		    if (p == null) p = ceiling.LookupParameter("Площадь");
		    if (p == null) p = ceiling.LookupParameter("Powierzchnia");
		
		    if (p != null)
		        return p.AsDouble();
		
		    return 0;
		}
		public void PutSlabBottomElevationByCeiling()
		{	
			if (Ceilings.Count == 0)
	        return;
		
		    double maxArea = double.MinValue;
    		Ceiling biggestCeiling = null;
		
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

		    // Получаем уровень
		    Level lvl = Doc.GetElement(CurrentCeiling.LevelId) as Level;
		    double baseElevation = lvl.Elevation;
		
		    // Получаем смещение
		    Parameter offsetParam = CurrentCeiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM);
		    double offset = offsetParam != null ? offsetParam.AsDouble() : 0;
		
		    // Абсолютная отметка низа потолка
		    double ceilingElevation = baseElevation + offset;
		
		    // Сохраняем
		    this.SlabBottomElevation = ceilingElevation;
		}
	}
}
