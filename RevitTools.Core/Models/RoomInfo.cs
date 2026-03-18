using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitTools.Core.Models
{
    public class RoomInfo
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }

        public double StartHeight { get; set; }
        public double SlabBottomElevation { get; set; }

        public ElementId LevelId { get; set; }
        public string LevelName { get; set; }

        public List<ElementId> CeilingIds { get; set; } = new List<ElementId>();
        public ElementId CurrentCeilingId { get; set; }

        public bool WillBeChanged { get; set; }

        public RoomInfo() { }

        public RoomInfo(ElementId id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
