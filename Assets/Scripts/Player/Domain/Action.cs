namespace Player.Domain
{
    public enum ActionInput
    {
        rb,
        lb,
        rt,
        lt,
        x,
        none
    }

    public enum ActionType
    {
        attack,
        block,
        spells,
        parry
    }

    [System.Serializable]
    public class Action
    {
        public ActionInput input;
        public ActionType type;
        public string targetAnim;
        public bool mirror = false;
        public bool canBeParried = true;
        public bool changeSpeed = false;
        public float animSpeed = 1;
    }

    [System.Serializable]
    public class ItemAction : Action
    {
        public string item_id;
    }
}

