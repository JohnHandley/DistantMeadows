using UnityEngine;

using DistantMeadows.Actors.Player.Controllers;
using DistantMeadows.Core.Models;
using DistantMeadows.Actors.Core.Models;
using DistantMeadows.Core.Constants;

namespace DistantMeadows.Actors.Core.Behaviors {
    public class UpdateCharacterMovementAnimation : StateAction {
        private readonly PlayerStateManager player;

        public UpdateCharacterMovementAnimation ( PlayerStateManager playerStateManager ) {
            player = playerStateManager;
        }

        public override bool Execute ( ) {
            player.anim.SetFloat(
                AnimationConstants.AnimatorParamVelocity,
                player.actorMovementData.moveAmount, 0.2f, player.delta
            );

            player.anim.SetFloat(
                AnimationConstants.AnimatorParamSlopeNormal,
                player.actorMovementData.slopeAmount, 0.2f, player.delta
            );

            player.anim.SetFloat(
                AnimationConstants.AnimatorParamSpeedModifier,
                player.actorMovementData.currentSpeed / ( player.actorStats.movementSpeed + player.actorStats.RunSpeedModifier ), 0.2f, player.delta
            );

            player.anim.SetBool(
                AnimationConstants.AnimatorParamIsOnIncline,
                player.actorMovementData.slopeAmount > 0.2 || player.actorMovementData.slopeAmount < -0.2
            );

            player.anim.SetBool(
                AnimationConstants.AnimatorParamOnGround,
                player.actorStates.isGrounded
            );

            player.anim.SetBool(
                AnimationConstants.AnimatorParamRun,
                player.actorStates.isRunning
            );

            player.anim.SetBool(
                AnimationConstants.AnimatorParamIsFacingDrop,
                player.actorStates.isFacingDrop
            );

            player.anim.SetBool(
                AnimationConstants.AnimatorParamIsFacingWall,
                player.actorStates.isFacingWall
            );

            return false;
        }
    }
}
