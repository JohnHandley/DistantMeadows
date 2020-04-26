using UnityEngine;

using DistantMeadows.Actors.Core.Controllers;

namespace DistantMeadows.Actors.Core.Models {
    [CreateAssetMenu( menuName = "Abilities/RaycastAbility" )]
    public class RaycastAbility : Ability {

        public int damage = 1;
        public float range = 50f;
        public float hitForce = 100f;
        public Color laserColor = Color.white;

        private RaycastAbilityTriggerable trigger;

        public override void Initialize ( GameObject obj ) {
            trigger = obj.GetComponent<RaycastAbilityTriggerable>();
            trigger.Initialize();

            trigger.damage = damage;
            trigger.range = range;
            trigger.hitForce = hitForce;
            trigger.laserLine.material = new Material( Shader.Find( "Unlit/Color" ) );
            trigger.laserLine.material.color = laserColor;
        }

        public override void TriggerAbility ( ) {
            trigger.Fire();
        }
    }
}
