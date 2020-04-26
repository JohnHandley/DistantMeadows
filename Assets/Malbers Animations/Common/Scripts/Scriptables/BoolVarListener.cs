using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    public class BoolVarListener : MonoBehaviour
    {
        public BoolVar value;
        public UnityEvent OnTrue = new UnityEvent();
        public UnityEvent OnFalse = new UnityEvent();

        void OnEnable()
        {
            value?.OnValueChanged.AddListener(InvokeBool);
        }

        void OnDisable()
        {
            value?.OnValueChanged.RemoveListener(InvokeBool);
        }

        public virtual void InvokeBool(bool value)
        {
            if (value)
                OnTrue.Invoke();
            else
                OnFalse.Invoke();
        }
    }
}
