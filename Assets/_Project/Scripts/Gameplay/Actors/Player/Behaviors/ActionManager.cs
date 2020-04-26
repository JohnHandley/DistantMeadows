using UnityEngine;
using System.Collections.Generic;

using DistantMeadows.Items.Models;
using DistantMeadows.Actors.Core.Models;
using DistantMeadows.Actors.Core.Enums;

namespace DistantMeadows.Actors.Player.Behaviors
{
    public class ActionManager : MonoBehaviour
    {
        public List<ActorAction> actionSlots = new List<ActorAction>();
        public ItemAction consumableItem;

        public StateManager states;

        ActionManager ( )
        {
            for ( int i = 0; i < 4; i++ )
            {
                ActorAction a = new ActorAction();
                a.input = (ActorActionInput) i;
                actionSlots.Add( a );
            }
        }

        public void Init ( StateManager st )
        {
            states = st;

            UpdateActionsWithCurrentWeapon( false );
        }

        public void UpdateActionsWithCurrentWeapon ( bool twoHanded )
        {
            EmptyAllSlots();

            if ( states.inventoryManager.HasWeaponInLeftHand() )
            {
                UpdateActionsDuelWielding();
                return;
            }

            Weapon currentWeapon = states.inventoryManager.rightHandWeapon;

            List<ActorAction> weaponActions = twoHanded ? currentWeapon.th_actions : currentWeapon.oh_actions;

            for ( int i = 0; i < weaponActions.Count; i++ )
            {
                SetActionForInput( weaponActions[ i ].input, weaponActions[ i ] );
            }
        }

        private void UpdateActionsDuelWielding ( )
        {
            InventoryManager playerInventory = states.inventoryManager;
            Weapon currentRightWeapon = playerInventory.rightHandWeapon;
            Weapon currentLeftWeapon = playerInventory.leftHandWeapon;

            SetActionForInput(
                ActorActionInput.rb,
                currentRightWeapon.GetAction( ActorActionInput.rb, false )
            );
            SetActionForInput(
                ActorActionInput.rt,
                currentRightWeapon.GetAction( ActorActionInput.rt, false )
            );
            SetActionForInput(
                ActorActionInput.lb,
                currentLeftWeapon.GetAction( ActorActionInput.rb, false )
            );
            SetActionForInput(
                ActorActionInput.lt,
                currentLeftWeapon.GetAction( ActorActionInput.rt, false )
            );
        }

        public ActorAction GetActionSlot ( )
        {
            ActorActionInput a_input = GetActionInput();
            return GetActionForInput( a_input );
        }

        private ActorAction GetActionForInput ( ActorActionInput input )
        {
            for ( int i = 0; i < actionSlots.Count; i++ )
            {
                if ( actionSlots[ i ].input == input )
                {
                    return actionSlots[ i ];
                }
            }

            return null;
        }

        private void SetActionForInput ( ActorActionInput input, ActorAction action )
        {
            for ( int i = 0; i < actionSlots.Count; i++ )
            {
                if ( actionSlots[ i ].input == input )
                {
                    actionSlots[ i ].targetAnim = action.targetAnim;
                    actionSlots[ i ].mirror = action.mirror;
                    actionSlots[ i ].actorActionType = action.actorActionType;
                    actionSlots[ i ].canBeParried = action.canBeParried;
                    actionSlots[ i ].animSpeed = action.animSpeed;
                    actionSlots[ i ].changeSpeed = action.changeSpeed;
                    actionSlots[ i ].canBackStab = action.canBackStab;
                }
            }
        }

        private ActorActionInput GetActionInput ( )
        {
            if ( states.rb )
            {
                return ActorActionInput.rb;
            }
            if ( states.rt )
            {
                return ActorActionInput.rt;
            }
            if ( states.lb )
            {
                return ActorActionInput.lb;
            }
            if ( states.lt )
            {
                return ActorActionInput.lt;
            }

            return ActorActionInput.rb;
        }

        private void EmptyAllSlots ( )
        {
            for ( int i = 0; i < 4; i++ )
            {
                ActorAction action = new ActorAction();
                action.targetAnim = null;
                action.mirror = false;
                action.actorActionType = ActorActionType.attack;
                action.animSpeed = 1.0f;
                action.changeSpeed = false;
                action.canBackStab = false;
                SetActionForInput( (ActorActionInput) i, action );
            }
        }
    }
}
