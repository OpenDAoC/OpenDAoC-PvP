﻿using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;

namespace DOL.AI.Brain
{
    public class MeleeBehaviour : IAttackBehaviour
    {
        public MeleeBehaviour(GameLiving body)
        {
            Body = body;
        }

        private GameLiving Body { get; set; }

        #region IAttackBehaviour Members

        public void Attack(GameObject target)
        {
            Body.attackComponent.RequestStartAttack(target);
        }

        public void Retreat()
        {
            Body.attackComponent.StopAttack();
        }

        #endregion
    }
}
