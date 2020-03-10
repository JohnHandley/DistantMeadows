using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SA
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
            Weapon w = states.inventoryManager.curWeapon;

            List<Action> weaponActions = twoHanded ? w.th_actions : w.oh_actions;

            for ( int i = 0; i < weaponActions.Count; i++ )
            {
                Action a = GetAction( weaponActions[ i ].input );
                a.targetAnim = weaponActions[ i ].targetAnim;
            }
        }

        public Action GetActionSlot ( )
        {
            ActionInput a_input = GetActionInput();
            return GetAction( a_input );
        }

        public Action GetAction ( ActionInput input )
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

        public ActionInput GetActionInput ( )
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

        void EmptyAllSlots ( )
        {
            for ( int i = 0; i < 4; i++ )
            {
                Action a = GetAction( (ActionInput) i );
                a.targetAnim = null;
            }
        }
    }

    public enum ActionInput
    {
        rb,
        lb,
        rt,
        lt,
        x,
        none
    }

    [System.Serializable]
    public class Action
    {
        public ActionInput input;

        public string targetAnim;
    }

    [System.Serializable]
    public class ItemAction : Action
    {
        public string item_id;
    }
}
