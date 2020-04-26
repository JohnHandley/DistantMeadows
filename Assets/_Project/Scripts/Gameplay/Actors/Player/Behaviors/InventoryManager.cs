using UnityEngine;

using DistantMeadows.Core.Constants;
using DistantMeadows.Items.Behaviors;
using DistantMeadows.Items.Models;
using DistantMeadows.UI.Behaviors;
using DistantMeadows.UI.Enums;

namespace DistantMeadows.Actors.Player.Behaviors
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

            QuickSlotManager uiSlotManager = QuickSlotManager.singleton;
            if ( uiSlotManager != null )
            {
                uiSlotManager.UpdateSlot(
                    ( isLeftHand ) ? QuickSlotType.LeftHand : QuickSlotType.RightHand,
                    weapon.icon
                );
            }

            string targetIdle = weapon.oh_idle;
            targetIdle += isLeftHand ? "_l" : "_r";

            states.anim.SetBool( AnimationConstants.AnimatorParamMirror, isLeftHand );
            states.anim.Play( AnimationConstants.AnimationChangeWeapon );
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


