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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain that can be controlled
	/// </summary>
	public class BDHealerBrain : BDPetBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public BDHealerBrain(GameLiving owner) : base(owner)
		{
			AggroLevel = 0;
			AggroRange = 0;
		}

		#region Control

		/// <summary>
		/// Gets or sets the aggression state of the brain
		/// </summary>
		public override eAggressionState AggressionState
		{
			get { return eAggressionState.Passive; }
			set { }
		}

		/// <summary>
		/// Attack the target on command
		/// </summary>
		/// <param name="target"></param>
		public override void Attack(GameObject target) { }

		#endregion

		#region AI

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public override void CheckAbilities() { }

		/// <summary>
		/// Do nothing since we don't have offensive spells
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		protected override bool CheckOffensiveSpells(Spell spell) { return false; }

		/// <summary>
		/// Don't
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		protected override bool CheckInstantSpells(Spell spell) { return false; }

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro		
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public override void AddToAggroList(GameLiving living, int aggroamount) { }

		public override void  RemoveFromAggroList(GameLiving living) { }

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			return null;
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		public override void AttackMostWanted() { }

		/// <summary>
		/// Owner attacked event
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		public override void OnOwnerAttacked(AttackData ad) { }

		#endregion
	}
}
