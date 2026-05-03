using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RevitTools.Core.Services
{
    public class DiffuserService
    {
        private readonly Document _doc;
        private readonly MepConnectivityService _connectivity;

        private readonly EquipmentIdentifier _identifier;

        public DiffuserService(Document doc, MepConnectivityService connectivity, EquipmentIdentifier identifier)
        {
            _doc = doc;
            _connectivity = connectivity;
            _identifier = identifier;

        }

        public List<DiffuserInfo> CreateDiffuserInfoList(List<FamilyInstance> diffusers)
        {
            List<DiffuserInfo> diffuserInfoList = new List<DiffuserInfo>();
            foreach (var diffuser in diffusers)
            {
                var info = CreateDiffuserInfo(diffuser);
                if (info != null)
                    diffuserInfoList.Add(info);
            }

            return diffuserInfoList;

        }

        public List<FamilyInstance> GetDiffusers()
        {

            var collector = new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_DuctTerminal)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();

            return collector;

        }

        public FamilyInstance GetDiffuser(ElementId id)
        {
            return _doc.GetElement(id) as FamilyInstance;
        }


        public DiffuserInfo CreateDiffuserInfo(FamilyInstance diffuser)
        {
            if (diffuser == null)
                return null;

            BoundingBoxXYZ box = diffuser.get_BoundingBox(null);
            if (box == null)
                return null;

            // Получаем уровень
            ElementId levelId = diffuser.LevelId;

            if (levelId == null || levelId == ElementId.InvalidElementId)
            {
                levelId = diffuser.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)?.AsElementId();
            }

            double levelZ = 0;
            var level = _doc.GetElement(levelId) as Level;
            if (level != null)
                levelZ = level.Elevation;

            return new DiffuserInfo(diffuser.Id, box, levelId, levelZ);
        }

        public void ExpandBoundingBoxes(List<DiffuserInfo> diffusers, double verticalOffsetFeet = 3.28084 )
        {
            if (diffusers == null)
                return;

            // 500 mm / 2
            double halfXY =
                UnitUtils.ConvertToInternalUnits(
                    250.0,
                    DisplayUnitType.DUT_MILLIMETERS
                );

            foreach (var info in diffusers)
            {
                if (info?.Box == null)
                    continue;

                var min = info.Box.Min;
                var max = info.Box.Max;

                // 🔹 Центр бокса
                double centerX = (min.X + max.X) / 2.0;
                double centerY = (min.Y + max.Y) / 2.0;

                // 🔹 Новый bounding box
                info.Box.Min = new XYZ(
                    centerX - halfXY,
                    centerY - halfXY,
                    min.Z - verticalOffsetFeet
                );

                info.Box.Max = new XYZ(
                    centerX + halfXY,
                    centerY + halfXY,
                    max.Z + verticalOffsetFeet
                );
            }
        }

        public void SetDiffusersElevation(List<DiffuserInfo> diffusers)
        {
            if (diffusers == null)
                return;
            foreach (var info in diffusers)
            {
                if (info.CeilingInfos.Count != 1)
                    continue;
                info.Elevation = info.CeilingInfos[0].Elevation - info.LevelZ;
                info.WillBeChanged = true;
            }
        }


        public bool IsConnectedToFlex(FamilyInstance diffuser)
        {
            var mepModel = diffuser.MEPModel;
            if (mepModel == null)
                return false;

            ConnectorSet connectors = mepModel.ConnectorManager.Connectors;

            // Должен быть ровно один коннектор
            if (connectors.Size != 1)
                return false;

            Connector connector = connectors.Cast<Connector>().First();

            // Проверяем цепочку до глубины 2
            return 
            _connectivity.IsConnectedRecursive(
                        connector, 0, 2, el => el is FlexDuct);

        }

        
        public bool IsBalancingDeviceMyTry(FamilyInstance diffuser)
        {
            var mepModel = diffuser.MEPModel;
            if (mepModel == null)
                return false;
            double flow = 0;
            bool hasDumper = false;
            TryGetDiffuserAirFlow(diffuser, out  flow);

            ConnectorSet connectors = mepModel.ConnectorManager.Connectors;

            // Должен быть ровно один коннектор
            if (connectors.Size != 1)
                return false;

            Connector connector = connectors.Cast<Connector>().First();

            _connectivity.IsConnectedRecursive(
                       connector, 0, 10, el => IsHasADumper(el, flow, out hasDumper));
            // Проверяем цепочку до глубины 10
            return
                hasDumper;
           
        }

        public bool IsBalancingDevice(FamilyInstance diffuser)
        {
            var mepModel = diffuser.MEPModel;
            if (mepModel == null)
                return false;
            //LoggingService.Log($"Проверяем диффузор {diffuser.Id}");

            if (!TryGetDiffuserAirFlow(diffuser, out double diffuserFlowM3h))
                return false;

            var connectors = mepModel.ConnectorManager.Connectors;
            if (connectors.Size != 1)
                return false;

            var connector = connectors.Cast<Connector>().First();

            var result = _connectivity.IsConnectedRecursive(
                connector,
                0,
                10,
                el => CheckBalancingPathElement(el, diffuserFlowM3h)
            );
            if (result == ConnectivityCheckResult.Success)
            {
                //LoggingService.Log($"Проверка- {diffuser.Id} нашла балансировочный клапан");
                return true;
            }
            else
            {
               // LoggingService.Log($"Проверка- {diffuser.Id} не нашла балансировочный клапан");
                return false;
            }   
        }




        public bool TryGetDiffuserAirFlow(
            FamilyInstance diffuser,
            out double flowM3h
        )
        {
            flowM3h = 0;

            if (diffuser?.MEPModel?.ConnectorManager == null)
                return false;

            var conn = diffuser.MEPModel.ConnectorManager.Connectors
                .Cast<Connector>()
                .FirstOrDefault(c =>
                    c.Domain == Domain.DomainHvac &&
                    Math.Abs(c.Flow) > 1e-9
                );

            if (conn == null)
                return false;

            double flowInternal = conn.Flow; // ft³/s

            if (flowInternal <= 0)
                return false;

            // ft³/s → m³/h
            double flowM3s =
                UnitUtils.ConvertFromInternalUnits(
                    flowInternal,
                    DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND
                );

            flowM3h = flowM3s * 3600.0;

            return true;
        }


        private bool IsConnectedToFlexRecursive(Connector connector, int depth)
        {
            // Ограничиваем глубину рекурсии
            if (depth > 1)
                return false;

            foreach (Connector refConn in connector.AllRefs)
            {
                Element owner = refConn.Owner;

                // Если это FlexDuct — успех
                if (owner is FlexDuct)
                    return true;

                // Если это Duct или переходник — продолжаем искать дальше
                if (owner is MEPCurve || owner is FamilyInstance)
                {
                    // Берём все коннекторы этого элемента
                    var nextConnectors = GetConnectors(owner);

                    foreach (var next in nextConnectors)
                    {
                        // Не возвращаемся назад по цепочке
                        if (next.Id != refConn.Id)
                        {
                            if (IsConnectedToFlexRecursive(next, depth + 1))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private IEnumerable<Connector> GetConnectors(Element element)
        {
            if (element is MEPCurve curve)
                return curve.ConnectorManager.Connectors.Cast<Connector>();

            if (element is FamilyInstance fi && fi.MEPModel != null)
                return fi.MEPModel.ConnectorManager.Connectors.Cast<Connector>();

            return Enumerable.Empty<Connector>();
        }

        public List<FamilyInstance> GetDiffusersWithFlex(List<FamilyInstance> allDiffusers)
        {

            List<FamilyInstance> filterdDiffusers = new List<FamilyInstance>();
            foreach (var diffuser in allDiffusers)
            {
                if (IsConnectedToFlex(diffuser))
                {
                    filterdDiffusers.Add(diffuser);
                }
            }

            return filterdDiffusers;

        }

        public bool IsHasADumper (Element el, double flow, out bool hasDamper)
        {
            hasDamper = false;
            if (el is Duct duct)
            {
                var shape = _connectivity.GetDuctShape(duct);

                // ❌ Прямоугольный воздуховод недопустим
                if (_connectivity.GetDuctShape(duct) != 0)
                    return false;

                // ✅ Круглый воздуховод → проверяем расход

                if (_connectivity.TryGetAirFlow(duct, out double ductFlowM3h))
                {
                    const double tolerance = 1.0; // м³/ч

                    if (Math.Abs(ductFlowM3h - flow) > tolerance)
                        return false;
                }
                return true;
            }
            // --- 3️⃣ Воздухораспределитель / Mechanical Equipment ---
            if (el is FamilyInstance fi)
            {
                //  Mechanical Equipment ---
                if (fi.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment)
                    return false;
                // --- 3️⃣ Воздухораспределитель  ---
                if (fi.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal)
                {
                    return false;
                }

                // Балансировочный клапан или другая арматура воздуховодов ---
                if (fi.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory)
                {
                    var type = _doc.GetElement(el.GetTypeId()) as Element;
                    string code = "";
                    if (type != null)
                    {

                        var modelParam = type.LookupParameter("MC Product Code");
                        code = modelParam?.AsString() ?? "";
                    }

                    // --- 2️⃣ Балансировочный клапан ---
                    if (_identifier.IsBalancingDamper(code))
                    {
                        hasDamper = true;
                        return false;
                    }
                    // --- 4️⃣ Всё остальное ---
                    else
                    {
                        return true;
                    }
                }    
                    return true;
            }





            return true;

        }

        public ConnectivityCheckResult CheckBalancingPathElement(
            Element element,
            double diffuserFlowM3h
        )
        {
            //LoggingService.Log($"Проверяем элемент {element.Id}");
            // --- 1️⃣ Воздуховод ---
            if (element is Duct duct)            {
                
                int shape = _connectivity.GetDuctShape(duct);

                // ❌ Прямоугольный воздуховод недопустим
                if (shape != 0) // 0 = круглый
                {
                    //LoggingService.Log($"Элемент - {element.Id} не круглый воздуховод");
                    return ConnectivityCheckResult.Fail;
                }

                // Проверка расхода
                if (_connectivity.TryGetAirFlow(duct, out double ductFlowM3h))
                {
                    //LoggingService.Log($"Элемент - {element.Id} круглый воздуховод");
                    if (Math.Abs(ductFlowM3h - diffuserFlowM3h) > 1.0)
                    {
                        //LoggingService.Log($"Элемент - {element.Id} круглый воздуховод с отличным расходом");
                        return ConnectivityCheckResult.Fail;
                    }
                }
                //LoggingService.Log($"Элемент - {element.Id} круглый воздуховод - продолжаем проверку");
                return ConnectivityCheckResult.Continue;
            }

            // --- 2️⃣ Получаем код изделия ---
            string code = _doc.GetElement(element.GetTypeId())?
                .LookupParameter("MC Product Code")?
                .AsString() ?? "";

            // --- 3️⃣ Балансировочный клапан ---
            if (_identifier.IsBalancingDamper(code))
            {
                //LoggingService.Log($"Элемент - {element.Id} балансировочный клапан");
                return ConnectivityCheckResult.Success;
            }
            // --- 4️⃣ Воздухораспределитель ---
            if (element is FamilyInstance fi &&
                fi.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal)
            {
                //LoggingService.Log($"Элемент - {element.Id} воздухораспределитель");
                return ConnectivityCheckResult.Fail;
            }
            // --- 5️⃣ Mechanical Equipment ---
            if (element is FamilyInstance fi2 &&
                fi2.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment)
            {
                //LoggingService.Log($"Элемент - {element.Id} механическое оборудование");
                return ConnectivityCheckResult.Fail;
            }
            // --- 6️⃣ DuctAccessory, но НЕ балансировочный клапан ---
            if (element is FamilyInstance fi3 &&
                fi3.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory)
            {
                //LoggingService.Log($"Элемент - {element.Id} любой другой элемент арматуры воздуховода");
                // Противопожарные клапаны, датчики, переходники — всё это OK
                // Просто продолжаем поиск
                return ConnectivityCheckResult.Continue;
            }
            //LoggingService.Log($"Элемент - {element.Id} любой другой элемент системы");
            // --- 7️⃣ Всё остальное ---
            return ConnectivityCheckResult.Continue;
        }



        public enum ConnectivityCheckResult
        {
            Continue,   // идём дальше по системе
            Success,    // нашли корректный балансировочный клапан
            Fail        // путь недопустим
        }


    }
}