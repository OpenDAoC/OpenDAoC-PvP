using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.Scripts;

public class TitanAlphaLootMerchant : BattlegroundEventLoot
{
	readonly int realmPointAward = 213875;
	
	public override bool AddToWorld()
    {
	    if (base.AddToWorld())
	    {
		    Model = 996;
		    Name = "Quartermaster";
		    GuildName = "Titan";
		    Level = 75;
		    Size = 100;
		    Flags |= GameNPC.eFlags.PEACE;
		    return true;    
	    }

	    return false;
    }
    
    public override bool Interact(GamePlayer player)
    {
        string realmName = player.Realm.ToString();
        if (realmName.Equals("_FirstPlayerRealm")) {
            realmName = "Albion";
        } else if (realmName.Equals("_LastPlayerRealm")){
            realmName = "Hibernia";
        }
        TurnTo(player.X, player.Y);

        /*
        if (!player.Boosted)
        {
            player.Out.SendMessage("I'm sorry " + player.Name + ", my services are not available to you.", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
            return false;
        }*/
			
        player.Out.SendMessage("Hello " + player.Name + "! We're happy to see you here supporting our testing.\n" +
                               "For your efforts, the team has procured a [full suit] of equipment and some [gems] to adorn them with. " +
                               "Additionally, I can provide you with some [weapons] and [coin].\n\n" +
                               "This is the best gear we could provide on short notice. If you want something better, you'll have to take it from your enemies on the battlefield. " + 
                               "Go forth, and do battle!", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
        
        var lvCap = ServerProperties.Properties.EVENT_LVCAP;
        
        if (lvCap != 0 && player.Level < lvCap)
		{
			player.Out.SendMessage($"It also looks like you could use a bit of [experience] to reach level {lvCap}.", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
		}

        if (lvCap == 50)
        {
	        player.Out.SendMessage($"The king has granted me permission to give you some [realm points] to get you started.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }

	    return true;
    }

    
    public override bool WhisperReceive(GameLiving source, string str) {
	    if (!(source is GamePlayer)) return false;
		GamePlayer player = (GamePlayer)source;
		TurnTo(player.X, player.Y);
		eRealm realm = player.Realm;
		eCharacterClass charclass = (eCharacterClass)player.CharacterClass.ID;
		eObjectType armorType = GetArmorType(realm, charclass, (byte)(player.Level));
		eColor color = eColor.White;

		switch (realm) {
			case eRealm.Hibernia:
				color = eColor.Green_4;
				break;
			case eRealm.Albion:
				color = eColor.Red_4;
				break;
			case eRealm.Midgard:
				color = eColor.Blue_4;
				break;
		}
		if (str.Equals("full suit"))
		{
			const string customKey = "free_event_armor";
			var hasFreeArmor = DOLDB<DOLCharactersXCustomParam>.SelectObject(DB.Column("DOLCharactersObjectId").IsEqualTo(player.ObjectId).And(DB.Column("KeyName").IsEqualTo(customKey)));

			if (hasFreeArmor != null)
			{
				player.Out.SendMessage("Sorry " + player.Name + ", I don't have enough items left to give you another set.\n\n Go fight for your Realm to get more equipment!", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				return false;
			}

			GenerateArmor(player);
			
			player.Out.SendMessage("May it protect you well, " + player.Name + ".", eChatType.CT_Say,eChatLoc.CL_PopupWindow);

			/*
			DOLCharactersXCustomParam charFreeEventEquip = new DOLCharactersXCustomParam();
			charFreeEventEquip.DOLCharactersObjectId = player.ObjectId;
			charFreeEventEquip.KeyName = customKey;
			charFreeEventEquip.Value = "1";
			GameServer.Database.AddObject(charFreeEventEquip);*/
		} 
		else if (str.Equals("weapons")) {
			
			const string customKey = "free_event_weapons";
			var hasFreeWeapons = DOLDB<DOLCharactersXCustomParam>.SelectObject(DB.Column("DOLCharactersObjectId").IsEqualTo(player.ObjectId).And(DB.Column("KeyName").IsEqualTo(customKey)));

			if (hasFreeWeapons != null)
			{
				player.Out.SendMessage("Sorry " + player.Name + ", I don't have enough weapons left to give you another set.\n\n Go fight for your Realm to get more equipment!", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				return false;
			}
			
			GenerateWeaponsForClass(charclass, player);
			
			player.Out.SendMessage("May it strike your enemies well, " + player.Name + ".", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
			
			/*
			DOLCharactersXCustomParam charFreeEventEquip = new DOLCharactersXCustomParam();
			charFreeEventEquip.DOLCharactersObjectId = player.ObjectId;
			charFreeEventEquip.KeyName = customKey;
			charFreeEventEquip.Value = "1";
			GameServer.Database.AddObject(charFreeEventEquip);*/
		} else if (str.Equals("gems"))
        {
			const string customKey = "free_event_gems";
			var hasFreeArmor = DOLDB<DOLCharactersXCustomParam>.SelectObject(DB.Column("DOLCharactersObjectId").IsEqualTo(player.ObjectId).And(DB.Column("KeyName").IsEqualTo(customKey)));

			if (hasFreeArmor != null)
			{
				player.Out.SendMessage("Sorry " + player.Name + ", I don't have enough items left to give you another set.\n\n Go fight for your Realm to get more equipment!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}

			if (player.Level < 50)
			{
				
				GenerateGems(player);
				// List<eInventorySlot> gemSlots = new List<eInventorySlot>();
				// gemSlots.Add(eInventorySlot.Cloak);
				// gemSlots.Add(eInventorySlot.Neck);
				// gemSlots.Add(eInventorySlot.Waist);
				// gemSlots.Add(eInventorySlot.Jewellery);
				// gemSlots.Add(eInventorySlot.LeftRing);
				// gemSlots.Add(eInventorySlot.RightRing);
				// gemSlots.Add(eInventorySlot.LeftBracer);
				// gemSlots.Add(eInventorySlot.RightBracer);
				//
				// foreach (eInventorySlot islot in gemSlots)
				// {
				// 	GeneratedUniqueItem item = null;
				// 	item = new GeneratedUniqueItem(realm, charclass, (byte)(player.Level + freeLootLevelOffset), eObjectType.Magical, islot);
				// 	item.AllowAdd = true;
				// 	item.Color = (int)color;
				// 	item.IsTradable = false;
				// 	item.Price = 1;
				// 	GameServer.Database.AddObject(item);
				// 	InventoryItem invitem = GameInventoryItem.Create<ItemUnique>(item);
				// 	player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
				// 	//player.Out.SendMessage("Generated: " + item.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				// }
				
				player.Out.SendMessage("May they adorn you well, " + player.Name + ".", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				
			}
			else
			{
				List<ItemTemplate> atlasGem = new List<ItemTemplate>(DOLDB<ItemTemplate>.SelectObjects(DB.Column("Id_nb").IsEqualTo("atlas_gem")));
				InventoryItem invitem = GameInventoryItem.Create<ItemUnique>(atlasGem.FirstOrDefault());
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
			
				List<ItemTemplate> atlasCloak = new List<ItemTemplate>(DOLDB<ItemTemplate>.SelectObjects(DB.Column("Id_nb").IsEqualTo("atlas_cloak")));
				InventoryItem invitem2 = GameInventoryItem.Create<ItemUnique>(atlasCloak.FirstOrDefault());
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem2);
				
				List<ItemTemplate> atlasRing = new List<ItemTemplate>(DOLDB<ItemTemplate>.SelectObjects(DB.Column("Id_nb").IsEqualTo("atlas_ring")));
				InventoryItem invitem3 = GameInventoryItem.Create<ItemUnique>(atlasRing.FirstOrDefault());
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem3);
			}
			
			
			/*
			DOLCharactersXCustomParam charFreeEventEquip = new DOLCharactersXCustomParam();
			charFreeEventEquip.DOLCharactersObjectId = player.ObjectId;
			charFreeEventEquip.KeyName = customKey;
			charFreeEventEquip.Value = "1";
			GameServer.Database.AddObject(charFreeEventEquip);*/
		}
		else if (str.Equals("coin"))
		{
			const string moneyKey = "free_money";
			//var hasFreeOrbs = DOLDB<DOLCharactersXCustomParam>.SelectObject(DB.Column("DOLCharactersObjectId").IsEqualTo(player.ObjectId).And(DB.Column("KeyName").IsEqualTo(customKey)));
			string customKey = moneyKey + player.Realm;
			var hasAccountMoney = DOLDB<AccountXCustomParam>.SelectObject(DB.Column("Name").IsEqualTo(player.Client.Account.Name).And(DB.Column("KeyName").IsEqualTo(customKey)));
		
			if (hasAccountMoney != null)
			{
				player.Out.SendMessage("Sorry " + player.Name + ", I don't have enough money left to give you more.\n\n Go fight for your Realm to get some!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}
			
			AccountXCustomParam charFreeEventMoney = new AccountXCustomParam();
			charFreeEventMoney.Name = player.Client.Account.Name;
			charFreeEventMoney.KeyName = customKey;
			charFreeEventMoney.Value = "1";
			GameServer.Database.AddObject(charFreeEventMoney);

			player.AddMoney(5000000);
		}
		else if (str.Equals("Atlas Orbs"))
		{
			return false;
		}
		else if (str.Equals("experience"))
		{
			var lvCap = ServerProperties.Properties.EVENT_LVCAP;
			
			if (player.Level < lvCap)
			{
				string customKey = "BoostedLevel-" + lvCap;
				var boosterKey = DOLDB<DOLCharactersXCustomParam>.SelectObject(DB.Column("DOLCharactersObjectId").IsEqualTo(player.ObjectId).And(DB.Column("KeyName").IsEqualTo(customKey)));
                        
				player.Out.SendMessage("I have given you enough experience to fight, now speak again with me to get a new set of equipment.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				player.Boosted = true;
				player.CanGenerateNews = false;
				player.Level = (byte)lvCap;
				player.Health = player.MaxHealth;
                        
                        
				if (boosterKey == null)
				{
					DOLCharactersXCustomParam boostedLevel = new DOLCharactersXCustomParam();
					boostedLevel.DOLCharactersObjectId = player.ObjectId;
					boostedLevel.KeyName = customKey;
					boostedLevel.Value = "1";
					GameServer.Database.AddObject(boostedLevel);
				}

				return true;
			}
			player.Out.SendMessage("You are a veteran already, go fight for your Realm!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return false;
		}
		else if (str.Equals("realm points"))
		{
			if (player.RealmPoints < realmPointAward && ServerProperties.Properties.EVENT_LVCAP == 50)
			{
				player.GainRealmPoints((long)(double)(realmPointAward - player.RealmPoints));
				player.Out.SendMessage("A royal award to get you started.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return true;
			}
			player.Out.SendMessage("You have already been granted these realm points. Go slay others to earn more.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return false;

		}

		
		return true;
	}
    
}