using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.ChangeHeight
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CeilingDiffuserElevationCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;


                // Создаём сервисы              

                var diffuserService = new DiffuserService(doc);
                var linkedService = new LinkService(doc);
                var linkedCeilingService = new LinkedCeilingService(doc, linkedService);
                var intersactionService = new IntersectionService();

                var allDiffusers = diffuserService.GetDiffusers();
                var diffusers = diffuserService.GetDiffusersWithFlex(allDiffusers);
                var diffuserInfos = diffuserService.CreateDiffuserInfoList(diffusers);
                diffuserService.ExpandBoundingBoxes(diffuserInfos);
                var ceilingInfos = linkedCeilingService.GetLinkedCeilings();
                intersactionService.FindIntersections(diffuserInfos, ceilingInfos);
                diffuserService.SetDiffusersElevation(diffuserInfos);

                using (var t = new Transaction(doc, "Update diffusers elevation"))
                {
                    t.Start();

                    foreach (var info in diffuserInfos)
                    {
                        if (!info.WillBeChanged)
                            continue;

                        var element = doc.GetElement(info.Id);
                        if (element == null)
                            continue;

                        var param = element.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                        if (param != null && !param.IsReadOnly)
                        {
                            param.Set(info.Elevation);
                        }
                    }

                    t.Commit();
                }
                return Result.Succeeded;

            }
            catch (Exception ex)
            {
                // Показываем ошибку пользователю
                TaskDialog.Show("Ошибка", ex.Message);

                // Возвращаем ошибку Revit
                message = ex.ToString();
                return Result.Failed;
            }
        }
    }
}
