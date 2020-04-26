namespace DistantMeadows.Core.Constants {
    public static class AnimationConstants {
        #region Animator Parameters
        public static string AnimatorParamVertical => "vertical";
        public static string AnimatorParamHorizontal => "horizontal";
        public static string AnimatorParamTwoHanded => "IsTwoHanding";
        public static string AnimatorParamCanMove => "IsAbleToMove";
        public static string AnimatorParamInteracting => "IsInteracting";
        public static string AnimatorParamLockOn => "IsLockedOn";
        public static string AnimatorParamOnGround => "IsOnGround";
        public static string AnimatorParamRun => "IsRunning";
        public static string AnimatorParamMirror => "mirror";
        public static string AnimatorParamBlocking => "IsBlocking";
        public static string AnimatorParamIsLeft => "IsLeftHanding";
        public static string AnimatorParamAnimSpeed => "animSpeed";
        public static string AnimatorParamIsDead => "IsDead";
        public static string AnimatorParamVelocity => "Velocity";
        public static string AnimatorParamSlopeNormal => "SlopeNormal";
        public static string AnimatorParamIsFacingWall => "IsFacingWall";
        public static string AnimatorParamIsFacingDrop => "IsFacingDrop";
        public static string AnimatorParamSpeedModifier => "SpeedModifier";
        public static string AnimatorParamIsOnIncline => "IsOnIncline";
        #endregion

        #region Animations
        public static string AnimationOneHandAttack1 => "oh_attack_1";
        public static string AnimationOneHandAttack2 => "oh_attack_2";
        public static string AnimationOneHandAttack3 => "oh_attack_3";

        public static string AnimationTwoHandAttack1 => "th_attack_1";
        public static string AnimationTwoHandAttack2 => "th_attack_2";

        public static string AnimationParry => "parry";
        public static string AnimationParryAttack => "parry_attack";
        public static string AnimationParryRecieved => "parry_recieved";
        public static string AnimationAttackIterrupt => "attack_interrupt";

        public static string AnimationBackStabRecieved => "backstab_recieved";

        public static string AnimationDamage1 => "damage_1";
        public static string AnimationDamage2 => "damage_2";
        public static string AnimationDamage3 => "damage_3";

        public static string AnimationRolls => "Rolls";

        public static string AnimationChangeWeapon => "changeWeapon";
        public static string AnimationUseItem => "use_item";
        public static string AnimationEstus => "bestus";

        public static string AnimationShieldLeftHand => "shield_l";
        public static string AnimationShieldRightHand => "shield_r";

        public static string AnimationTwoHandIdle => "th_idle";
        public static string AnimationOneHandIdleLeft => "oh_idle_l";
        public static string AnimationOneHandIdleRight => "oh_idle_r";
        #endregion
    }
}
