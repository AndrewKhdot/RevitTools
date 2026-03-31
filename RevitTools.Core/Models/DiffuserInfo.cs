using Autodesk.Revit.DB;
using System.Collections.Generic;


namespace RevitTools.Core.Models
{
    public class DiffuserInfo
    {
        public ElementId Id { get; }
        public BoundingBoxXYZ Box { get; set; }
        public double Elevation { get; set; }
        public XYZ Center { get; }
        public ElementId LevelId { get; set; }

        public List<CeilingInfo>  CeilingInfos { get; set; } = new List<CeilingInfo>();

        public DiffuserInfo(ElementId id, BoundingBoxXYZ box, ElementId levelId)
        {
            Id = id;
            Box = box;
            LevelId = levelId;
            Center = (box.Min + box.Max) * 0.5;
            Elevation = Box.Min.Z;
        }
    }
}
