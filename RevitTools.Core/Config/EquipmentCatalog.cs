using System.Collections.Generic;

namespace RevitTools.Core.Config
{
    public class EquipmentCatalog
    {       
    public EquipmentGroup FireDampers { get; set; }
    public EquipmentGroup SoundAttenuators { get; set; }

    }

   
    public class EquipmentGroup
    {
        public List<string> Circle { get; set; } = new();
        public List<string> Rect { get; set; } = new();
        public List<string> Other { get; set; } = new();
    }

}