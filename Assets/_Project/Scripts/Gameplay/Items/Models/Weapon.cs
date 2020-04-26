using System.Collections.Generic;
using UnityEngine;

using DistantMeadows.Actors.Core.Enums;
using DistantMeadows.Actors.Core.Models;
using DistantMeadows.Items.Behaviors;

namespace DistantMeadows.Items.Models
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponId;
        public Sprite icon;
        public string oh_idle;
        public string th_idle;

        public List<ActorAction> oh_actions;
        public List<ActorAction> th_actions;
        public bool LeftHandMirror;

        public GameObject weaponModel;
        public WeaponHook weaponHook;

        public ActorAction GetAction ( ActorActionInput input, bool isBeingTwoHanded )
        {
            List<ActorAction> actions = isBeingTwoHanded ? th_actions : oh_actions;

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