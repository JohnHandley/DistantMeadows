using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations
{
    public class FloatVarListener : MonoBehaviour
    {
        public FloatVar value;
        public FloatEvent Raise = new FloatEvent();

        void OnEnable()
        {
            value?.OnValueChanged.AddListener(InvokeFloat);
            Raise.Invoke(value ?? 0f);
        }

        void OnDisable()
        {
            value?.OnValueChanged.RemoveListener(InvokeFloat);
        }

        public virtual void InvokeFloat(float value)   {  Raise.Invoke(value);}
    }
}