using System.Collections.Generic;
using UnityEngine;

namespace DistantMeadows.Actors.NPC.Models {
    [CreateAssetMenu( fileName = "New Dialogue Data", menuName = "Dialogue Data" )]
    public class DialogueData : ScriptableObject {
        [TextArea( 4, 4 )]
        public List<string> conversationBlock;
    }
}

