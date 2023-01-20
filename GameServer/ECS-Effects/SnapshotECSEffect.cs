using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS
{
    public class SnapshotECSEffect : ECSGameAbilityEffect
    {
        public SnapshotECSEffect(ECSGameEffectInitParams initParams)
            : base(initParams)
        {
            EffectType = eEffect.Snapshot;
            EffectService.RequestStartEffect(this);
        }
        
        public override ushort Icon { get { return 3037; } }
        public override string Name { get { return "Snapshot"; } }
        public override bool HasPositiveEffect { get { return true; } }

        public override void OnStartEffect()
        {
            OwnerPlayer?.Out.SendMessage("You begin to steady your crossbow.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        
        public override void OnStopEffect()
        {
            OwnerPlayer?.Out.SendMessage("You are no longer steadying your crossbow.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        
    }
}
