using UI.Application;
using UnityEngine;

namespace Player.Application
{
    public class InputHandler : MonoBehaviour
    {
        float vertical;
        float horizontal;
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        bool rb_input;
        float rt_axis;
        bool rt_input;
        bool lb_input;
        float lt_axis;
        bool lt_input;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float lt_timer;
        float rt_timer;

        StateManager states;
        CameraManager camManager;

        float delta;

        void Start ( )
        {
            QuickSlotManager.singleton.Init();

            states = GetComponent<StateManager>();
            states.Init();

            camManager = CameraManager.singleton;
            camManager.Init( states );
        }

        void FixedUpdate ( )
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick( delta );
            camManager.Tick( delta );
        }

        void Update ( )
        {
            RunImmediateChecks();
            delta = Time.deltaTime;
            states.Tick( delta );
            ResetInputAndStates();
        }

        void GetInput ( )
        {
            vertical = Input.GetAxis( "Vertical" );
            horizontal = Input.GetAxis( "Horizontal" );
            b_input = Input.GetButton( "B" );
            a_input = Input.GetButton( "A" );

            x_input = Input.GetButton( "X" );
            rt_input = Input.GetButton( "RT" );
            rt_axis = Input.GetAxis( "RT" );
            if ( rt_axis != 0 )
                rt_input = true;

            lt_input = Input.GetButton( "LT" );
            lt_axis = Input.GetAxis( "LT" );
            if ( lt_axis != 0 )
                lt_input = true;
            rb_input = Input.GetButton( "RB" );
            lb_input = Input.GetButton( "LB" );

            if ( b_input )
            {
                b_timer += delta;
            }
        }

        void UpdateStates ( )
        {
            if ( camManager == null )
            {
                throw new System.Exception( "Camera Manager on Input Handler Null" );
            }

            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = vertical * camManager.transform.forward;
            Vector3 h = horizontal * camManager.transform.right;
            states.moveDir = ( v + h ).normalized;
            float m = Mathf.Abs( horizontal ) + Mathf.Abs( vertical );
            states.moveAmount = Mathf.Clamp01( m );

            if ( x_input )
                b_input = false;

            if ( b_input && b_timer > 0.5f )
            {
                states.run = ( states.moveAmount > 0 );
            }

            if ( !b_input && b_timer > 0 && b_timer < 0.5f )
            {
                states.rollInput = true;
            }

            states.itemInput = x_input;
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;
        }

        void RunImmediateChecks ( )
        {
            leftAxis_down = Input.GetButtonUp( "L" );
            rightAxis_down = Input.GetButtonUp( "R" );
            y_input = Input.GetButtonUp( "Y" );
            if ( y_input )
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            if ( states.lockOnTarget != null )
            {
                if ( states.lockOnTarget.eStates.isDead )
                {
                    states.lockOn = false;
                    states.lockOnTarget = null;
                    states.lockOnTransform = null;
                    camManager.lockon = false;
                    camManager.lockonTarget = null;
                }
            }


            if ( rightAxis_down )
            {
                states.lockOn = !states.lockOn;

                if ( states.lockOnTarget == null )
                    states.lockOn = false;

                camManager.lockonTarget = states.lockOnTarget;
                states.lockOnTransform = camManager.lockonTransform;
                camManager.lockon = states.lockOn;
            }
        }

        void ResetInputAndStates ( )
        {
            if ( b_input == false )
            {
                b_timer = 0;
            }

            if ( states.rollInput )
            {
                states.rollInput = false;
            }

            if ( states.run )
            {
                states.run = false;
            }
        }
    }
}
