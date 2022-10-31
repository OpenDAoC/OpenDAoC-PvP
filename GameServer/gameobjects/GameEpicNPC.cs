﻿using System;
using System.Text.RegularExpressions;
using DOL.GS.ServerProperties;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace DOL.GS
{
    public class GameEpicNPC : GameNPC
    {
        public GameEpicNPC() : base()
        {
            ScalingFactor = 60;
        }
        public override bool HasAbility(string keyName)
        {
            //if (IsAlive && keyName == GS.Abilities.CCImmunity) //incase we decide to make them immune to any cc
            //return true;
            if (IsAlive && keyName == GS.Abilities.ConfusionImmunity)
                return true;
            if (IsAlive && keyName == GS.Abilities.NSImmunity)
                return true;

            return base.HasAbility(keyName);
        }
        public override short MaxSpeedBase
        {
            get => (short)(191 + (Level * 2));
            set => m_maxSpeedBase = value;
        }
        public override int MaxHealth
        {
            get { return (10000 + (Level * 125)); }
        }
        public override void Die(GameObject killer)
        {
            try
            {
                // debug
                log.Debug($"{Name} killed by {killer.Name}");

                if (killer is GamePet pet) killer = pet.Owner;

                var playerKiller = killer as GamePlayer;

                var amount = Util.Random(Level / 10, Level * 2 / 10);
                var baseChance = 80;
                var carapaceChance = Properties.CARAPACE_DROPCHANCE;
                var realmLoyalty = 0;

                double numCurrentLoyalDays = LoyaltyManager.GetPlayerRealmLoyalty(playerKiller) != null
                           ? LoyaltyManager.GetPlayerRealmLoyalty(playerKiller).Days
                           : 0;

                if (numCurrentLoyalDays > 30)
                {
                    numCurrentLoyalDays = 30;
                }

                if (numCurrentLoyalDays >= 1)
                {
                    realmLoyalty = (int)Math.Round(20 * (numCurrentLoyalDays / 30.0));
                }

                var achievementMob = Regex.Replace(Name, @"\s+", "");

                var killerBG = (BattleGroup)playerKiller?.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);


                if (killerBG != null)
                {
                    ICollection bgPlayers;
                    lock (killerBG.Members.Keys)
                    {
                         bgPlayers = killerBG.Members.Keys;
                    }

                    if (bgPlayers != null)
                    {
                        foreach (GamePlayer bgPlayer in bgPlayers)
                        {
                            if (bgPlayer.IsWithinRadius(this, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                            {
                                if (bgPlayer.Level < 45) continue;

                                if (Util.Chance(baseChance + realmLoyalty))
                                {
                                    AtlasROGManager.GenerateReward(bgPlayer, amount);
                                }

                                if (Util.ChanceDouble(carapaceChance))
                                {
                                    AtlasROGManager.GenerateBeetleCarapace(bgPlayer);
                                }

                                bgPlayer.Achieve($"{achievementMob}-Credit");
                            }
                        }
                    }


                }
                else if (playerKiller?.Group != null)
                {
                    foreach (var groupPlayer in playerKiller.Group.GetPlayersInTheGroup())
                    {
                        if (groupPlayer.IsWithinRadius(this, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {
                            if (groupPlayer.Level < 45) continue;

                            if (Util.Chance(baseChance + realmLoyalty))
                            {
                                AtlasROGManager.GenerateReward(groupPlayer, amount);
                            }
                            if (Util.ChanceDouble(carapaceChance))
                            {
                                AtlasROGManager.GenerateBeetleCarapace(groupPlayer);
                            }
                            groupPlayer.Achieve($"{achievementMob}-Credit");

                        }
                    }
                }
                else if (playerKiller != null)
                {
                    if (playerKiller.Level >= 45)
                    {
                        if (Util.Chance(baseChance + realmLoyalty))
                        {
                            AtlasROGManager.GenerateReward(playerKiller, amount);
                        }

                        if (Util.ChanceDouble(carapaceChance))
                        {
                            AtlasROGManager.GenerateBeetleCarapace(playerKiller);
                        }

                        playerKiller.Achieve($"{achievementMob}-Credit");
                    }
                }
            }
            finally
            {
                base.Die(killer);
            }
        }
    }
}
