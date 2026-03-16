using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFirstPlugin
{
    [Transaction(TransactionMode.ReadOnly)]
    internal class CommandTwo : IExternalCommand
    {   
        public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Categories categories = doc.Settings.Categories;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("ОТЧЁТ ПО ЭЛЕМЕНТАМ:");
            sb.AppendLine();

            foreach (Category category in categories)
            {
                int count = new FilteredElementCollector(doc)
                    .OfCategoryId(category.Id)
                    .WhereElementIsNotElementType()
                    .GetElementCount();

                if (count > 0)
                {
                    sb.AppendLine(category.Name + " - " + count.ToString());                    
                }
            }
            TaskDialog.Show("Element Counter",
               sb.ToString());

            return Result.Succeeded;
        }
    }
}
