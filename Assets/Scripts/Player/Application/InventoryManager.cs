using UnityEngine;
using Player.Domain;
using Items.Application;

namespace Player.Application
{
    public class InventoryManager : MonoBehaviour
    {
        public Weapon rightHandWeapon;
        public Weapon leftHandWeapon;
        public GameObject parryCollider;

        StateManager states;

        public void Init ( StateManager st )
        {
            states = st;
            EquipWeapon( rightHandWeapon, false );
            EquipWeapon( leftHandWeapon, true );
            CloseAllDamageColliders();

            ParryCollider pr = parryCollider.GetComponent<ParryCollider>();
            pr.InitPlayer( states );
            CloseParryCollider();
        }

        public void EquipWeapon ( Weapon weapon, bool isLeftHand )
        {
            if ( !WeaponIsSafeToUse( weapon ) )
            {
                return;
            }

            string targetIdle = weapon.oh_idle;
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

        public bool WeaponIsSafeToUse ( Weapon weapon )
        {
            return weapon != null
                && weapon.weaponHook != null
                && weapon.weaponHook.isActiveAndEnabled;
        }

        public bool HasWeaponInLeftHand ( )
        {
            return WeaponIsSafeToUse( leftHandWeapon );
        }

        public bool HasWeaponInRightHand ( )
        {
            return WeaponIsSafeToUse( rightHandWeapon );
        }

        public void CloseParryCollider ( )
        {
            parryCollider.SetActive( false );
        }

        public void OpenParryCollider ( )
        {
            parryCollider.SetActive( true );
        }
    }
}


