using RevitTools.Core.Config;
using System.IO;
using Newtonsoft.Json;

public class ConfigService
{
    private readonly string _path;

    public ConfigService(string path)
    {
        _path = path;
    }

    public EquipmentCatalog Load()
    {
        var json = File.ReadAllText(_path);
        return JsonConvert.DeserializeObject<EquipmentCatalog>(json);
    }
}