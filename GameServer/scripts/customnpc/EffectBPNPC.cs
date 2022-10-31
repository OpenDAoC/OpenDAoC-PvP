//Dawn of Light Version 1.7.48
//12/13/2004
//Written by Gavinius
//based on Nardin and Zjovaz previous script
//08/18/2005
//by sirru
//completely rewrote SetEffect, no duel item things whatsoever left
//compatible with dol 1.7 and added some nice things like more 
//smalltalk, equip, emotes, changing of itemnames and spellcast at the end of process
//plus i added changing of speed and color
//what could be done is trimming the prefixes from the name instead of looking at the db, but i dont know how to do that :)

using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Database.UniqueID;

namespace DOL.GS {
    [NPCGuildScript("Effect Master")]
    public class EffectBPNPC : EffectNPC {
        private string EFFECTNPC_ITEM_WEAK = "DOL.GS.Scripts.EffectNPC_Item_Manipulation";//used to store the item in the player
        private ushort spell = 7215;//The spell which is casted
        private ushort duration = 3000;//3s, the duration the spell is cast
        private eEmote Emotes = eEmote.Raise;//The Emote the NPC does when Interacted
        private Queue m_timer = new Queue();//Gametimer for casting some spell at the end of the process
        private Queue castplayer = new Queue();//Used to hold the player who the spell gets cast on
        public string currencyName = "BPs";
        private int effectPrice = 5000; //effects price
        private int dyePrice = 2000; //effects price
        private int removePrice = 0; //removal is free
        public string TempProperty = "ItemEffect";
        public string DisplayedItem = "EffectDisplay";
        public string TempEffectId = "TempEffectID";
        public string TempColorId = "TempColorID";
        public override void SetColor(GamePlayer player, int color, int price)
        {
            InventoryItem item = player.TempProperties.getProperty<InventoryItem>(EFFECTNPC_ITEM_WEAK);

            player.TempProperties.removeProperty(EFFECTNPC_ITEM_WEAK);

            if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
                || item.OwnerID == null || item.OwnerID != player.InternalID)
            {
                player.Out.SendMessage("Invalid item.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }

            if (item.Object_Type == 41 || item.Object_Type == 43 || item.Object_Type == 44 ||
               item.Object_Type == 46)
            {
                SendReply(player, "You can't dye that.");
                return;
            }
            

            if (player.BountyPoints < price)
            {
                SayTo(player, eChatLoc.CL_PopupWindow, "I need " + price + " " + currencyName + " to dye that.");
                return;
            }

            m_timer.Enqueue(new ECSGameTimer(this, new ECSGameTimer.ECSTimerCallback(Effect), duration));
            castplayer.Enqueue(player);

            player.Inventory.RemoveItem(item);
            ItemUnique unique = new ItemUnique(item.Template);
            unique.Color = color;
            unique.ObjectId = "Unique" + System.Guid.NewGuid().ToString();
            unique.Id_nb = "Unique" + System.Guid.NewGuid().ToString();
            if (GameServer.Database.ExecuteNonQuery("SELECT ItemUnique_ID FROM itemunique WHERE ItemUnique_ID = 'unique.ObjectId'"))
            {
                unique.ObjectId = "Unique" + System.Guid.NewGuid().ToString();
            }
            if (GameServer.Database.ExecuteNonQuery("SELECT Id_nb FROM itemunique WHERE Id_nb = 'unique.Id_nb'"))
            {
                unique.Id_nb = IDGenerator.GenerateID();
            }
            GameServer.Database.AddObject(unique);

            InventoryItem newInventoryItem = GameInventoryItem.Create<ItemUnique>(unique);
            if(item.IsCrafted)
                newInventoryItem.IsCrafted = true;
            if(item.Creator != "")
                newInventoryItem.Creator = item.Creator;
            newInventoryItem.Count = 1;
            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, newInventoryItem);
            player.Out.SendInventoryItemsUpdate(new InventoryItem[] { newInventoryItem });
            //player.RealmPoints -= price;
            //player.RespecRealm();
            //SetRealmLevel(player, (int)player.RealmPoints);

            player.BountyPoints -= price;
            player.Out.SendUpdatePoints();
            
            player.TempProperties.removeProperty(TempProperty);
            player.TempProperties.removeProperty(DisplayedItem);
            player.TempProperties.removeProperty(TempEffectId);
            player.TempProperties.removeProperty(TempColorId);
            
            SendReply(player, "Thanks for your donation. The color has come out beautifully, wear it with pride.");

            foreach (GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visplayer.Out.SendSpellCastAnimation(this, spell, 30);
            }
        }


        #region seteffect
        public override void SetEffect(GamePlayer player, int effect, int price)
        {
            if (player == null)
                return;

            InventoryItem item = player.TempProperties.getProperty<InventoryItem>(EFFECTNPC_ITEM_WEAK);
            player.TempProperties.removeProperty(EFFECTNPC_ITEM_WEAK);

            if (item == null)
                return;

            if ((item.Object_Type < 1 || item.Object_Type > 26) || item.Object_Type == 42)
            {
                SendReply(player, "I cannot work on anything other than weapons and shields.");
                return;
            }

            if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
                || item.OwnerID == null || item.OwnerID != player.InternalID)
            {
                player.Out.SendMessage("Invalid item.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.BountyPoints < price)
            {
                SayTo(player, eChatLoc.CL_PopupWindow, "I need " + price + " " + currencyName + " to enchant that.");
                return;
            }

            m_timer.Enqueue(new ECSGameTimer(this, new ECSGameTimer.ECSTimerCallback(Effect), duration));
            castplayer.Enqueue(player);


            player.Inventory.RemoveItem(item);
            ItemUnique unique = new ItemUnique(item.Template);
            unique.Effect = effect;
            unique.Id_nb = IDGenerator.GenerateID();
            unique.ObjectId = "Unique" + System.Guid.NewGuid().ToString();
            if (GameServer.Database.ExecuteNonQuery("SELECT ItemUnique_ID FROM itemunique WHERE ItemUnique_ID = 'unique.ObjectId'"))
            {
                unique.ObjectId = "Unique" + System.Guid.NewGuid().ToString();
            }
            if (GameServer.Database.ExecuteNonQuery("SELECT Id_nb FROM itemunique WHERE Id_nb = 'unique.Id_nb'"))
            {
                unique.Id_nb = IDGenerator.GenerateID();
            }
            GameServer.Database.AddObject(unique);

            InventoryItem newInventoryItem = GameInventoryItem.Create<ItemUnique>(unique);
            if(item.IsCrafted)
                newInventoryItem.IsCrafted = true;
            if(item.Creator != "")
                newInventoryItem.Creator = item.Creator;
            newInventoryItem.Count = 1;
            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, newInventoryItem);
            player.Out.SendInventoryItemsUpdate(new InventoryItem[] { newInventoryItem });
            //player.RealmPoints -= price;
            //player.RespecRealm();
            //SetRealmLevel(player, (int)player.RealmPoints);

            player.BountyPoints -= price;
            player.Out.SendUpdatePoints();
            
            player.TempProperties.removeProperty(TempProperty);
            player.TempProperties.removeProperty(DisplayedItem);
            player.TempProperties.removeProperty(TempEffectId);
            player.TempProperties.removeProperty(TempColorId);
            
            SendReply(player, "Thanks for your donation. May the " + item.Name + " lead you to a bright future.");
            foreach (GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visplayer.Out.SendSpellCastAnimation(this, spell, 30);
            }
        }
        #endregion seteffect
        
    }
}

