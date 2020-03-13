using Enemies.Application;
using UnityEngine;

namespace Player.Application
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator anim;
        StateManager states;
        EnemyStates eStates;
        Rigidbody rigid;

        public float rm_multi;
        bool rolling;
        float roll_t;
        float delta;
        AnimationCurve roll_curve;

        public void Init ( StateManager st, EnemyStates eSt )
        {
            states = st;
            if ( states != null )
            {
                anim = st.anim;
                rigid = st.rigid;
                roll_curve = st.roll_curve;
                delta = st.delta;
            }

            eStates = eSt;
            if ( eStates != null )
            {
                anim = eSt.anim;
                rigid = eSt.rigid;
                delta = eSt.delta;
            }
        }

        public void InitForRoll ( )
        {
            rolling = true;
            roll_t = 0;
        }

        public void CloseRoll ( )
        {
            if ( rolling == false )
                return;

            rm_multi = 1;
            roll_t = 0;
            rolling = false;
        }

        private void OnAnimatorMove ( )
        {
            if ( states == null && eStates == null )
                return;

            if ( rigid == null )
            {
                return;
            }

            if ( states != null )
            {
                if ( states.canMove )
                {
                    return;
                }

                delta = states.delta;
            }

            if ( eStates != null )
            {
                if ( eStates.canMove )
                {
                    return;
                }

                delta = eStates.delta;
            }

            rigid.drag = 0;

            if ( rm_multi == 0 )
                rm_multi = 1;



            if ( rolling == false )
            {
                Vector3 animDelta = anim.deltaPosition;
                animDelta.y = 0;
                Vector3 v = ( animDelta * rm_multi ) / delta;
                rigid.velocity = v;
            }
            else
            {
                roll_t += delta / 0.6f;

                if ( roll_t > 1 )
                {
                    roll_t = 1;
                }

                if ( states == null )
                {
                    return;
                }

                float zValue = roll_curve.Evaluate( roll_t );
                Vector3 v1 = Vector3.forward * zValue;
                Vector3 relative = transform.TransformDirection( v1 );
                Vector3 v2 = ( relative * rm_multi );
                rigid.velocity = v2;
            }
        }

        public void OpenDamageColliders ( )
        {
            if ( states )
            {
                states.inventoryManager.OpenAllDamageColliders();
            }

            OpenParryFlag();
        }

        public void CloseDamageColliders ( )
        {
            if ( states )
            {
                states.inventoryManager.CloseAllDamageColliders();
            }

            CloseParryFlag();
        }

        public void OpenParryCollider ( )
        {
            if ( states )
            {
                states.inventoryManager.OpenParryCollider();
            }
        }

        public void CloseParryCollider ( )
        {
            if ( states )
            {
                states.inventoryManager.CloseParryCollider();
            }
        }

        public void OpenParryFlag ( )
        {
            if ( states )
            {
                states.parryIsOn = true;
            }

            if ( eStates )
            {
                eStates.parryIsOn = true;
            }
        }

        public void CloseParryFlag ( )
        {
            if ( states )
            {
                states.parryIsOn = false;
            }

            if ( eStates )
            {
                eStates.parryIsOn = false;
            }
        }
    }
}
