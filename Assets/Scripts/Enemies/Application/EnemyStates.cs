using System.Collections;
using System.Collections.Generic;
using Player.Application;
using UnityEngine;

namespace Enemies.Application
{
    public class EnemyStates : MonoBehaviour
    {
        public float health;
        public bool canBeParried = true;
        public bool parryIsOn = true;
        public bool isInvincible;
        public bool canMove;
        public bool isDead;
        public bool dontDoAnything;

        public Animator anim;
        public Rigidbody rigid;
        public float delta;

        private EnemyTarget enTarget;
        private StateManager parriedBy;

        private AnimatorHook a_hook;
        private readonly List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        private readonly List<Collider> ragdollColliders = new List<Collider>();

        float timer;
        public bool doParry = false;

        #region Private Methods
        private void Start ( )
        {
            health = 100;
            anim = GetComponentInChildren<Animator>();
            enTarget = GetComponent<EnemyTarget>();
            enTarget.Init( this );

            rigid = GetComponent<Rigidbody>();

            a_hook = anim.GetComponent<AnimatorHook>();
            if ( a_hook == null )
            {
                Debug.Log( "Added Animator Hook to Enemy" );
                a_hook = anim.gameObject.AddComponent<AnimatorHook>();
            }
            a_hook.Init( null, this );

            InitRagdoll();

            Debug.Log( "Enemy State Started" );
        }

        private void Update ( )
        {
            delta = Time.deltaTime;
            canMove = anim.GetBool( "canMove" );

            if ( dontDoAnything )
            {
                dontDoAnything = !canMove;
                return;
            }

            if ( health <= 0 )
            {
                if ( !isDead )
                {
                    isDead = true;
                    EnableRagdoll();
                }
            }

            if ( isInvincible )
            {
                isInvincible = !canMove;
            }

            if ( parriedBy != null && !parryIsOn )
            {
                parriedBy = null;
            }

            if ( canMove )
            {
                parryIsOn = false;
                anim.applyRootMotion = false;

                //Debug
                timer += Time.deltaTime;
                if ( timer > 3 )
                {
                    DoAction();
                    timer = 0;
                }
            }
        }

        private void InitRagdoll ( )
        {
            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for ( int i = 0; i < rigs.Length; i++ )
            {
                if ( rigs[ i ] == rigid )
                {
                    continue;
                }

                ragdollRigids.Add( rigs[ i ] );
                rigs[ i ].isKinematic = true;

                Collider col = rigs[ i ].gameObject.GetComponent<Collider>();
                col.isTrigger = true;
                ragdollColliders.Add( col );
            }
        }

        private IEnumerator CloseAnimator ( )
        {
            yield return new WaitForEndOfFrame();
            anim.enabled = false;
            this.enabled = false;
        }

        private void DoAction ( )
        {
            anim.Play( "oh_attack_1" );
            anim.applyRootMotion = true;
            anim.SetBool( "canMove", false );
        }
        #endregion

        #region Public Methods
        public void EnableRagdoll ( )
        {
            for ( int i = 0; i < ragdollRigids.Count; i++ )
            {
                ragdollRigids[ i ].isKinematic = false;
                ragdollColliders[ i ].isTrigger = false;
            }

            Collider controllerCollider = rigid.gameObject.GetComponent<Collider>();
            controllerCollider.enabled = false;
            rigid.isKinematic = true;

            StartCoroutine( "CloseAnimator" );
        }

        public void DoDamage ( float v )
        {
            if ( isInvincible )
                return;

            health -= v;
            isInvincible = true;
            anim.Play( "damage_1" );
            anim.applyRootMotion = true;
            anim.SetBool( "canMove", false );
        }

        public void CheckForParry ( Transform target, StateManager player )
        {
            if ( !canBeParried || !parryIsOn || isInvincible )
            {
                return;
            }

            Vector3 directionToTarget = transform.position - target.position;
            directionToTarget.Normalize();
            bool isBehindTarget = Vector3.Dot( target.forward, directionToTarget ) < 0;
            if ( isBehindTarget )
            {
                // The target can't parry you if you are behind them
                return;
            }

            isInvincible = true;
            anim.Play( "attack_interrupt" );
            anim.applyRootMotion = true;
            anim.SetBool( "canMove", false );
            parriedBy = player;
            doParry = true;
        }

        public void IsGettingParried ( )
        {
            health = 0; // Die

            dontDoAnything = true;
            anim.SetBool( "canMove", false );
            anim.Play( "parry_recieved" );
        }
        #endregion
    }
}


