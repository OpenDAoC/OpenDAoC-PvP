using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.Scripts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DOL.GS;

public static class DungeonHelper
{
    private static List<DungeonItem> _availableItems;
    
    static DungeonHelper()
    {
        _availableItems = InitializeList();
    }

    public static ItemTemplate GetItemForZone(int zoneId)
    {
        return _availableItems.FirstOrDefault(x => x.ZoneID == zoneId)?.Template;
    }

    private static List<DungeonItem> InitializeList()
    {
         var templates = GameServer.Database.SelectAllObjects<ItemTemplate>().Where(x=> x.PackageID.Contains("TitanXP")).ToDictionary(k => k.Id_nb.ToLower());
         List<DungeonItem> items = new List<DungeonItem>();
         
        foreach (var itemTemplate in templates)
        {
            switch (itemTemplate.Key)
            {
                case "avaloncitymedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 40, 50, 50));
                    break;
                case "catacombsmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 25, 35, 23));
                    break;
                case "coruscatingmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 30, 45, 220));
                    break;
                case "cursedmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 15, 22, 128));
                    break;
                case "fomormedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 40, 50, 180));
                    break;
                case "keltoimedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 15, 30, 22));
                    break;
                case "koalinthmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 20, 30, 223));
                    break;
                case "iarnvidiurmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 40, 50, 161));
                    break;
                case "mithramedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 7, 20, 21));
                    break;
                case "muiremedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 7, 20, 221));
                    break;
                case "nissesmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 7, 20, 129));
                    break;
                case "spindelhallamedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 35, 45, 125));
                    break;
                case "spraggonmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 18, 28, 222));
                    break;
                case "stonehengemedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 30, 50, 19));
                    break;
                case "tepokmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 17, 30, 24));
                    break;
                case "treibhmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 20, 35, 224));
                    break;
                case "trollheimmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 40, 50, 150));
                    break;
                case "tursuilmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 40, 50, 190));
                    break;
                case "varulvhamnmedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 30, 40, 127));
                    break;
                case "vendomedallion":
                    items.Add(new DungeonItem(itemTemplate.Value, 18, 30, 126));
                    break;
                case "fragmentofpower":
                    items.Add(new DungeonItem(itemTemplate.Value, 10, 49, 0));
                    break;
            }
        }

        return items;
    }
    
    public static void GetLevelRangeForDungeon(int regionId, out int minLevel, out int maxLevel)
    {
        switch (regionId)
        {
            case 23: //catacombs of cordova
                minLevel = 25;
                maxLevel = 35;
                break;
            case 220://coruscating mines
                minLevel = 30;
                maxLevel = 45;
                break;
            case 128://cursed tomb
                minLevel = 15;
                maxLevel = 30;
                break;
            case 22://keltoi fogue
                minLevel = 15;
                maxLevel = 30;
                break;
            case 223://koalinth caverns
                minLevel = 15;
                maxLevel = 30;
                break;
            case 21://mithra's tomb
                minLevel = 5;
                maxLevel = 20;
                break;
            case 221://muire
                minLevel = 5;
                maxLevel = 20;
                break;
            case 129://nisse's lair
                minLevel = 5;
                maxLevel = 20;
                break;
            case 125://spindelhalla
                minLevel = 30;
                maxLevel = 45;
                break;
            case 222://spraggon's den
                minLevel = 15;
                maxLevel = 25;
                break;
            case 20://stonehenge barrows
                minLevel = 30;
                maxLevel = 45;
                break;
            case 24://tepok
                minLevel = 15;
                maxLevel = 30;
                break;
            case 224://treibh cailte
                minLevel = 25;
                maxLevel = 40;
                break;
            case 127://varulvhamn
                minLevel = 25;
                maxLevel = 40;
                break;
            case 126://vendo
                minLevel = 15;
                maxLevel = 30;
                break;
            default:
                minLevel = 1;
                maxLevel = 50;
                break;
        }
    }

    public static List<string> GetDungeonLevelInfo()
    {
        List<string> output = new List<string>();

        foreach (var id in GetDungeonZoneIds())
        {
            GetLevelRangeForDungeon(id, out var minLvl, out var maxLvl);
            output.Add($"{GetNameFromId(id)} | {minLvl} - {maxLvl}");
        }

        return output;
    }

    public static List<int> GetDungeonZoneIds()
    {
        List<int> Ids = new List<int>();
        Ids.Add(23); //catacombs of cordova
        Ids.Add(220); //coruscating mines
        Ids.Add(128); //cursed tomb
        Ids.Add(22); //keltoi
        Ids.Add(223); //koalinth caverns
        Ids.Add(21); //mithra's tomb
        Ids.Add(221); //muire
        Ids.Add(129); //nisse
        Ids.Add(125); //spindelhalla
        Ids.Add(222); //spraggon den
        Ids.Add(20); //stonehenge barrows
        Ids.Add(24); //tepok
        Ids.Add(224); //treibh cailte
        Ids.Add(127); //varulvhamn
        Ids.Add(126); //vendo

        return Ids;
    }

    private static string GetNameFromId(int id)
    {
        switch (id)
        {
            case 23:
                return "Catacombs of Cordova";
            case 220:
                return "Coruscating Mines";
            case 128:
                return "Cursed Tomb";
            case 22:
                return "Keltoi";
            case 223:
                return "Koalinth Caverns";
            case 21:
                return "Mithra's Tomb";
            case 221:
                return "Muire's Tomb";
            case 129:
                return "Nisse's Lair";
            case 125:
                return "Spindelhalla";
            case 222:
                return "Spraggon's Den";
            case 20:
                return "Stonehenge Barrows";
            case 24:
                return "Tepok's Mine";
            case 224:
                return "Treibh Cailte";
            case 127:
                return "Varulvhamn";
            case 126:
                return "Vendo Caverns";
            default:
                return $"{id} UNMAPPED - Talk to Fen";
            
        }
    }
}

class DungeonItem
{
    private ItemTemplate _template;
    private int _startLevel;
    private int _endLevel;
    private int _zoneID;

    public int ZoneID => _zoneID;
    public ItemTemplate Template => _template;
    
    public DungeonItem(ItemTemplate template, int startLevel, int endLevel, int zoneId)
    {
        _template = template;
        _startLevel = startLevel;
        _endLevel = endLevel;
        _zoneID = zoneId;
    }
}