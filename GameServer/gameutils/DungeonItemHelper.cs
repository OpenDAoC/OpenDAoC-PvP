using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DOL.GS;

public static class DungeonItemHelper
{
    private static List<DungeonItem> _availableItems;
    
    static DungeonItemHelper()
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