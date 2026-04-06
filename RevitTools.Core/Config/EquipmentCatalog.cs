using System.Collections.Generic;

namespace RevitTools.Core.Config
{
    public class EquipmentCatalog
    {       
    public EquipmentGroup FireDampers { get; set; }
    public EquipmentGroup SoundAttenuators { get; set; }
    public EquipmentGroup BalancingDampers { get; set; }

    }

   
    public class EquipmentGroup
    {
        public List<string> Circle { get; set; } = new List<string>();
        public List<string> Rect { get; set; } = new List<string>();
        public List<string> Other { get; set; } = new List<string>();
    }

}