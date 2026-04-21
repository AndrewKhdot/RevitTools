

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
    public class MepConnectivityService
    {
        public bool IsConnectedRecursive(
            Connector connector,
            int depth,
            Func<Element, bool> match
        )
        {
            if (depth > 1)
                return false;

            foreach (Connector refConn in connector.AllRefs)
            {
                Element owner = refConn.Owner;
                if (owner == null)
                    continue;

                if (match(owner))
                    return true;

                if (owner is MEPCurve || owner is FamilyInstance)
                {
                    foreach (var next in GetConnectors(owner))
                    {
                        if (next.Id != refConn.Id)
                        {
                            if (IsConnectedRecursive(next, depth + 1, match))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

       private IEnumerable<Connector> GetConnectors(Element el)
        {
            if (el is MEPCurve curve)
                return curve.ConnectorManager.Connectors.Cast<Connector>();

            if (el is FamilyInstance fi && fi.MEPModel != null)
                return fi.MEPModel.ConnectorManager.Connectors.Cast<Connector>();

            return Enumerable.Empty<Connector>();
        }
    }
}