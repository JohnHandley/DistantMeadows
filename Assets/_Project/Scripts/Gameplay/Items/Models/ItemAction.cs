using DistantMeadows.Core.Enums;
using DistantMeadows.Core.Models;

namespace DistantMeadows.Items.Models
{
    [System.Serializable]
    public class ItemAction : Action
    {
        public string item_id;

        public ItemAction ( )
        {
            type = ActionType.Item;
        }
    }
}
