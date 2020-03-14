using UnityEngine.UI;

namespace UI.Domain
{
    [System.Serializable]
    public class QuickSlot
    {
        public Image icon;
        public QuickSlotType type;
    }

    public enum QuickSlotType
    {
        RightHand,
        LeftHand,
        Item,
        Spell
    }
}

