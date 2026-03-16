using Autodesk.Revit.DB;

namespace MyMacros
{
	public class ElementItem
	{
	    public ElementId Id { get; set; }
	    public string Display { get; set; }
	
	    public override string ToString()
	    {
	        return Display;
	    }
	}
}
