using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SA
{
    public class InventoryManager : MonoBehaviour
    {
        public Weapon rightHandWeapon;
        public Weapon leftHandWeapon;

        StateManager states;

        public void Init ( StateManager st )
        {
            states = st;
            EquipWeapon( rightHandWeapon, false );
            EquipWeapon( leftHandWeapon, true );
            CloseAllDamageColliders();
        }

        public void EquipWeapon ( Weapon w, bool isLeftHand )
        {
            if ( !WeaponIsSafeToUse( w ) )
            {
                return;
            }

            string targetIdle = w.oh_idle;
            targetIdle += isLeftHand ? "_l" : "_r";

            states.anim.SetBool( "mirror", isLeftHand );
            states.anim.Play( "changeWeapon" );
            states.anim.Play( targetIdle );
        }

        public void OpenAllDamageColliders ( )
        {
            if ( HasWeaponInRightHand() )
            {
                rightHandWeapon.weaponHook.OpenDamageColliders();
            }

            if ( HasWeaponInLeftHand() )
            {
                leftHandWeapon.weaponHook.OpenDamageColliders();
            }
        }

        public void CloseAllDamageColliders ( )
        {
            if ( HasWeaponInRightHand() )
            {
                rightHandWeapon.weaponHook.CloseDamageColliders();
            }

            if ( HasWeaponInLeftHand() )
            {
                leftHandWeapon.weaponHook.CloseDamageColliders();
            }
        }

        public bool WeaponIsSafeToUse ( Weapon w )
        {
            return w != null
                && w.weaponHook != null
                && w.weaponHook.isActiveAndEnabled;
        }

        public bool HasWeaponInLeftHand ( )
        {
            return WeaponIsSafeToUse( leftHandWeapon );
        }

        public bool HasWeaponInRightHand ( )
        {
            return WeaponIsSafeToUse( rightHandWeapon );
        }
    }

    [System.Serializable]
    public class Weapon
    {
        public string oh_idle;
        public string th_idle;

        public List<Action> oh_actions;
        public List<Action> th_actions;
        public bool LeftHandMirror;

        public GameObject weaponModel;
        public WeaponHook weaponHook;

        public Action GetAction ( ActionInput input, bool isBeingTwoHanded )
        {
            List<Action> actions = isBeingTwoHanded ? th_actions : oh_actions;

            for ( int i = 0; i < actions.Count; i++ )
            {
                if ( actions[ i ].input == input )
                {
                    return actions[ i ];
                }
            }

            return null;
        }
    }
}


