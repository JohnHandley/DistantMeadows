using UnityEngine;

using DistantMeadows.Core.Models;

namespace DistantMeadows.Core.Behaviors
{
    public abstract class StateManager : MonoBehaviour
    {
        public StateMachine<EffectCondition> effectStateMachine = EffectsStateMachineFactory.Generate();

        [HideInInspector]
        public Transform mTransform;
        [HideInInspector]
        public float delta;

        private void Start()
        {
            mTransform = this.transform;
            Init();
        }

        public abstract void Init();

        public void FixedTick()
        {
            if (effectStateMachine.GetCurrentState() == null)
                return;
            delta = Time.fixedDeltaTime;
            effectStateMachine.GetCurrentState().FixedTick();
        }

        public void Tick()
        {
            if (effectStateMachine.GetCurrentState() == null)
                return;
            delta = Time.deltaTime;
            effectStateMachine.GetCurrentState().Tick();
        }

        public void LateTick()
        {
            if (effectStateMachine.GetCurrentState() == null)
                return;

            effectStateMachine.GetCurrentState().LateTick();
        }
    }
}
