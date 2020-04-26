using DistantMeadows.Actors.Core.Behaviors;
using DistantMeadows.Core.Models;

namespace DistantMeadows.Actors.Player.Controllers
{

    public class MonitorInteractingAnimation : StateAction
    {
        private readonly CharacterStateManager states;
        private readonly string targetBool;
        private readonly string targetState;

        public MonitorInteractingAnimation(CharacterStateManager characterStateManager, string targetBool, string targetState)
        {
            states = characterStateManager;
            this.targetBool = targetBool;
            this.targetState = targetState;
        }

        public override bool Execute()
        {
            bool isInteracting = states.anim.GetBool(targetBool);

            if (isInteracting)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}