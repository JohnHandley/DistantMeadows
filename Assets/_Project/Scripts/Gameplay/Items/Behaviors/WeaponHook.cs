﻿using UnityEngine;

namespace DistantMeadows.Items.Behaviors
{
    public class WeaponHook : MonoBehaviour
    {
        public GameObject[] damageCollider;

        public void OpenDamageColliders ( )
        {
            for ( int i = 0; i < damageCollider.Length; i++ )
            {
                damageCollider[ i ].SetActive( true );
            }
        }

        public void CloseDamageColliders ( )
        {
            for ( int i = 0; i < damageCollider.Length; i++ )
            {
                damageCollider[ i ].SetActive( false );
            }
        }
    }
}


