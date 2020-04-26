using UnityEngine;

using DistantMeadows.Actors.Enemies.Behaviors;

namespace DistantMeadows.Items.Behaviors
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter ( Collider other )
        {
            EnemyStates eStates = other.transform.GetComponentInParent<EnemyStates>();
            if ( eStates == null )
            {
                return;
            }

            // Do Damage
            eStates.DoDamage( 35 );
        }
    }
}