using System;
using System.Collections.Generic;
using DistantMeadows.Core.Effects;

namespace DistantMeadows.Core.Models
{
    public enum EffectCondition
    {
        BeingBurnt,
        BeingFrozen,
        BeingUnFrozen,
        BeingElectrified,
        TouchingWater,
        Nuetral,
        Reset,
    }

    public static class EffectsStateMachineFactory
    {
        public static StateMachine<EffectCondition> Generate()
        {
            State normal = EffectStatesFactory.GenerateNormalEffectState();
            State burning = EffectStatesFactory.GenerateBurningEffectState();
            State burnt = EffectStatesFactory.GenerateBurntEffectState();
            State freezing = EffectStatesFactory.GenerateFreezingEffectState();
            State frozen = EffectStatesFactory.GenerateFrozenEffectState();
            State electrified = EffectStatesFactory.GenerateElectrifiedEffectState();

            Condition<EffectCondition> beingBurnt = new Condition<EffectCondition>(EffectCondition.BeingBurnt);
            Condition<EffectCondition> beingFrozen = new Condition<EffectCondition>(EffectCondition.BeingFrozen);
            Condition<EffectCondition> beingUnFrozen = new Condition<EffectCondition>(EffectCondition.BeingUnFrozen);
            Condition<EffectCondition> beingElectrified = new Condition<EffectCondition>(EffectCondition.BeingElectrified);
            Condition<EffectCondition> touchingWater = new Condition<EffectCondition>(EffectCondition.TouchingWater);
            Condition<EffectCondition> nuetral = new Condition<EffectCondition>(EffectCondition.Nuetral);
            Condition<EffectCondition> reset = new Condition<EffectCondition>(EffectCondition.Reset);

            List<StateTransition<EffectCondition>> effectTransitions = new List<StateTransition<EffectCondition>> {
                new StateTransition<EffectCondition>(
                    "NormalToElectrified",
                    normal,
                    electrified,
                    new List<Condition<EffectCondition>> { beingElectrified }
                ),
                new StateTransition<EffectCondition>(
                    "NormalToBurning",
                    normal,
                    burning,
                    new List<Condition<EffectCondition>> { beingBurnt }
                ),
                new StateTransition<EffectCondition>(
                    "NormalToFreezing",
                    normal,
                    freezing,
                    new List<Condition<EffectCondition>> { beingFrozen }
                ),
                new StateTransition<EffectCondition>(
                    "BurningToBurnt",
                    burning,
                    burnt,
                    new List<Condition<EffectCondition>> { beingBurnt }
                ),
                new StateTransition<EffectCondition>(
                    "BurningToNormal",
                    burning,
                    normal,
                    new List<Condition<EffectCondition>> { touchingWater, beingFrozen, reset }
                ),
                new StateTransition<EffectCondition>(
                    "BurntToNormal",
                    burnt,
                    normal,
                    new List<Condition<EffectCondition>> { reset }
                ),
                new StateTransition<EffectCondition>(
                    "FreezingToNormal",
                    freezing,
                    normal,
                    new List<Condition<EffectCondition>> { nuetral, reset }
                ),
                new StateTransition<EffectCondition>(
                    "FrozenToNormal",
                    frozen,
                    normal,
                    new List<Condition<EffectCondition>> { beingBurnt, beingUnFrozen, reset }
                ),
                new StateTransition<EffectCondition>(
                    "ElectrifiedToNormal",
                    electrified,
                    normal,
                    new List<Condition<EffectCondition>> { nuetral, reset }
                )
            };

            StateMachine<EffectCondition> newStateMachine = new StateMachine<EffectCondition>(
                "EffectState",
                normal,
                effectTransitions
            );

            return newStateMachine;
        }
    }

}
