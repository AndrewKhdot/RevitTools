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
            if (levelId == null || levelId == ElementId.InvalidElementId)
            {
                levelId = diffuser.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)?.AsElementId();
            }

            return new DiffuserInfo(diffuser.Id, box, levelId);
        }

        public bool IsConnectedToFlex(FamilyInstance diffuser)
        {
            var mepModel = diffuser.MEPModel;
            if (mepModel == null)
                return false;
            ConnectorSet connectors = mepModel.ConnectorManager.Connectors;

            if(connectors.Size != 1)
                return false;

            foreach (Connector connector in connectors)
            {
                foreach (Connector refConn in connector.AllRefs)
                {
                    var owner = refConn.Owner;
                    if (owner is FlexDuct)
                        return true;
                }
            }

            return false;
        }

    }
}