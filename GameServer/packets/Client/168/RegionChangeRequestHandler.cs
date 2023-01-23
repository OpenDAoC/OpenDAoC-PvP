/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Linq;
using System.Collections;
using System.Reflection;

using DOL.Database;
using DOL.GS.API;
using DOL.GS.Quests;
using DOL.GS.ServerRules;

using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PlayerRegionChangeRequest, "Player Region Change Request handler.", eClientStatus.PlayerInGame)]
	public class PlayerRegionChangeRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds jump point types
		/// </summary>
		protected readonly Hashtable m_customJumpPointHandlers = new Hashtable();

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort jumpSpotId = packet.ReadShort();

			eRealm targetRealm = client.Player.Realm;

			if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis && client.Player.CurrentZone.Realm != eRealm.None)
			{
				// if we are in TrialsOfAtlantis then base the target jump on the current region realm instead of the players realm
				// this is only used if zone table has the proper realms defined, otherwise it reverts to old behavior - Tolakram
                targetRealm = client.Player.CurrentZone.Realm;
			}

			var filterRealm = DB.Column("Realm").IsEqualTo((byte)targetRealm).Or(DB.Column("Realm").IsEqualTo(0)).Or(DB.Column("Realm").IsNull());
			var zonePoint = DOLDB<ZonePoint>.SelectObject(DB.Column("Id").IsEqualTo(jumpSpotId).And(filterRealm));

			if (zonePoint == null || zonePoint.TargetRegion == 0)
			{
				ChatUtil.SendDebugMessage(client, $"Invalid Jump (ZonePoint table): [{jumpSpotId}]{((zonePoint == null) ? ". Entry missing!" : ". TargetRegion is 0!")}");
				zonePoint = new ZonePoint();
				zonePoint.Id = jumpSpotId;
			}

			if (client.Account.PrivLevel > 1)
			{
				client.Out.SendMessage($"JumpSpotID = {jumpSpotId}", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage($"ZonePoint Target: Region = {zonePoint.TargetRegion}, ClassType = \'{zonePoint.ClassType}\'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			//Dinberg: Fix - some jump points are handled code side, such as instances.
			//As such, region MAY be zero in the database, so this causes an issue.

			if (zonePoint.TargetRegion != 0)
			{
				Region reg = WorldMgr.GetRegion(zonePoint.TargetRegion);
				if (reg != null)
				{
					// check for target region disabled if player is in a standard region
					// otherwise the custom region should handle OnZonePoint for this check
					if (client.Player.CurrentRegion.IsCustom == false && reg.IsDisabled)
					{
						if ((client.Player.Mission is TaskDungeonMission &&
						     (client.Player.Mission as TaskDungeonMission).TaskRegion.Skin == reg.Skin) == false)
						{
							client.Out.SendMessage("This region has been disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							if (client.Account.PrivLevel == 1)
								return;
						}
					}
				}
			}

			// Allow the region to either deny exit or handle the zonepoint in a custom way
			if (client.Player.CurrentRegion.OnZonePoint(client.Player, zonePoint) == false)
			{
				return;
			}

			if (client.Player.InCombatPvPInLast(30000))
			{
				var time = 30000 + client.Player.LastCombatTickPvP - GameLoop.GameLoopTime;
				client.Out.SendMessage($"You have been in pvp combat recently and are unable to teleport for {time/1000} seconds!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (client.Account.PrivLevel == 1)
					return;
			}

			int minLvl = 0;
			int maxLvl = 0;
			GetLevelRangeForDungeon(zonePoint.TargetRegion, out minLvl, out maxLvl);

			if (client.Player.Level < minLvl)
			{
				client.Out.SendMessage($"You are below the minimum level ({minLvl}) for this dungeon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (client.Account.PrivLevel == 1)
					return;
			}
			
			if (client.Player.Level > maxLvl)
			{
				client.Out.SendMessage($"You are above the maximum level ({maxLvl}) for this dungeon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (client.Account.PrivLevel == 1)
					return;
			}

			//check caps for battleground
			Battleground bg = GameServer.KeepManager.GetBattleground(zonePoint.TargetRegion);
			if (bg != null)
			{
				if (client.Player.Level < bg.MinLevel && client.Player.Level > bg.MaxLevel &&
				    client.Player.RealmLevel >= bg.MaxRealmLevel)
					return;
			}

			IJumpPointHandler customHandler = null;
			if (string.IsNullOrEmpty(zonePoint.ClassType) == false)
			{
				customHandler = (IJumpPointHandler)m_customJumpPointHandlers[zonePoint.ClassType];

				// check for db change to update cached handler
				if (customHandler != null && customHandler.GetType().FullName != zonePoint.ClassType)
				{
					customHandler = null;
				}

				if (customHandler == null)
				{
					//Dinberg - Instances need to use a special handler. This is because some instances will result
					//in duplicated zonepoints, such as if Tir Na Nog were to be instanced for a quest.
					string type = (client.Player.CurrentRegion.IsInstance)
						? "DOL.GS.ServerRules.InstanceDoorJumpPoint"
						: zonePoint.ClassType;
					Type t = ScriptMgr.GetType(type);

					if (t == null)
					{
						Log.ErrorFormat("jump point {0}: class {1} not found!", zonePoint.Id, zonePoint.ClassType);
					}
					else if (!typeof(IJumpPointHandler).IsAssignableFrom(t))
					{
						Log.ErrorFormat("jump point {0}: class {1} must implement IJumpPointHandler interface!", zonePoint.Id,
						                zonePoint.ClassType);
					}
					else
					{
						try
						{
							customHandler = (IJumpPointHandler)Activator.CreateInstance(t);
						}
						catch (Exception e)
						{
							customHandler = null;
							Log.Error(
								string.Format("jump point {0}: error creating a new instance of jump point handler {1}", zonePoint.Id,
								              zonePoint.ClassType), e);
						}
					}
				}

				if (customHandler != null)
				{
					m_customJumpPointHandlers[zonePoint.ClassType] = customHandler;
				}
			}

			new RegionChangeRequestHandler(client.Player, zonePoint, customHandler).Start(1);
		}

		private void GetLevelRangeForDungeon(int regionId, out int minLevel, out int maxLevel)
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

		/// <summary>
		/// Handles player region change requests
		/// </summary>
		protected class RegionChangeRequestHandler : RegionECSAction
		{
			/// <summary>
			/// Checks whether player is allowed to jump
			/// </summary>
			protected readonly IJumpPointHandler m_checkHandler;

			/// <summary>
			/// The target zone point
			/// </summary>
			protected readonly ZonePoint m_zonePoint;

			/// <summary>
			/// Constructs a new RegionChangeRequestHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="zonePoint">The target zone point</param>
			/// <param name="checker">The jump point checker instance</param>
			public RegionChangeRequestHandler(GamePlayer actionSource, ZonePoint zonePoint, IJumpPointHandler checkHandler)
				: base(actionSource)
			{
				if (zonePoint == null)
					throw new ArgumentNullException("zonePoint");

				m_zonePoint = zonePoint;
				m_checkHandler = checkHandler;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override int OnTick(ECSGameTimer timer)
			{
				var player = (GamePlayer)m_actionSource;

				
				//TODO - nuke this when SI is ready to come back
				if ((m_zonePoint.TargetRegion is 51 or 151 or 181) && ServerProperties.Properties.DISABLE_SI_TELEPORT)
				{
					//51 == alb SI
					//151 == mid SI
					//181 == hib SI
					player.Out.SendMessage("The Shrouded Isles have been temporarily disabled.",
						eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}

				Region reg = WorldMgr.GetRegion(m_zonePoint.TargetRegion);
				if (reg != null && reg.Expansion > (int)player.Client.ClientType)
				{
					player.Out.SendMessage("Destination region (" + reg.Description + ") is not supported by your client type.",
					                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}

				try
				{

					//check if the zonepoint has source locations set  Check prior to any zonepoint modification by handlers
					if (m_zonePoint.SourceRegion == 0)
					{
						m_zonePoint.SourceRegion = player.CurrentRegionID;
						m_zonePoint.SourceX = player.X;
						m_zonePoint.SourceY = player.Y;
						m_zonePoint.SourceZ = player.Z;
						GameServer.Database.SaveObject(m_zonePoint);
					}

				}
				catch (Exception ex)
				{
					Log.Error("Can't save updated ZonePoint with source info.", ex);
				}

			

				if (m_checkHandler != null)
				{
					try
					{
						if (!m_checkHandler.IsAllowedToJump(m_zonePoint, player))
							return 0;
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Jump point handler (" + m_zonePoint.ClassType + ")", e);

						player.Out.SendMessage("exception in jump point (" + m_zonePoint.Id + ") handler...", eChatType.CT_System,
						                       eChatLoc.CL_SystemWindow);
						return 0;
					}
				}

				//move the player
				player?.MoveTo(m_zonePoint.TargetRegion, m_zonePoint.TargetX, m_zonePoint.TargetY, m_zonePoint.TargetZ, m_zonePoint.TargetHeading);
				return 0;
			}
		}
	}
}