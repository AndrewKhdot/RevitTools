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
                //LogWindowManager.Show();                             
                
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;


                // Создаём сервисы     
                var ids = ElementIdLoaderHelper.LoadFromTxt(@"C:\Users\Khadatchuk\Downloads\ids.txt");
                // var highlightService = new HighlightService(doc, ids);       
                // highlightService.Pause();
                // LoggingService.Log("Plugin execution started.");
                // highlightService.Pause();
                var diffuserService = new DiffuserService(doc);
                var linkedService = new LinkService(doc);
                var linkedCeilingService = new LinkedCeilingService(doc, linkedService);
                var intersactionService = new IntersectionService();

                var allDiffusers = diffuserService.GetDiffusers();
                var diffusers = diffuserService.GetDiffusersWithFlex(allDiffusers);
                var diffuserInfos = diffuserService.CreateDiffuserInfoList(diffusers);
                
                // LoggingService.Log("Searching for air terminals connected via flexible ducts.");

                // foreach(var di in diffuserInfos)
                // {

                //     highlightService.DemoBoundingBoxStep(di.Id, di.Box, $"diffuserId - {di.Id.ToString()}");
                // }  
 
                diffuserService.ExpandBoundingBoxes(diffuserInfos);

                // LoggingService.Log("Expanding air terminal bounding boxes.");

                // foreach(var di in diffuserInfos)
                // {
                //     highlightService.DemoBoundingBoxStep(di.Id, di.Box, $"diffuserId - {di.Id.ToString()}");
                // }
              

                var ceilingInfos = linkedCeilingService.GetLinkedCeilings();

                // LoggingService.Log("Searching for suspended ceilings.");

                // foreach(var ci in ceilingInfos)
                // {
                //     highlightService.DemoBoundingBoxStep(ci.Id, ci.Box, $"ceilingId - {ci.Id.ToString()}");
                // }
                // LoggingService.Log("Detecting intersections and assigning ceilings to air terminals.");
                // highlightService.Pause();
                intersactionService.FindIntersections(diffuserInfos, ceilingInfos);
                diffuserService.SetDiffusersElevation(diffuserInfos);
                // LoggingService.Log("Setting air terminal elevations based on ceiling heights.");
                // highlightService.Pause();
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
                // LoggingService.Log("Plugin execution completed.");
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
