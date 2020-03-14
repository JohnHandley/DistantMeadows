using UnityEngine;
using Enemies.Application;
using Player.Application;

namespace Items.Application
{
    public class ParryCollider : MonoBehaviour
    {
        StateManager player;
        EnemyStates enemy;

        public float maxTimer = 0.6f;
        float timer;

        void Update ( )
        {
            if ( player != null )
            {
                timer += player.delta;

                if ( timer > maxTimer )
                {
                    timer = 0;
                    gameObject.SetActive( false );
                }
            }

            if ( enemy != null )
            {
                timer += enemy.delta;

                if ( timer > maxTimer )
                {
                    timer = 0;
                    gameObject.SetActive( false );
                }
            }
        }

        public void InitPlayer ( StateManager st )
        {
            player = st;
        }

        public void InitEnemy ( EnemyStates eSt )
        {
            enemy = eSt;
        }

        private void OnTriggerEnter ( Collider other )
        {
            //DamageCollider dc = other.GetComponent<DamageCollider>();
            //if ( dc == null )
            //{
            //    return;
            //}

            if ( player != null )
            {
                // Check to see if the Player has parried an enemy
                EnemyStates possibleParriedEnemy = other.transform.GetComponentInParent<EnemyStates>();
                if ( possibleParriedEnemy != null )
                {
                    possibleParriedEnemy.CheckForParry( transform.root, player );
                }
            }

            if ( enemy != null )
            {
                // Check to see if an Enemy has parried the player
            }

        }
    }
}

