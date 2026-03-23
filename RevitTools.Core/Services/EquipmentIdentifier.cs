public class EquipmentIdentifier
{
    private readonly EquipmentCatalog _cat;

    public EquipmentIdentifier(EquipmentCatalog cat)
    {
        _cat = cat;
    }

    public bool IsCircleFireDamper(string code)
        => _cat.FireDampers.Circle.Any(c => code.StartsWith(c));

    public bool IsRectFireDamper(string code)
        => _cat.FireDampers.Rect.Any(c => code.StartsWith(c));

    public bool IsCircleSilencer(string code)
        => _cat.SoundAttenuators.Circle.Any(c => code.StartsWith(c));

    public bool IsRectSilencer(string code)
        => _cat.SoundAttenuators.Rect.Any(c => code.StartsWith(c));
}