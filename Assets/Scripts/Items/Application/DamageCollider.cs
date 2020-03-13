using Enemies.Application;
using UnityEngine;

namespace Items.Application
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