using UnityEngine;

namespace DistantMeadows.Actors.Core.Controllers {
    public class ProjectileAbilityTriggerable : MonoBehaviour {

        // Rigidbody variable to hold a reference to our projectile prefab
        [HideInInspector] public Rigidbody projectile;

        // Transform variable to hold the location where we will spawn our projectile
        public Transform projectileSpawn;

        // Float variable to hold the amount of force which we will apply to launch our projectiles
        [HideInInspector] public float projectileForce = 250f;

        public void Launch ( ) {
            //Instantiate a copy of our projectile and store it in a new rigidbody variable called clonedBullet
            Rigidbody clonedProjectile = Instantiate( projectile, projectileSpawn.position, transform.rotation ) as Rigidbody;

            //Add force to the instantiated bullet, pushing it forward away from the bulletSpawn location, using projectile force for how hard to push it away
            clonedProjectile.AddForce( projectileSpawn.transform.forward * projectileForce );
        }
    }
}
