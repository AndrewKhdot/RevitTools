using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using RevitTools.Core.Services;
using RevitTools.Revit.Services;
using RevitTools.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RevitTools.DiffuserDamperAssistant
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class IsDiffuserHasARegulatorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<string> sizes = new List<string>() { "100", "125", "160", "200", "250"};
            try
            {

                LogWindowManager.Show();

                //Подготовка всех сервисов и данных
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;

                string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string jsonPath = Path.Combine(pluginFolder, "Config", "EquipmentCatalog.json");
                var config = new ConfigService(jsonPath).Load();
                var identifier = new EquipmentIdentifier(config);
                var collector = new DuctAccessoryCollectorService(doc);
                var filtering = new FilteringAccessoryService(doc, identifier);

                var allFSymbols = collector.GetDuctAccessoryFamilySymbols();
                var balancingDampers = filtering.FilterBalancingDamperSymbols(allFSymbols);
                var infoService = new DuctAccessoryInfoService(doc, identifier);
                var connectivityService = new MepConnectivityService();
                var diffuserService = new DiffuserService(doc, connectivityService, identifier);
                var diffusers = diffuserService.GetDiffusers();
                var diffuserInfos = diffuserService.CreateDiffuserInfoList(diffusers);
                var spaceLookupService = new SpaceLookupService(doc);
                var ductService = new DuctService(doc);
                var damperPlacementService = new DamperPlacementService(doc);


                // Проверяем наличие семейств в проекте

                bool ok = true;

                    // Собираем размеры всех найденных регуляторов
                    var availableSizes = new HashSet<string>();
                    var damperBySize = new Dictionary<string, FamilySymbol>();

                    foreach (var symbol in balancingDampers)
                    {
                        string size = infoService.GetAccessoryConSize(symbol); // твой метод
                        if (!string.IsNullOrEmpty(size))
                        {
                            availableSizes.Add(size);
                            if (!damperBySize.ContainsKey(size))
                                damperBySize[size] = symbol;
                        }
                    }

                    // Проверяем, что каждый обязательный размер присутствует
                    foreach (var req in sizes)
                    {
                        if (!availableSizes.Contains(req))
                        {
                            ok = false;
                            break;// хотя бы одного размера нет → провал
                        }
                    }
                if (!ok)
                {
                    TaskDialog.Show("Regulatory", "Brakuje wymaganych rozmiarów regulatorów.");
                    message = "Brakuje wymaganych rozmiarów regulatorów.";
                    return Result.Failed;
                }


                //Запускаем сборку и проверку диффузоров
                foreach (var info in diffuserInfos)
                {
                    var diffuser = diffuserService.GetDiffuser(info.Id);
                    info.WillBeChanged = diffuserService.IsBalancingDevice(info);
                    Space space = spaceLookupService.GetSpaceFor(diffuser);
                    if (space != null)
                        info.SpaceName = space.Name;
                }

                //Выбираем этаж или этажи для установки регуляторов

                //Находим воздуоводы, на котороые можно установить регуляторы



                foreach (var info in diffuserInfos)
                {
                    if (!info.WillBeChanged)
                    {
                        var ductsId = info.DuctsForDampers;
                        List<Duct> ducts = ductService.GetDucts(ductsId);
                        ducts = ducts
                         .Where(d =>
                         {
                             double lengthMm = UnitUtils.ConvertFromInternalUnits(
                                 d.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble(),
                                 DisplayUnitType.DUT_MILLIMETERS);

                             return lengthMm >= 150;
                         })
                         .ToList();

                        HashSet<ElementId> ductIds = ducts
                        .Select(d => d.Id)
                        .ToHashSet();
                        info.DuctsForDampers = ductIds;
                    }
                }

                //Ставим регуляторы на выбранные воздуховоды
                int placedCount = 0;

                using (var t = new Transaction(doc, "Place balancing dampers"))
                {
                    t.Start();

                    foreach (var info in diffuserInfos)
                    {
                        if (info.WillBeChanged)
                            continue;

                        foreach (var duct in ductService.GetDucts(info.DuctsForDampers))
                        {
                            double diameterMm = UnitUtils.ConvertFromInternalUnits(
                                duct.Diameter,
                                DisplayUnitType.DUT_MILLIMETERS);
                            string size = $"{(int)Math.Round(diameterMm, MidpointRounding.AwayFromZero)}";

                            if (!damperBySize.TryGetValue(size, out FamilySymbol damperSymbol))
                                continue;

                            // Один регулятор на воздухораспределитель
                            if (damperPlacementService.TryPlaceDamper(duct, damperSymbol))
                            {
                                info.WillBeChanged = true;
                                placedCount++;
                                break;
                            }
                        }
                    }

                    t.Commit();
                }

                LoggingService.Log($"Установлено регуляторов: {placedCount}");


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
