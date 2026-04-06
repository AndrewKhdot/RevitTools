using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using RevitTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.Core.Services
{
    public class DiffuserService
    {
        private readonly Document _doc;

        public DiffuserService(Document doc)
        {
            _doc = doc;
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
            double elevation = 0;
            if (levelId == null || levelId == ElementId.InvalidElementId)
            {
                levelId = diffuser.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)?.AsElementId();
                var level = _doc.GetElement(levelId) as Level;
                if (level != null)
                    elevation = level.Elevation;
            }

            return new DiffuserInfo(diffuser.Id, box, levelId, elevation);
        }

        public void ExpandBoundingBoxes(List<DiffuserInfo> diffusers, double offsetFeet = 3.28084)
        {
            if (diffusers == null)
                return;

            foreach (var info in diffusers)
            {
                if (info?.Box == null)
                    continue;

                var min = info.Box.Min;
                var max = info.Box.Max;

                info.Box.Min = new XYZ(min.X, min.Y, min.Z - offsetFeet);
                info.Box.Max = new XYZ(max.X, max.Y, max.Z + offsetFeet);
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
                info.Elevation = info.CeilingInfos[0].Elevation;
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
            return IsConnectedToFlexRecursive(connector, 0);
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

    }
}