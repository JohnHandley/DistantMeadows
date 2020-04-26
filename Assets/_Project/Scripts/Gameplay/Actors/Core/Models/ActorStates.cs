namespace DistantMeadows.Actors.Core.Models {
    [System.Serializable]
    public class ActorStates {
        public bool isGrounded = true;
        public bool isLockedOn = false;
        public bool isInteracting = false;
        public bool isAbleToCombo = true;
        public bool isAttacking = false;
        public bool isFacingWall = false;
        public bool isFacingDrop = false;

        #region Movement
        public bool isAbleToRotate = true;
        public bool isAbleToMove = true;
        public bool isRunning = false;
        public bool isRolling = false;
        public bool useRootMotion = false;
        #endregion

    }
}