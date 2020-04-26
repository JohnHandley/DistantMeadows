
using System.Collections.Generic;

namespace DistantMeadows.Core.Models
{
    public class StateTransition<ConditionType>
    {
        public string id;
        public State from;
        public State to;
        public List<Condition<ConditionType>> conditions;

        public StateTransition(string id, State from, State to, List<Condition<ConditionType>> conditions)
        {
            this.id = id;
            this.from = from;
            this.to = to;
            this.conditions = conditions;
        }
    }
}

