using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using ECS.Debug;
using log4net;

namespace DOL.GS
{
    public static class EffectListService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ServiceName = "EffectListService";

        static EffectListService()
        {
            //This should technically be the world manager
            EntityManager.AddService(typeof(EffectListService));
        }

        public static void Tick(long tick)
        {
            Diagnostics.StartPerfCounter(ServiceName);

            GameLiving[] arr = EntityManager.GetLivingByComponent(typeof(EffectListComponent));

            Parallel.ForEach(arr, p =>
            {
                long startTick = GameTimer.GetTickCount();
                HandleEffects(tick,p);
                long stopTick = GameTimer.GetTickCount();
                if((stopTick - startTick)  > 25 )
                    log.Warn($"Long EffectListService.Tick for {p.Name}({p.ObjectID}) Time: {stopTick - startTick}ms");
            });
            
            Diagnostics.StopPerfCounter(ServiceName);               
        }

        private static void HandleEffects(long tick, GameLiving living)
        {
            if (living?.effectListComponent?.Effects.Count > 0)
            {
                var effects = new List<ECSGameEffect>(10);
                
                lock (living.effectListComponent.EffectsLock)
                {
                    var currentEffects = living.effectListComponent.Effects.Values.ToList();

                    for (int i = 0; i < currentEffects.Count; i++)
                    {
                        effects.AddRange(currentEffects[i]);
                    }
                }
                    
                for (int j = 0; j < effects.Count; j++)
                {
                    var e = effects[j];
                    if (e is null)
                        continue;

                    if (!e.Owner.IsAlive || e.Owner.ObjectState == GameObject.eObjectState.Deleted)
                    {
                        EffectService.RequestCancelEffect(e);
                        continue;
                    }

                    // TEMP - A lot of the code below assumes effects come from spells but many effects come from abilities (Sprint, Stealth, RAs, etc)
                    // This will need a better refactor later but for now this prevents crashing while working on porting over non-spell based effects to our system.
                    if (e is ECSGameAbilityEffect)
                    {
                        if (e.NextTick != 0 && tick > e.NextTick)
                            e.OnEffectPulse();
                        if (e.Duration > 0 && tick > e.ExpireTick)
                            EffectService.RequestCancelEffect(e);
                        continue;
                    }
                    else if (e is ECSGameSpellEffect effect)
                    {
                        if (tick > effect.ExpireTick && (!effect.IsConcentrationEffect() || effect.SpellHandler.Spell.IsFocus))
                        {
                            if (effect.EffectType == eEffect.Pulse && effect.SpellHandler.Caster.LastPulseCast == effect.SpellHandler.Spell)
                            {
                                if (effect.SpellHandler.Spell.PulsePower > 0)
                                {
                                    if (effect.SpellHandler.Caster.Mana >= effect.SpellHandler.Spell.PulsePower)
                                    {
                                        effect.SpellHandler.Caster.Mana -= effect.SpellHandler.Spell.PulsePower;
                                        effect.SpellHandler.StartSpell(null);
                                        effect.ExpireTick += effect.PulseFreq;
                                    }
                                    else
                                    {
                                        ((SpellHandler)effect.SpellHandler).MessageToCaster("You do not have enough power and your spell was canceled.", eChatType.CT_SpellExpires);
                                        EffectService.RequestCancelConcEffect(effect);
                                        continue;
                                    }
                                }
                                else
                                {
                                    effect.SpellHandler.StartSpell(null);
                                    effect.ExpireTick += effect.PulseFreq;
                                }

                                if (effect.SpellHandler.Spell.IsHarmful && effect.SpellHandler.Spell.SpellType != (byte)eSpellType.Charm && effect.SpellHandler.Spell.SpellType != (byte)eSpellType.SpeedDecrease)
                                {
                                    if (!(effect.Owner.IsMezzed || effect.Owner.IsStunned))
                                        ((SpellHandler)effect.SpellHandler).SendCastAnimation();
                                }
                            }
                            else
                            {
                                if (effect.SpellHandler.Spell.IsPulsing && effect.SpellHandler.Caster.LastPulseCast == effect.SpellHandler.Spell &&
                                    effect.ExpireTick >= (effect.LastTick + (effect.Duration > 0 ? effect.Duration : effect.PulseFreq)))
                                {
                                    //Add time to effect to make sure the spell refreshes instead of cancels
                                    effect.ExpireTick += GameLoop.TickRate;
                                    effect.LastTick = GameLoop.GameLoopTime;
                                }
                                else
                                {
                                    EffectService.RequestCancelEffect(effect);
                                }
                            }
                        }

                        if (!(effect is ECSImmunityEffect) && effect.EffectType != eEffect.Pulse && effect.SpellHandler.Spell.SpellType == (byte)eSpellType.SpeedDecrease)
                        {
                            if (tick > effect.NextTick)
                            {
                                double factor = 2.0 - (effect.Duration - effect.GetRemainingTimeForClient()) / (double)(effect.Duration >> 1);
                                if (factor < 0) factor = 0;
                                else if (factor > 1) factor = 1;

                                //effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect.SpellHandler.Spell.ID, 1.0 - effect.SpellHandler.Spell.Value * factor * 0.01);
                                effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect.EffectType, 1.0 - effect.SpellHandler.Spell.Value * factor * 0.01);

                                UnbreakableSpeedDecreaseSpellHandler.SendUpdates(effect.Owner);
                                effect.NextTick = GameLoop.GameLoopTime + effect.TickInterval;
                                if (factor <= 0)
                                    effect.ExpireTick = GameLoop.GameLoopTime - 1;
                            }
                        }

                        if (effect.NextTick != 0 && tick >= effect.NextTick && tick < effect.ExpireTick)
                        {
                            effect.OnEffectPulse();
                        }
                        if (effect.IsConcentrationEffect() && tick > effect.NextTick)
                        {
                            //Check if player is too far away from Caster for Concentration buff.
                            if (!effect.SpellHandler.Caster.
                                IsWithinRadius(effect.Owner,
                                effect.SpellHandler.Spell.SpellType != (byte)eSpellType.EnduranceRegenBuff ? ServerProperties.Properties.BUFF_RANGE > 0 ? ServerProperties.Properties.BUFF_RANGE : 5000 : 1500)
                                && !effect.IsDisabled)
                            {
                                ECSGameSpellEffect disabled = null;
                                if (effect.Owner.effectListComponent.GetSpellEffects(effect.EffectType).Count > 1)
                                    disabled = effect.Owner.effectListComponent.GetBestDisabledSpellEffect(effect.EffectType);

                                EffectService.RequestDisableEffect(effect);

                                if (disabled != null)
                                    EffectService.RequestEnableEffect(disabled);
                            }
                            //Check if player is back in range of Caster for Concentration buff.
                            else if (effect.SpellHandler.Caster.IsWithinRadius(effect.Owner,
                                effect.SpellHandler.Spell.SpellType != (byte)eSpellType.EnduranceRegenBuff ? ServerProperties.Properties.BUFF_RANGE > 0 ? ServerProperties.Properties.BUFF_RANGE : 5000 : 1500)
                                && effect.IsDisabled)
                            {
                                //Check if this effect is better than currently enabled effects. Enable this effect and disable other effect if true.
                                ECSGameSpellEffect enabled = null;
                                List<ECSGameEffect> sameEffectTypeEffects;
                                effect.Owner.effectListComponent.Effects.TryGetValue(effect.EffectType, out sameEffectTypeEffects);
                                bool isBest = false;
                                if (sameEffectTypeEffects.Count == 1)
                                    isBest = true;
                                else if (sameEffectTypeEffects.Count > 1)
                                {
                                    foreach (var tmpEff in sameEffectTypeEffects)
                                    {
                                        if (tmpEff is ECSGameSpellEffect eff)
                                        {
                                            //Check only against enabled spells
                                            if (!eff.IsDisabled)
                                            {
                                                enabled = eff;
                                                if (effect.SpellHandler.Spell.Value > eff.SpellHandler.Spell.Value)
                                                {
                                                    isBest = true;
                                                    //break;
                                                }
                                                else
                                                {
                                                    isBest = false;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (isBest)
                                {
                                    EffectService.RequestEnableEffect(effect);
                                    if (enabled != null)
                                    {
                                        EffectService.RequestDisableEffect(enabled);
                                    }
                                }
                     
                            }
                            effect.NextTick = GameLoop.GameLoopTime + effect.PulseFreq;
                        }
                    }
                }
                
            }           
        }

        public static ECSGameEffect GetEffectOnTarget(GameLiving target, eEffect effectType, eSpellType spellType = eSpellType.Null)
        {
            List<ECSGameEffect> effects;

            lock (target.effectListComponent.EffectsLock)
            {
                target.effectListComponent.Effects.TryGetValue(effectType, out effects);
            
                if (effects != null && spellType == eSpellType.Null)
                    return effects.FirstOrDefault();
                else if (effects != null)
                    return effects.OfType<ECSGameSpellEffect>().Where(e => e.SpellHandler.Spell.SpellType == (byte)spellType).FirstOrDefault();
                else
                    return null;
            }
        }

        public static ECSGameSpellEffect GetSpellEffectOnTarget(GameLiving target, eEffect effectType, eSpellType spellType = eSpellType.Null)
        {
            if (target == null) return null;
            List<ECSGameEffect> effects;

            lock (target.effectListComponent.EffectsLock)
            {
                target.effectListComponent.Effects.TryGetValue(effectType, out effects);

                if (effects != null)
                    return effects.OfType<ECSGameSpellEffect>().Where(e => e is ECSGameSpellEffect && (spellType == eSpellType.Null || e.SpellHandler.Spell.SpellType == (byte)spellType)).FirstOrDefault();
                else
                    return null;
            }
        }

        public static ECSGameAbilityEffect GetAbilityEffectOnTarget(GameLiving target, eEffect effectType)
        {
            List<ECSGameEffect> effects;

            lock (target.effectListComponent.EffectsLock)
            {
                target.effectListComponent.Effects.TryGetValue(effectType, out effects);

                if (effects != null)
                    return (ECSGameAbilityEffect)effects.Where(e => e is ECSGameAbilityEffect).FirstOrDefault();
                else
                    return null;
            }
        }

        public static ECSImmunityEffect GetImmunityEffectOnTarget(GameLiving target, eEffect effectType)
        {
            List<ECSGameEffect> effects;

            lock (target.effectListComponent.EffectsLock)
            {
                target.effectListComponent.Effects.TryGetValue(effectType, out effects);

                if (effects != null)
                    return (ECSImmunityEffect)effects.Where(e => e is ECSImmunityEffect).FirstOrDefault();
                else
                    return null;
            }
        }

        public static ECSPulseEffect GetPulseEffectOnTarget(GameLiving target)
        {
            List<ECSGameEffect> effects;

            lock (target.effectListComponent.EffectsLock)
            {
                target.effectListComponent.Effects.TryGetValue(eEffect.Pulse, out effects);

                if (effects != null)
                    return (ECSPulseEffect)effects.Where(e => e is ECSPulseEffect).FirstOrDefault();
                else
                    return null;
            }
        }

        public static bool TryCancelFirstEffectOfTypeOnTarget(GameLiving target, eEffect effectType)
        {
            if (target == null || target.effectListComponent == null)
                return false;

            ECSGameEffect effectToCancel;

            lock (target.effectListComponent.EffectsLock)
            {
                if (!target.effectListComponent.ContainsEffectForEffectType(effectType))
                    return false;

                effectToCancel = GetEffectOnTarget(target, effectType);

                if (effectToCancel == null)
                    return false;

                EffectService.RequestImmediateCancelEffect(effectToCancel);
                return true;
            }
        }
    }
}