using System;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;

public class GameInventoryItemLootable : GameInventoryItem
{
    public GameInventoryItemLootable()
        : base()
    {
    }

    public GameInventoryItemLootable(ItemTemplate template)
        : base(template)
    {
    }

    public GameInventoryItemLootable(ItemUnique template)
        : base(template)
    {
    }
    
    public GameInventoryItemLootable(InventoryItem item)
        : base(item)
    {
        OwnerID = item.OwnerID;
        ObjectId = item.ObjectId;
    }

    public override bool CanPersist => false;

    public override WorldInventoryItem Drop(GamePlayer anchor)
    {
        WorldInventoryItemLootable worldItem = new WorldInventoryItemLootable(this);

        Point2D itemloc = anchor.GetPointFromHeading(anchor.Heading, 30);
        worldItem.X = itemloc.X;
        worldItem.Y = itemloc.Y;
        worldItem.Z = anchor.Z;
        worldItem.Heading = anchor.Heading;
        worldItem.CurrentRegionID = anchor.CurrentRegionID;
        
        worldItem.AddToWorld();

        return worldItem;
    }
    
    public new static GameInventoryItemLootable Create(InventoryItem item)
    {
        string classType = item.Template.ClassType;

        if (!string.IsNullOrEmpty(classType))
        {
            GameInventoryItemLootable gameItem = ScriptMgr.CreateObjectFromClassType<GameInventoryItemLootable, InventoryItem>(classType, item);

            if (gameItem != null)
                return gameItem;
            
        }

        return new GameInventoryItemLootable(item);
    }
    
    public new static GameInventoryItemLootable Create(ItemTemplate item)
    {
        string classType = item.ClassType;
        var itemUnique = item as ItemUnique;

        if (!string.IsNullOrEmpty(classType))
        {
            GameInventoryItemLootable gameItem;
            
            if (itemUnique != null)
                gameItem = ScriptMgr.CreateObjectFromClassType<GameInventoryItemLootable, ItemUnique>(classType, itemUnique);
            else
                gameItem = ScriptMgr.CreateObjectFromClassType<GameInventoryItemLootable, ItemTemplate>(classType, item);

            if (gameItem != null)
                return gameItem;
        }

        if (itemUnique != null)
            return new GameInventoryItemLootable(itemUnique);

        return new GameInventoryItemLootable(item);
    }
    
}