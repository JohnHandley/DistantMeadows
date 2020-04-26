using UnityEngine;

using DistantMeadows.Core.Models;
using DistantMeadows.Actors.Core.Models;

namespace DistantMeadows.Actors.Player.Controllers {
    public class MovePlayerCharacter : StateAction {
        private readonly PlayerStateManager player;

        public MovePlayerCharacter ( PlayerStateManager playerStateManager ) {
            player = playerStateManager;
        }

        public override bool Execute ( ) {
            ActorMovementData movement = player.actorMovementData;
            ActorStatsData stats = player.actorStats;

            // reset movement
            movement.moveDirection = Vector3.zero;
            player.actorStates.isRunning = false;

            // base movement on camera
            Vector3 correctedVertical = player.controls.Move.y * player.cam.transform.forward;
            Vector3 correctedHorizontal = player.controls.Move.x * player.cam.transform.right;

            Vector3 combinedInput = correctedHorizontal + correctedVertical;
            // normalize so diagonal movement isnt twice as fast, clear the Y so your character doesnt try to
            // walk into the floor/ sky when your camera isn't level

            movement.moveDirection = new Vector3( ( combinedInput ).normalized.x, 0, ( combinedInput ).normalized.z );

            // make sure the input doesnt go negative or above 1;
            float inputMagnitude = Mathf.Abs( player.controls.Move.x ) + Mathf.Abs( player.controls.Move.y );
            movement.moveAmount = player.actorStats.locomotionSpeedCurve.Evaluate( Mathf.Clamp01( inputMagnitude ) );

            // rotate player to movement direction
            if ( movement.moveDirection != Vector3.zero ) {
                Quaternion rot = Quaternion.LookRotation( movement.moveDirection );
                Quaternion targetRotation = Quaternion.Slerp( player.mTransform.rotation, rot, Time.fixedDeltaTime * movement.moveAmount * stats.rotateSpeed );
                player.mTransform.rotation = targetRotation;
            }

            if ( player.controls.InteractOrPickup ) {
                //Jump();
                player.dialogueHandler.RequestConversation();
            }

            if ( player.controls.Sprint ) {
                Run();
            }


            return false;
        }

        void Jump ( ) {
            if ( player.actorStates.isGrounded ) {
                player.actorMovementData.gravity.y = player.actorStats.jumpPower;
                player.anim.SetTrigger( "Jumping" );
            }
        }

        void Run ( ) {
            if ( player.actorStates.isGrounded && player.actorMovementData.moveAmount > .4f ) {
                player.actorStates.isRunning = true;
            }
        }
    }
}
