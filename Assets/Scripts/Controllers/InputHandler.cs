using UnityEngine;

namespace SA
{
    public class InputHandler : MonoBehaviour
    {
        Controls cont;

        float delta;

        StateManager states;
        CameraManager camManager;

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Init();

            camManager = CameraManager.singleton;
            camManager.Init(states);
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick(Time.deltaTime);
            camManager.Tick(delta, cont);
        }

        void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
        }

        void GetInput()
        {
            cont = MouseAndKeyboard.GetControls();
        }

        void UpdateStates()
        {
            states.cont = cont;

            Vector3 v = cont.Vertical * camManager.transform.forward;
            Vector3 h = cont.Horizontal * camManager.transform.right;
            states.moveDir = (v + h).normalized;
            float m = Mathf.Abs(cont.Horizontal) + Mathf.Abs(cont.Vertical);
            states.moveAmount = Mathf.Clamp01(m);

            if(cont.Sprint)
            {
                // states.run = (states.moveAmount > 0);
            }
            else
            {
                // states.run = false;
            }

            if(cont.TwoHand)
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            if(cont.Lockon)
            {
                states.lockOn = !states.lockOn;
                if (states.lockOnTarget == null)
                {
                    states.lockOn = false;
                }

                camManager.lockonTarget = states.lockOnTarget;
                states.lockOnTransform = camManager.lockonTransform;
                camManager.lockon = states.lockOn;
            }
        }
    }

}
