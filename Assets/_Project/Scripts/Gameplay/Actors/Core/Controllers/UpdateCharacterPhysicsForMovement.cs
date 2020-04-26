using UnityEngine;

using DistantMeadows.Core.Models;
using DistantMeadows.Actors.Player.Controllers;
using DistantMeadows.Actors.Core.Models;

namespace DistantMeadows.Actors.Core.Behaviors {
    public class UpdateCharacterPhysicsForMovement : StateAction {
        private readonly PlayerStateManager player;

        public UpdateCharacterPhysicsForMovement ( PlayerStateManager playerStateManager ) {
            player = playerStateManager;
        }

        public override bool Execute ( ) {
            ActorMovementData movement = player.actorMovementData;
            ActorStatsData stats = player.actorStats;

            player.actorStates.isGrounded = IsGrounded();

            // if not grounded , increase down force
            if ( !player.actorStates.isGrounded || movement.slopeAmount >= 0.1f )// if going down, also apply, to stop bouncing
            {
                movement.gravity += Vector3.up * Physics.gravity.y * stats.jumpFalloff * Time.fixedDeltaTime;
            }

            // actual movement of the rigidbody + extra down force
            player.rigidbody.velocity = ( movement.moveDirection * GetMoveSpeed() * movement.moveAmount ) + movement.gravity;

            // find the Y position via raycasts
            movement.floorMovement = new Vector3(
                player.rigidbody.position.x,
                FindFloor().y + movement.floorOffsetY,
                player.rigidbody.position.z
            );

            // only stick to floor when grounded
            if ( movement.floorMovement != player.rigidbody.position && player.actorStates.isGrounded && player.rigidbody.velocity.y <= 0 ) {
                // move the rigidbody to the floor
                player.rigidbody.MovePosition( movement.floorMovement );
                movement.gravity.y = 0;
            }

            return false;
        }

        bool IsGrounded ( ) {
            ActorMovementData movement = player.actorMovementData;
            if ( FloorRaycasts( 0, 0, 0.6f ) != Vector3.zero ) {
                movement.slopeAmount = Vector3.Dot( player.mTransform.forward, movement.floorNormal );
                return true;
            } else {
                return false;
            }
        }

        Vector3 FindFloor ( ) {
            ActorMovementData movement = player.actorMovementData;
            // width of raycasts around the centre of your character
            float raycastWidth = 0.25f;
            // check floor on 5 raycasts, get the average when not Vector3.zero  
            int floorAverage = 1;

            movement.CombinedRaycast = FloorRaycasts( 0, 0, 1.6f );
            floorAverage += (
                getFloorAverage( raycastWidth, 0 )
                + getFloorAverage( -raycastWidth, 0 )
                + getFloorAverage( 0, raycastWidth )
                + getFloorAverage( 0, -raycastWidth )
            );

            return movement.CombinedRaycast / floorAverage;
        }

        // only add to average floor position if its not Vector3.zero
        int getFloorAverage ( float offsetx, float offsetz ) {
            ActorMovementData movement = player.actorMovementData;
            if ( FloorRaycasts( offsetx, offsetz, 1.6f ) != Vector3.zero ) {
                movement.CombinedRaycast += FloorRaycasts( offsetx, offsetz, 1.6f );
                return 1;
            } else {
                return 0;
            }
        }

        Vector3 FloorRaycasts ( float offsetx, float offsetz, float raycastLength ) {
            ActorMovementData movement = player.actorMovementData;
            RaycastHit hit;
            // move raycast
            movement.raycastFloorPos = player.mTransform.TransformPoint( 0 + offsetx, 0 + 0.5f, 0 + offsetz );

            Debug.DrawRay( movement.raycastFloorPos, Vector3.down, Color.magenta );
            if ( Physics.Raycast( movement.raycastFloorPos, -Vector3.up, out hit, raycastLength ) ) {
                movement.floorNormal = hit.normal;
                float obsticalAngle = Vector3.Angle( movement.floorNormal, Vector3.up );
                if ( obsticalAngle < player.actorStats.slopeLimit ) {
                    player.actorStates.isFacingWall = false;
                    return hit.point;
                } else
                    player.actorStates.isFacingWall = true;
                return Vector3.zero;
            } else
                return Vector3.zero;
        }

        float GetMoveSpeed ( ) {
            ActorMovementData movement = player.actorMovementData;

            float targetSpeed = player.actorStats.movementSpeed;
            if ( player.actorStates.isRunning ) {
                targetSpeed += player.actorStats.RunSpeedModifier;
            }

            // you can add a run here, if run button : currentMovespeed = runSpeed;
            float currentMovespeed =
                Mathf.Clamp(
                    targetSpeed + ( movement.slopeAmount * player.actorStats.slopeInfluence ),
                    0,
                    targetSpeed + 1
                );

            movement.currentSpeed = currentMovespeed;

            return currentMovespeed;
        }
    }
}


