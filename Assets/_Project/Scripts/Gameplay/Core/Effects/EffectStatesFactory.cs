using DistantMeadows.Core.Models;

namespace DistantMeadows.Core.Effects
{
    public static class EffectStatesFactory
    {
        public static State GenerateNormalEffectState()
        {
            return new State("Normal", null, null, null);
        }

        public static State GenerateElectrifiedEffectState()
        {
            return new State("Electrified", null, null, null);
        }

        public static State GenerateBurningEffectState()
        {
            return new State("Burning", null, null, null);
        }

        public static State GenerateBurntEffectState()
        {
            return new State("Burnt", null, null, null);
        }

        public static State GenerateFreezingEffectState()
        {
            return new State("Freezing", null, null, null);
        }

        public static State GenerateFrozenEffectState()
        {
            return new State("Frozen", null, null, null);
        }
    }
}
