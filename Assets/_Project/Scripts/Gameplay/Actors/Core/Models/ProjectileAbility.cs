using UnityEngine;

using DistantMeadows.Actors.Core.Controllers;

namespace DistantMeadows.Actors.Core.Models {
    [CreateAssetMenu( menuName = "Abilities/ProjectileAbility" )]
    public class ProjectileAbility : Ability {
        public float projectileForce = 500f;
        public Rigidbody projectile;

        private ProjectileAbilityTriggerable launcher;

        public override void Initialize ( GameObject obj ) {
            launcher = obj.GetComponent<ProjectileAbilityTriggerable>();
            launcher.projectileForce = projectileForce;
            launcher.projectile = projectile;
        }

        public override void TriggerAbility ( ) {
            launcher.Launch();
        }
    }
}

