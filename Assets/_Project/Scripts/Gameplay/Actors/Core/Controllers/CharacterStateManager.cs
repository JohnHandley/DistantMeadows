using UnityEngine;

using DistantMeadows.Core.Behaviors;
using DistantMeadows.Actors.Core.Models;

namespace DistantMeadows.Actors.Core.Behaviors {
    public abstract class CharacterStateManager : StateManager {
        [Header( "References" )]
        public Animator anim;
        // public Renderer eyesRenderer;
        // public Renderer mouthRenderer;
        public new Rigidbody rigidbody;
        public AnimatorHook animHook;
        public CharacterDialogueHandler dialogueHandler;

        [Header( "States" )]
        public ActorStates actorStates = new ActorStates();

        [Header( "Stats" )]
        public ActorStatsData actorStats;

        [Header( "Game Information" )]
        public CharacterData characterInfo;

        [Header( "Movement Data" )]
        public ActorMovementData actorMovementData = new ActorMovementData();

        public LayerMask ignoreForGroundCheck;

        public override void Init ( ) {
            if ( anim == null ) {
                anim = GetComponentInChildren<Animator>();
            }
            if ( animHook == null ) {
                animHook = GetComponentInChildren<AnimatorHook>();
            }
            if ( rigidbody == null ) {
                rigidbody = GetComponentInChildren<Rigidbody>();
            }
            if ( dialogueHandler == null ) {
                dialogueHandler = new CharacterDialogueHandler();
            }

            anim.applyRootMotion = false;
            animHook.Init( this );
            dialogueHandler.Init( this );
            ignoreForGroundCheck = LayerMask.GetMask( "Ground", "Default" );
        }

        public new void Tick ( ) {
            base.Tick();
            dialogueHandler.Update();
        }

        public void PlayTargetAnimation ( string targetAnim, bool isInteracting ) {
            anim.SetBool( "isInteracting", isInteracting );
            anim.CrossFade( targetAnim, 0.2f );
        }

        private void OnDestroy ( ) {
            dialogueHandler.Destroy();
        }
    }
}
