using System.Collections.Generic;

namespace DistantMeadows.Core.Models
{
    public class StateMachine<StateType>
    {
        private string id;
        private State currentState;
        private List<StateTransition<StateType>> transitions;

        private Dictionary<string, State> states = new Dictionary<string, State>();

        public StateMachine(
            string id,
            State currentState,
            List<StateTransition<StateType>> transitions
        )
        {
            this.id = id;
            this.currentState = currentState;
            this.transitions = transitions;

            foreach (StateTransition<StateType> transition in transitions)
            {
                if (!states.ContainsKey(transition.from.id))
                {
                    states.Add(transition.from.id, transition.from);
                }
                if (!states.ContainsKey(transition.to.id))
                {
                    states.Add(transition.to.id, transition.to);
                }
            }
        }

        public void Apply(List<Condition<StateType>> conditions)
        {
            currentState = GetNextState(conditions);
        }

        public State GetCurrentState()
        {
            return currentState;
        }

        public State GetState(string id)
        {
            states.TryGetValue(id, out State state);
            return state;
        }

        private State GetNextState(List<Condition<StateType>> conditions)
        {
            foreach (StateTransition<StateType> transition in transitions)
            {
                bool currentStateMatches = transition.from == currentState;
                bool conditionsMatch = transition.conditions == conditions;
                if (currentStateMatches && conditionsMatch)
                {
                    return transition.to;
                }
            }
            return null;
        }
    }
}
