using System;
using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests;

public class PlayerKillKeepAttackAny : PlayerKillAny
{
    private const string questTitle = "[Daily] Property Reclamation";

    protected int RewardAmount = 250;
    
    public override void Notify(DOLEvent e, object sender, EventArgs args)
    {
        GamePlayer player = sender as GamePlayer;

        if (player?.IsDoingQuest(typeof(PlayerKillAny)) == null)
            return;

        if (sender != m_questPlayer)
            return;

        if (e != GameLivingEvent.EnemyKilled || Step != 1) return;
        EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

        AbstractGameKeep keep = DOL.GS.GameServer.KeepManager.GetKeepCloseToSpot(gArgs.Target.CurrentRegionID, gArgs.Target, 3500);
        if (keep is null || (gArgs.Target is GamePlayer targetPlayer && keep.Guild.ID != targetPlayer.Guild.ID)) return;

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
        get => "PlayerKillKeepAttackAny";
        set { ; }
    }

    public override void FinishQuest()
    {
        int reward = DOL.GS.ServerProperties.Properties.DAILY_RVR_REWARD;
        
        m_questPlayer.AddMoney(Money.GetMoney(0,0,m_questPlayer.Level,Util.Random(99),Util.Random(99)), "You receive {0} as a reward.");
        AtlasROGManager.GenerateReward(m_questPlayer, RewardAmount);
        PlayersKilled = 0;
			
        if (reward > 0)
        {
            m_questPlayer.Out.SendMessage($"You have been rewarded {reward} realm points for finishing {this.Name}.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            m_questPlayer.GainRealmPoints(reward, false);
            m_questPlayer.Out.SendUpdatePlayer();
        }
    }
}