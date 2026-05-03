using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RevitTools.CeilingDiffuserElevation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class IsDiffuserHasARegulatorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                LogWindowManager.Show();

                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;

                string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string jsonPath = Path.Combine(pluginFolder, "Config", "EquipmentCatalog.json");
                var config = new ConfigService(jsonPath).Load();
                var identifier = new EquipmentIdentifier(config);
                var connectivityService = new MepConnectivityService();
                var diffuserService = new DiffuserService(doc, connectivityService, identifier);
                var diffusers = diffuserService.GetDiffusers();
                var diffuserInfos = diffuserService.CreateDiffuserInfoList(diffusers);
                var spaceLookupService = new SpaceLookupService(doc);
                foreach (var info in diffuserInfos)
                {
                    var diffuser = diffuserService.GetDiffuser(info.Id);
                    info.WillBeChanged = diffuserService.IsBalancingDevice(diffuser);
                    Space space = spaceLookupService.GetSpaceFor(diffuser);
                    if (space != null)
                        info.SpaceName = space.Name;
                }
                string needBalancerDumper = "";

                foreach (var item in diffuserInfos)
                {
                    if (!item.WillBeChanged)
                    {
                        needBalancerDumper = $"{needBalancerDumper}id- {item.Id} - {item.SpaceName}{Environment.NewLine}";
                    }                   
                }


                
                LoggingService.Log(needBalancerDumper);
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
