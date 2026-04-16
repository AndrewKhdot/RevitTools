using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.CeilingDiffuserElevation
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
                var ids = ElementIdLoaderHelper.LoadFromTxt(@"E:\makro\RevitTools\RevitTools.Core\Config\ids.txt");
                var highlightService = new HighlightService(doc, ids);         

                var diffuserService = new DiffuserService(doc);
                var linkedService = new LinkService(doc);
                var linkedCeilingService = new LinkedCeilingService(doc, linkedService);
                var intersactionService = new IntersectionService();

                var allDiffusers = diffuserService.GetDiffusers();
                var diffusers = diffuserService.GetDiffusersWithFlex(allDiffusers);
                var diffuserInfos = diffuserService.CreateDiffuserInfoList(diffusers);

                    using (var t = new Transaction(doc, "Show diffusers"))
                {
                    t.Start();
                    foreach(var di in diffuserInfos)
                    {
                        highlightService.DemoBoundingBoxStep(di.Id, di.Box, $"diffuserId - {di.Id.ToString()}");
                    }  
                    t.Commit();
                }                  

                diffuserService.ExpandBoundingBoxes(diffuserInfos);
                    using (var t = new Transaction(doc, "Show expanded diffusers"))
                {
                    t.Start();
                    foreach(var di in diffuserInfos)
                    {
                        highlightService.DemoBoundingBoxStep(di.Id, di.Box, $"diffuserId - {di.Id.ToString()}");
                    }
                    t.Commit();
                }                  

                var ceilingInfos = linkedCeilingService.GetLinkedCeilings();
                using (var t = new Transaction(doc, "Show ceilings"))
                {
                    t.Start();
                    foreach(var ci in ceilingInfos)
                    {
                        highlightService.DemoBoundingBoxStep(ci.Id, ci.Box, $"ceilingId - {ci.Id.ToString()}");
                    }
                   t.Commit();
                }
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
