using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

public class SpaceLookupService
{
    private readonly Document _doc;

    public SpaceLookupService(Document doc)
    {
        _doc = doc;
    }

    private XYZ GetPoint(FamilyInstance inst)
    {
        if (inst.Location is LocationPoint lp)
            return lp.Point;

        if (inst.Location is LocationCurve lc)
            return lc.Curve.Evaluate(0.5, true);

        return null;
    }

    public Space GetSpaceFor(FamilyInstance inst)
    {
        XYZ p = GetPoint(inst);
        if (p == null)
            return null;

        XYZ test = p - new XYZ(0, 0, 0.1);

        return _doc.GetSpaceAtPoint(test);
    }
}