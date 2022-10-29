using System;
using DOL.Database;
using DOL.GS;

namespace DOL.GS
{
    public class WorldInventoryItemLootable : WorldInventoryItem
    {
        
        public WorldInventoryItemLootable() : base()
        {
        }
        
        public WorldInventoryItemLootable(InventoryItem item) : base(item)
        {
        }
        
        public static WorldInventoryItemLootable CreateFromTemplate(ItemTemplate template)
        {
            if (template == null)
                return null;

            WorldInventoryItemLootable invItem = new WorldInventoryItemLootable();

            invItem.m_item = GameInventoryItemLootable.Create(template);
			
            invItem.m_item.SlotPosition = 0;
            invItem.m_item.OwnerID = null;

            invItem.Level = (byte)template.Level;
            invItem.Model = (ushort)template.Model;
            invItem.Emblem = template.Emblem;
            invItem.Name = template.Name;

            return invItem;
        }

        public static WorldInventoryItemLootable CreateUniqueFromTemplate(ItemTemplate template)
        {
            if (template == null)
                return null;

            WorldInventoryItemLootable invItem = new WorldInventoryItemLootable();
            ItemUnique item = new ItemUnique(template);
            GameServer.Database.AddObject(item);

            invItem.m_item = GameInventoryItemLootable.Create(item);
			
            invItem.m_item.SlotPosition = 0;
            invItem.m_item.OwnerID = null;

            invItem.Level = (byte)template.Level;
            invItem.Model = (ushort)template.Model;
            invItem.Emblem = template.Emblem;
            invItem.Name = template.Name;

            return invItem;
        }
    }
}
