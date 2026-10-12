using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools.Core.Services
{
    public class DamperPlacementService
    {
        // Минимальный остаток воздуховода с каждой стороны регулятора
        private const double MinStubMm = 10;

        private readonly Document _doc;

        public DamperPlacementService(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Пытается врезать регулятор в середину воздуховода.
        /// Должен вызываться внутри открытой транзакции.
        /// При неудаче все изменения этой попытки откатываются.
        /// </summary>
        public bool TryPlaceDamper(Duct duct, FamilySymbol symbol)
        {
            using (var st = new SubTransaction(_doc))
            {
                st.Start();
                try
                {
                    if (PlaceDamper(duct, symbol))
                    {
                        st.Commit();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log($"Не удалось поставить регулятор на воздуховод {duct.Id}: {ex.Message}");
                }

                st.RollBack();
                return false;
            }
        }

        private bool PlaceDamper(Duct duct, FamilySymbol symbol)
        {
            var line = (duct.Location as LocationCurve)?.Curve as Line;
            if (line == null)
                return false;

            XYZ ductDir = line.Direction;
            XYZ mid = (line.GetEndPoint(0) + line.GetEndPoint(1)) * 0.5;
            double ductLength = line.Length;

            if (!symbol.IsActive)
            {
                symbol.Activate();
                _doc.Regenerate();
            }

            var damper = _doc.Create.NewFamilyInstance(
                mid, symbol, duct.ReferenceLevel, StructuralType.NonStructural);
            _doc.Regenerate();

            var damperConnectors = GetHvacConnectors(damper);
            if (damperConnectors.Count != 2)
                return false;

            // Разворачиваем регулятор вдоль оси воздуховода
            XYZ damperDir = (damperConnectors[1].Origin - damperConnectors[0].Origin).Normalize();
            XYZ axis = damperDir.CrossProduct(ductDir);
            if (axis.GetLength() > 1e-9)
            {
                ElementTransformUtils.RotateElement(
                    _doc,
                    damper.Id,
                    Line.CreateUnbound(mid, axis),
                    damperDir.AngleTo(ductDir));
                _doc.Regenerate();
            }

            // Совмещаем центр регулятора с серединой воздуховода
            damperConnectors = GetHvacConnectors(damper);
            XYZ center = (damperConnectors[0].Origin + damperConnectors[1].Origin) * 0.5;
            if (!center.IsAlmostEqualTo(mid))
            {
                ElementTransformUtils.MoveElement(_doc, damper.Id, mid - center);
                _doc.Regenerate();
                damperConnectors = GetHvacConnectors(damper);
            }

            // Регулятор должен помещаться на воздуховоде
            double damperLength = damperConnectors[0].Origin.DistanceTo(damperConnectors[1].Origin);
            double minStub = UnitUtils.ConvertToInternalUnits(MinStubMm, DisplayUnitType.DUT_MILLIMETERS);
            if (damperLength + 2 * minStub > ductLength)
                return false;

            // Разрезаем воздуховод и подтягиваем обе части к коннекторам регулятора
            ElementId newDuctId = MechanicalUtils.BreakCurve(_doc, duct.Id, mid);
            var newDuct = _doc.GetElement(newDuctId) as Duct;
            if (newDuct == null)
                return false;
            _doc.Regenerate();

            return AttachDuct(duct, mid, damperConnectors)
                && AttachDuct(newDuct, mid, damperConnectors);
        }

        private bool AttachDuct(Duct duct, XYZ breakPoint, List<Connector> damperConnectors)
        {
            var location = duct.Location as LocationCurve;
            var line = location?.Curve as Line;
            if (line == null)
                return false;

            XYZ p0 = line.GetEndPoint(0);
            XYZ p1 = line.GetEndPoint(1);

            // Конец, оставшийся после разреза у регулятора, и дальний конец
            bool innerIsStart = p0.DistanceTo(breakPoint) < p1.DistanceTo(breakPoint);
            XYZ outer = innerIsStart ? p1 : p0;

            // Коннектор регулятора со стороны этой части воздуховода
            Connector damperConnector = damperConnectors
                .OrderBy(c => c.Origin.DistanceTo(outer))
                .First();
            if (damperConnector.IsConnected)
                return false;

            location.Curve = innerIsStart
                ? Line.CreateBound(damperConnector.Origin, outer)
                : Line.CreateBound(outer, damperConnector.Origin);
            _doc.Regenerate();

            Connector ductConnector = duct.ConnectorManager.Connectors
                .Cast<Connector>()
                .Where(c => c.ConnectorType == ConnectorType.End)
                .OrderBy(c => c.Origin.DistanceTo(damperConnector.Origin))
                .FirstOrDefault();
            if (ductConnector == null)
                return false;

            damperConnector.ConnectTo(ductConnector);
            return true;
        }

        private List<Connector> GetHvacConnectors(FamilyInstance instance)
        {
            var manager = instance.MEPModel?.ConnectorManager;
            if (manager == null)
                return new List<Connector>();

            return manager.Connectors
                .Cast<Connector>()
                .Where(c => c.Domain == Domain.DomainHvac)
                .ToList();
        }
    }
}
