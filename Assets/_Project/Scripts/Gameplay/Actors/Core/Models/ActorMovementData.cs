using UnityEngine;

namespace DistantMeadows.Actors.Core.Models {
    [System.Serializable]
    public class ActorMovementData {
        public float moveAmount;
        public float currentSpeed;
        public Vector3 rotateDirection;
        public Vector3 rootMovement;
        public Vector3 moveDirection;
        public float floorOffsetY;
        public Vector3 raycastFloorPos;
        public Vector3 floorMovement;
        public Vector3 gravity;
        public Vector3 CombinedRaycast;
        public float slopeAmount;
        public Vector3 floorNormal;
    }
}



