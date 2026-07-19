using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools.Core.Models
{
    public class CirclDuctsList
    {
        public ElementId Id { get; set; } 
        public bool IsChanged { get; set; } = false;

        public List<ElementId> CeilingIds { get; set; } = new List<ElementId>();
        
    }

}
