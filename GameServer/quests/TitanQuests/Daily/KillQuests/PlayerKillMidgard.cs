﻿using System;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests;

public class PlayerKillMidgard : PlayerKillAny
{
    private const string questTitle = "[Daily] Send them to the Halls of Valhalla";
    
    public override void Notify(DOLEvent e, object sender, EventArgs args)
    {
        GamePlayer player = sender as GamePlayer;

        if (player?.IsDoingQuest(typeof(PlayerKillAny)) == null)
            return;

        if (sender != m_questPlayer)
            return;

        if (e != GameLivingEvent.EnemyKilled || Step != 1) return;
        EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

        if (gArgs.Target.Realm != eRealm.Midgard) return;

        if (gArgs.Target.Realm == 0 || gArgs.Target is not GamePlayer || (gArgs.Target.GuildName != null && gArgs.Target.GuildName.Equals(m_questPlayer.GuildName))||
            !(player.GetConLevel(gArgs.Target) > MIN_PLAYER_CON)) return;
        PlayersKilled++;
        player.Out.SendMessage("[Daily] Enemy Killed: (" + PlayersKilled + " | " + MAX_KILLED + ")", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
        player.Out.SendQuestUpdate(this);
					
        if (PlayersKilled >= MAX_KILLED)
        {
            // FinishQuest or go back to Dean
            Step = 2;
        }
    }
    
    public override string QuestPropertyKey
    {
        get => "PlayerKillMidgard";
        set { ; }
    }
}