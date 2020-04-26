using UnityEngine;

namespace DistantMeadows.Core.Utilities
{
    public static class GameLogger
    {
        public static void Info ( string message, GameObject gameObject )
        {
            Debug.Log( $"<size=22>{message}</size>", gameObject );
        }
    }
}

