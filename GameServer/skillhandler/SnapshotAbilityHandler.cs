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
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Sure Shot ability
	/// </summary>
	[SkillHandlerAttribute(Abilities.Snapshot)]
	public class SnapshotAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// The ability reuse time in milliseconds
		/// </summary>
		protected const int REUSE_TIMER = 60000 * 10;

		/// <summary>
		/// The ability effect duration in milliseconds
		/// </summary>
		public const int DURATION = 60 * 1000; // 60 secondi
		
		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not use this ability.");
				return;
			}

			if (!player.IsAlive)
			{
				player.Out.SendMessage("You cannot use this while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage("You cannot use this while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage("You cannot use this while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage("You must be standing to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			var snap = player.effectListComponent.GetAllEffects()
				.FirstOrDefault(x => x.GetType() == typeof(SnapshotECSEffect));
			if (snap != null)
			{
				player.Out.SendMessage("That ability is already active, wait until it expires.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			player.DisableSkill(ab, REUSE_TIMER);

			new SnapshotECSEffect(new ECSGameEffectInitParams(null, DURATION, 1));
		}
	}
}
