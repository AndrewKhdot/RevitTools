
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.IO;

public static class ElementIdLoaderHelper
{
    public static List<ElementId> LoadFromTxt(string path)
    {
        var result = new List<ElementId>();

        if (!File.Exists(path))
            return result;

        foreach (var line in File.ReadAllLines(path))
        {
            if (int.TryParse(line.Trim(), out int id))
            {
                result.Add(new ElementId(id));
            }
        }

        return result;
    }
}
