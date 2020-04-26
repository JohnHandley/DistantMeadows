using DistantMeadows.Core.Constants;
using UnityEngine;

namespace DistantMeadows.Core.Utilities
{
    public class Helper : MonoBehaviour
    {
        [Range( -1, 1 )]
        public float vertical;

        [Range( -1, 1 )]
        public float horizontal;

        public bool playAnim;

        public string[] oh_attacks;
        public string[] th_attacks;

        public bool twoHanded;
        public bool enableRM;
        public bool useItem;
        public bool interacting;
        public bool lockon;

        Animator anim;

        void Start ( )
        {
            anim = GetComponent<Animator>();
        }

        void Update ( )
        {
            enableRM = !anim.GetBool( AnimationConstants.AnimatorParamCanMove );
            anim.applyRootMotion = enableRM;

            interacting = anim.GetBool( AnimationConstants.AnimatorParamInteracting );

            if ( !lockon )
            {
                horizontal = 0;
                vertical = Mathf.Clamp01( vertical );
            }

            anim.SetBool( "lockon", lockon );

            if ( enableRM )
            {
                return;
            }

            if ( useItem )
            {
                anim.Play( "use_item" );
                useItem = false;
            }

            if ( interacting )
            {
                playAnim = false;
                vertical = Mathf.Clamp( vertical, 0, 0.5f );
            }

            anim.SetBool( AnimationConstants.AnimatorParamTwoHanded, twoHanded );

            if ( playAnim )
            {
                string targetAnim;

                if ( twoHanded )
                {
                    int r = Random.Range( 0, th_attacks.Length );
                    targetAnim = th_attacks[ r ];
                }
                else
                {
                    int r = Random.Range( 0, oh_attacks.Length );
                    targetAnim = oh_attacks[ r ];

                    if ( vertical > 0.5f )
                    {
                        targetAnim = AnimationConstants.AnimationOneHandAttack3;
                    }
                }

                vertical = 0f;
                anim.CrossFade( targetAnim, 0.2f );
                playAnim = false;
            }

            anim.SetFloat( AnimationConstants.AnimatorParamVertical, vertical );
            anim.SetFloat( AnimationConstants.AnimatorParamHorizontal, horizontal );
        }
    }
}