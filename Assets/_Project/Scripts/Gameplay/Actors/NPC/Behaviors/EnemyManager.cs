using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DistantMeadows.Actors.Enemies.Behaviors
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager singleton;

        public List<EnemyTarget> enemies = new List<EnemyTarget>();

        void Awake ( )
        {
            singleton = this;
        }

        public EnemyTarget GetEnemy ( Vector3 from )
        {
            EnemyTarget enemy = null;
            float minDistance = float.MaxValue;
            for ( int i = 0; i < enemies.Count; i++ )
            {
                float tDist = Vector3.Distance( from, enemies[ i ].GetTarget().position );
                if ( tDist < minDistance )
                {
                    minDistance = tDist;
                    enemy = enemies[ i ];
                }
            }

            return enemy;
        }
    }
}
