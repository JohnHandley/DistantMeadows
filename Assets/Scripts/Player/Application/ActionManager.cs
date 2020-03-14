using UnityEngine;
using System.Collections.Generic;
using Player.Domain;

namespace Player.Application
{
    public class ActionManager : MonoBehaviour
    {
        public List<Action> actionSlots = new List<Action>();
        public ItemAction consumableItem;

        public StateManager states;

        ActionManager ( )
        {
            for ( int i = 0; i < 4; i++ )
            {
                Action a = new Action();
                a.input = (ActionInput) i;
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

            List<Action> weaponActions = twoHanded ? currentWeapon.th_actions : currentWeapon.oh_actions;

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
                ActionInput.rb,
                currentRightWeapon.GetAction( ActionInput.rb, false )
            );
            SetActionForInput(
                ActionInput.rt,
                currentRightWeapon.GetAction( ActionInput.rt, false )
            );
            SetActionForInput(
                ActionInput.lb,
                currentLeftWeapon.GetAction( ActionInput.rb, false )
            );
            SetActionForInput(
                ActionInput.lt,
                currentLeftWeapon.GetAction( ActionInput.rt, false )
            );
        }

        public Action GetActionSlot ( )
        {
            ActionInput a_input = GetActionInput();
            return GetActionForInput( a_input );
        }

        private Action GetActionForInput ( ActionInput input )
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

        private void SetActionForInput ( ActionInput input, Action action )
        {
            for ( int i = 0; i < actionSlots.Count; i++ )
            {
                if ( actionSlots[ i ].input == input )
                {
                    actionSlots[ i ].targetAnim = action.targetAnim;
                    actionSlots[ i ].mirror = action.mirror;
                    actionSlots[ i ].type = action.type;
                    actionSlots[ i ].canBeParried = action.canBeParried;
                    actionSlots[ i ].animSpeed = action.animSpeed;
                    actionSlots[ i ].changeSpeed = action.changeSpeed;
                    actionSlots[ i ].canBackStab = action.canBackStab;
                }
            }
        }

        private ActionInput GetActionInput ( )
        {
            if ( states.rb )
            {
                return ActionInput.rb;
            }
            if ( states.rt )
            {
                return ActionInput.rt;
            }
            if ( states.lb )
            {
                return ActionInput.lb;
            }
            if ( states.lt )
            {
                return ActionInput.lt;
            }

            return ActionInput.rb;
        }

        private void EmptyAllSlots ( )
        {
            for ( int i = 0; i < 4; i++ )
            {
                Action action = new Action();
                action.targetAnim = null;
                action.mirror = false;
                action.type = ActionType.attack;
                action.animSpeed = 1.0f;
                action.changeSpeed = false;
                action.canBackStab = false;
                SetActionForInput( (ActionInput) i, action );
            }
        }
    }
}
