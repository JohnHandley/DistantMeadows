// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.QuestMachine
{

    /// <summary>
    /// This helper component invokes a delegate method when disabled.
    /// Spawners work with SpawnedEntity instead of the Pixel Crushers
    /// common library SpawnedObject because they handle handle spawning 
    /// differently from SpawnedObjectManager.
    /// </summary>
    [AddComponentMenu("")]
    public class SpawnedEntity : MonoBehaviour
    {
        public delegate void SpawnedObjectDelegate(SpawnedEntity spawnedEntity);

        public event SpawnedObjectDelegate disabled = delegate { };

        protected virtual void OnDisable()
        {
            disabled(this);
        }
    }
}