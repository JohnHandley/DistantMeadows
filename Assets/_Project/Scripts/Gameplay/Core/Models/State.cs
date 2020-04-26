using System.Collections.Generic;

namespace DistantMeadows.Core.Models {
    public class State {
        public string id;

        private bool forceExit;
        private readonly List<StateAction> fixedUpdateActions;
        private readonly List<StateAction> updateActions;
        private readonly List<StateAction> lateUpdateActions;

        public delegate void OnEnter ( );
        public OnEnter onEnter;

        public delegate void OnExit ( );

        public OnExit onExit;

        public State ( string id, List<StateAction> fixedUpdateActions, List<StateAction> updateActions, List<StateAction> lateUpdateActions ) {
            this.id = id;
            this.fixedUpdateActions = fixedUpdateActions;
            this.updateActions = updateActions;
            this.lateUpdateActions = lateUpdateActions;
        }

        public void AddFixedTickAction ( StateAction action ) {
            fixedUpdateActions.Add( action );
        }

        public void AddTickAction ( StateAction action ) {
            updateActions.Add( action );
        }

        public void AddLateTickAction ( StateAction action ) {
            lateUpdateActions.Add( action );
        }

        public void FixedTick ( ) {
            ExecuteListOfActions( fixedUpdateActions );
        }

        public void Tick ( ) {
            ExecuteListOfActions( updateActions );
        }

        public void LateTick ( ) {
            ExecuteListOfActions( lateUpdateActions );
            forceExit = false;
        }

        void ExecuteListOfActions ( List<StateAction> actions ) {
            for ( int i = 0; i < actions.Count; i++ ) {
                if ( forceExit )
                    return;

                forceExit = actions[ i ].Execute();
            }
        }
    }
}
