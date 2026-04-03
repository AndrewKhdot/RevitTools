using Autodesk.Revit.DB;
using RevitTools.Core.Models;
using System.Collections.Generic;

namespace RevitTools.Core.Services
{
    public class IntersectionService
    {
        public bool Intersects(BoundingBoxXYZ a, BoundingBoxXYZ b)
        {
            if (a == null || b == null)
                return false;

            bool x = a.Max.X >= b.Min.X && a.Min.X <= b.Max.X;
            bool y = a.Max.Y >= b.Min.Y && a.Min.Y <= b.Max.Y;
            bool z = a.Max.Z >= b.Min.Z && a.Min.Z <= b.Max.Z;

            return x && y && z;
        }

        public void FindIntersections(
            List<DiffuserInfo> diffusers,
            List<CeilingInfo> ceilings)
        {
            foreach (var diffuser in diffusers)
            {
                diffuser.CeilingInfos.Clear();

                foreach (var ceiling in ceilings)
                {
                    if (Intersects(diffuser.Box, ceiling.Box))
                    {
                        diffuser.CeilingInfos.Add(ceiling);
                    }
                }
            }
        }
    }
}
