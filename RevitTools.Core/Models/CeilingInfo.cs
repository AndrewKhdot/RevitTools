using Autodesk.Revit.DB;

public class CeilingInfo
{
    public ElementId Id { get; }
    public BoundingBoxXYZ Box { get; }
    public XYZ Center { get; }
    public double Elevation { get; }
    public ElementId LevelId { get; }
    public RevitLinkInstance LinkInstance { get; }

    public CeilingInfo(
        ElementId id,
        BoundingBoxXYZ box,
        ElementId levelId,
        double elevation,
        RevitLinkInstance linkInstance = null)
    {
        Id = id;
        Box = box;
        LevelId = levelId;
        Elevation = elevation;
        LinkInstance = linkInstance;
        Center = (box.Min + box.Max) * 0.5;
    }
}
