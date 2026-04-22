

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
            int currentDepth,
            int maxDepth,
            Func<Element, bool> match
        )
        {
            if (currentDepth > maxDepth)
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
                            if (IsConnectedRecursive(
                                    next,
                                    currentDepth + 1,
                                    maxDepth,
                                    match))
                                return true;
                        }
                    }
                }
            }

            return false;
        }


        public bool IsConnectedRecursive(
            Connector connector,
            int currentDepth,
            int maxDepth,
            Func<Element, double, bool> match,
            double contextValue
        )
        {

            if (currentDepth > maxDepth)
                return false;

            foreach (Connector refConn in connector.AllRefs)
            {
                Element owner = refConn.Owner;
                if (owner == null)
                    continue;

                if (match(owner, contextValue))
                    return true;

                if (owner is MEPCurve || owner is FamilyInstance)
                {

                    foreach (var next in GetConnectors(owner))
                    {
                        if (next.Id != refConn.Id)
                        {
                            if (IsConnectedRecursive(
                                    next,
                                    currentDepth + 1,
                                    maxDepth,
                                    match,
                                    contextValue))
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

        
        public int GetDuctShape(Element el)
        {
            if (!(el is Duct duct))
                return -1;

            var connectors = duct.ConnectorManager.Connectors
                .Cast<Connector>()
                .Where(c => c.Domain == Domain.DomainHvac);

            if (!connectors.Any())
                return -1;

            // Берём первый HVAC‑коннектор
            var c0 = connectors.First();

            if (c0.Shape == ConnectorProfileType.Round)
                return 0;

            if (c0.Shape == ConnectorProfileType.Rectangular)
                return 1;

            return -1;
        }

        public bool TryGetAirFlow(
            Element el,
            out double flowM3h
        )
        {
            flowM3h = 0;

            
        if (!(el is Duct duct))
                return false;

        Parameter p = duct.get_Parameter(
                BuiltInParameter.RBS_DUCT_FLOW_PARAM
            );

            if (p == null || !p.HasValue)
                return false;

            double flowInternal = p.AsDouble(); // ft³/s

            // Перевод в м³/ч
            double flowM3s =
                UnitUtils.ConvertFromInternalUnits(
                    flowInternal,
                    DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND
                );

            flowM3h = flowM3s * 3600.0;

            return true;
        }

    }
}