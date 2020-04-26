using UnityEngine;

using DistantMeadows.Actors.Core.Models;

namespace DistantMeadows.Actors.NPC.Models {
    [CreateAssetMenu( fileName = "New Villager", menuName = "NPC/Villager", order = 1 )]
    public class VillagerData : CharacterData {
        public string villageName = "default village";
    }
}
