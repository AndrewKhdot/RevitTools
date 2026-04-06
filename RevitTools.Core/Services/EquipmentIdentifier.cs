using RevitTools.Core.Config;
using System.Collections.Generic;
using System.Linq;

public class EquipmentIdentifier
{
    private readonly EquipmentCatalog _cat;

    public EquipmentIdentifier(EquipmentCatalog cat)
    {
        _cat = cat;
    }


    public bool IsFireDamper(string code)
        { 
            List<string> FireDamper = new List<string>();
            FireDamper.AddRange(_cat.FireDampers.Circle);
            FireDamper.AddRange(_cat.FireDampers.Rect);
            FireDamper.AddRange(_cat.FireDampers.Other);
            return FireDamper.Any(c => code.StartsWith(c));
        }

    public bool IsBalancingDamper(string code)
    {
        List<string> FireDamper = new List<string>();
        FireDamper.AddRange(_cat.BalancingDampers.Circle);
        FireDamper.AddRange(_cat.BalancingDampers.Rect);
        FireDamper.AddRange(_cat.BalancingDampers.Other);
        return FireDamper.Any(c => code.StartsWith(c));
    }

    public bool IsSilencer(string code)
        {
            List<string> Silencer = new List<string>();
            Silencer.AddRange(_cat.SoundAttenuators.Circle);
            Silencer.AddRange(_cat.SoundAttenuators.Rect);
            Silencer.AddRange(_cat.SoundAttenuators.Other);
            return Silencer.Any(c => code.StartsWith(c));
        }

    public bool IsCircle(string code)
        {
          List<string> Circle = new List<string>();
            Circle.AddRange(_cat.FireDampers.Circle);
            Circle.AddRange(_cat.SoundAttenuators.Circle);
            return Circle.Any(c => code.StartsWith(c));
        }

    public bool IsRect(string code)
        {
            List<string> Rect = new List<string>();
            Rect.AddRange(_cat.FireDampers.Rect);
            Rect.AddRange(_cat.SoundAttenuators.Rect);
            return Rect.Any(c => code.StartsWith(c));
        }
}