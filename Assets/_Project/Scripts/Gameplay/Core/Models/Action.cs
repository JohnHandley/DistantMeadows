using DistantMeadows.Core.Enums;

namespace DistantMeadows.Core.Models
{
    [System.Serializable]
    public class Action
    {
        public ActionType type = ActionType.None;
        public string targetAnim;

        public bool mirror = false;
        public bool changeSpeed = false;
        public float animSpeed = 1;
    }
}

