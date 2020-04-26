using DistantMeadows.Core.Models;
using DistantMeadows.Actors.Core.Enums;
using DistantMeadows.Core.Enums;

namespace DistantMeadows.Actors.Core.Models
{
    [System.Serializable]
    public class ActorAction : Action
    {
        public string actor_id;

        public ActorActionInput input;
        public ActorActionType actorActionType;

        public bool canBeParried = true;
        public bool canBackStab = false;

        public ActorAction ( )
        {
            type = ActionType.Actor;
        }
    }
}
