using System.Collections.Generic;
using Items.Application;
using Player.Domain;
using UnityEngine;

namespace Items.Domain
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponId;
        public Sprite icon;
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