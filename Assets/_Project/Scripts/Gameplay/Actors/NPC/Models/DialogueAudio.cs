using UnityEngine;

namespace DistantMeadows.Actors.NPC.Models {
    [System.Serializable]
    public class DialogueAudio {
        [Header( "Effect Object" )]
        public AudioSource effectSource;

        [Header( "Effect Audio Clips" )]
        public AudioClip sparkleClip;
        public AudioClip rainClip;
    }
}

