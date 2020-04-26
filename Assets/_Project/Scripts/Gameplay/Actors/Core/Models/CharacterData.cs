using UnityEngine;

namespace DistantMeadows.Actors.Core.Models {
    [CreateAssetMenu( fileName = "New Character", menuName = "Character", order = 1 )]
    public class CharacterData : EntityData {
        public new string name = "character";
        public int level = 0;
        public int experience = 0;
        public Color familyColor = Color.black;
        public Color favoriteColor = Color.white;
    }
}
