using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SA
{
    public class InventoryManager : MonoBehaviour
    {
        public Weapon curWeapon;

        public void Init ( )
        {
            Debug.Log( "Closing Damage Collider on " + curWeapon.weaponHook.name );
            curWeapon.weaponHook.CloseDamageColliders();
        }
    }

    [System.Serializable]
    public class Weapon
    {
        public List<Action> oh_actions;
        public List<Action> th_actions;
        public GameObject weaponModel;
        public WeaponHook weaponHook;
    }
}


