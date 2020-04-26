using UnityEngine;

namespace DistantMeadows.Actors.Core.Behaviors {
    public class AnimatorHook : MonoBehaviour {
        CharacterStateManager actor;

        public virtual void Init ( CharacterStateManager actorState ) {
            actor = actorState;
        }

        public void OnAnimatorMove ( ) {
            OnAnimatorMoveOverrride();
        }

        protected virtual void OnAnimatorMoveOverrride ( ) {
            if ( actor == null ) {
                return;
            }

            if ( actor.actorStates.useRootMotion == false ) {
                return;
            }

            if ( actor.actorStates.isGrounded && actor.delta > 0 ) {
                Vector3 v = ( actor.anim.deltaPosition ) / actor.delta;
                v.y = actor.rigidbody.velocity.y;
                actor.rigidbody.velocity = v;
            }
        }

    }
}
