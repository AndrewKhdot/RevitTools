using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFirstPlugin
{
    [Transaction(TransactionMode.Manual)]
    internal class CommandThree : IExternalCommand
    {
        public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                                    .OfClass(typeof(Duct))
                                    .WhereElementIsNotElementType();
            double totalLength = 0;
            var totalReport = new Dictionary<string, double>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("ОТЧЁТ ПО ВОЗДУХОВОДАМ:");
            sb.AppendLine();
            // Перебираем все воздуховоды
            foreach (Duct duct in collector)
            {
                // Длина хранится в параметре RvtBuiltInParameter.CURVE_ELEM_LENGTH
                Parameter lengthParam = duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                string ductSize = null;

                if (lengthParam != null)
                {
                    if (duct.DuctType.Shape == ConnectorProfileType.Round)
                    {
                        ductSize = duct.Diameter.ToString();
                    }
                    else if (duct.DuctType.Shape == ConnectorProfileType.Rectangular)
                    {
                        ductSize = $"{duct.Width}x{duct.Height}";
                    }

                    if (ductSize != null && totalReport.ContainsKey(ductSize))
                    {

                        double currebtlength = 0;
                        totalReport.TryGetValue(ductSize, out currebtlength);
                        currebtlength += lengthParam.AsDouble();
                        totalReport[ductSize] = currebtlength;
                    }
                    else if (ductSize != null)
                    {
                        totalReport.Add(ductSize, lengthParam.AsDouble());
                    }

                }

            }

            foreach (var item in totalReport)
                {
                    sb.AppendLine($"Size: {item.Key} - Total Length: {item.Value * 0.3048:F2} m");
            }

            

            TaskDialog.Show("Total Duct Length",sb.ToString());

            using (Transaction tx = new Transaction(doc, "Update Duct Comments"))
            {
                tx.Start();

                foreach (Duct duct in collector)
                {
                    Parameter commentParam = duct.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    if (commentParam != null && !commentParam.IsReadOnly)
                    {
                        commentParam.Set("Reviewed by API");
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
