using Enemies.Application;
using Player.Domain;
using UnityEngine;

namespace Player.Application
{
    public class StateManager : MonoBehaviour
    {
        [Header( "Init" )]
        public GameObject activeModel;

        [Header( "Inputs" )]
        public float vertical;
        public float horizontal;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;
        public bool rollInput;
        public bool itemInput;

        [Header( "Stats" )]
        public float moveSpeed = 3.5f;
        public float runSpeed = 5.5f;
        public float rotateSpeed = 9.0f;
        public float toGround = 0.5f;
        public float rollSpeed = 15.0f;
        public float parryOffset = 1.4f;

        [Header( "States" )]
        public bool onGround;
        public bool run;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool canBeParried;
        public bool parryIsOn;
        public bool isTwoHanded;
        public bool usingItem;
        public bool isAttacking;
        public bool isBlocking;
        public bool isParrying;
        public bool isCasting;
        public bool isLeftHand;


        [Header( "Other" )]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve roll_curve;
        public EnemyStates parryTarget;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public AnimatorHook a_hook;
        [HideInInspector]
        public ActionManager actionManager;
        [HideInInspector]
        public InventoryManager inventoryManager;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;

        float _actionDelay;

        #region Private Methods
        public void Init ( )
        {
            SetupAnimator();
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init( this );

            actionManager = GetComponent<ActionManager>();
            actionManager.Init( this );

            a_hook = activeModel.GetComponent<AnimatorHook>();
            if ( a_hook == null )
            {
                Debug.Log( "Added Animator Hook to Player" );
                a_hook = activeModel.AddComponent<AnimatorHook>();
            }
            a_hook.Init( this, null );

            gameObject.layer = 8;
            ignoreLayers = ~( 1 << 9 );

            anim.SetBool( "onGround", true );
        }

        private void SetupAnimator ( )
        {
            if ( activeModel == null )
            {
                anim = GetComponentInChildren<Animator>();
                if ( anim == null )
                {
                    Debug.Log( "No model found" );
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }

            if ( anim == null )
                anim = activeModel.GetComponent<Animator>();

            anim.applyRootMotion = false;
        }

        private void DetectItemAction ( )
        {
            if ( !canMove || usingItem || isBlocking )
                return;

            if ( !itemInput )
                return;

            ItemAction slot = actionManager.consumableItem;
            string targetAnim = slot.targetAnim;

            if ( string.IsNullOrEmpty( targetAnim ) )
                return;

            usingItem = true;
            anim.Play( targetAnim );
        }

        private void DetectAction ( )
        {
            if ( canMove == false || usingItem )
            {
                return;
            }

            if ( rb == false && rt == false && lt == false && lb == false )
            {
                return;
            }

            Action currentAction = actionManager.GetActionSlot();
            if ( currentAction == null )
            {
                return;
            }
            isLeftHand = currentAction.mirror;
            anim.SetBool( "mirror", isLeftHand );

            switch ( currentAction.type )
            {
                case ActionType.attack:
                    AttackAction( currentAction );
                    break;
                case ActionType.block:
                    BlockAction( currentAction );
                    break;
                case ActionType.parry:
                    ParryAction( currentAction );
                    break;
                case ActionType.spells:
                    SpellAction( currentAction );
                    break;
                default:
                    break;
            }
        }

        private bool CheckForParry ( )
        {
            if ( parryTarget == null )
            {
                return false;
            }

            Vector3 parryTargetPosition = parryTarget.transform.position;
            Vector3 currentPosition = transform.position;

            float distanceToParryTarget = Vector3.Distance( parryTargetPosition, currentPosition );
            if ( distanceToParryTarget > 3.0f )
            {
                return false;
            }

            Vector3 parryTargetDirection = parryTargetPosition - currentPosition;
            parryTargetDirection.Normalize();
            parryTargetDirection.y = 0; // Make sure we aren't messing up the look rotation
            float angleOfParryTarget = Vector3.Angle( transform.forward, parryTargetDirection );
            bool parryTargetIsInFront = angleOfParryTarget < 60.0f;
            if ( !parryTargetIsInFront )
            {
                return false;
            }

            Vector3 targetPosition = ( -parryTargetDirection * parryOffset ) + parryTargetPosition;

            transform.position = targetPosition; // Move to target position

            if ( parryTargetDirection == Vector3.zero )
            {
                parryTargetDirection = -parryTarget.transform.forward;
            }

            // Make the parry target look at the player
            Quaternion parryTargetRotation = Quaternion.LookRotation( -parryTargetDirection );
            parryTarget.transform.rotation = parryTargetRotation;

            // Make the player look at the parry target
            Quaternion personalRotation = Quaternion.LookRotation( parryTargetDirection );
            transform.rotation = personalRotation;

            // Make the parry target run their parry operations
            parryTarget.IsGettingParried();

            canMove = false;
            inAction = true;
            anim.CrossFade( "parry_attack", 0.2f );
            return true;
        }

        private void AttackAction ( Action action )
        {
            if ( action == null )
            {
                return;
            }

            if ( CheckForParry() )
            {
                return;
            }

            isAttacking = true;
            AttemptAction( action );
        }

        private void BlockAction ( Action action )
        {
            if ( action == null )
            {
                return;
            }

            isBlocking = true;
        }

        private void ParryAction ( Action action )
        {
            if ( action == null )
            {
                return;
            }

            isParrying = true;
            AttemptAction( action );
        }

        private void SpellAction ( Action action )
        {
            if ( action == null )
            {
                return;
            }

            isCasting = true;
        }

        private void AttemptAction ( Action action )
        {
            string targetAnim = action.targetAnim;

            if ( string.IsNullOrEmpty( targetAnim ) )
                return;

            canBeParried = action.canBeParried;
            canMove = false;
            inAction = true;

            float targetSpeed = 1.0f;
            if ( action.changeSpeed )
            {
                targetSpeed = action.animSpeed;
                if ( targetSpeed == 0 )
                {
                    targetSpeed = 1;
                }
            }
            anim.SetFloat( "animSpeed", targetSpeed );
            anim.CrossFade( targetAnim, 0.2f );
        }

        private void DetectRollAction ( )
        {
            a_hook.CloseRoll();
            if ( !rollInput || usingItem )
                return;

            float v = vertical;
            float h = horizontal;
            v = ( moveAmount > 0.3f ) ? 1 : 0;
            h = 0;

            if ( v != 0 )
            {
                if ( moveDir == Vector3.zero )
                    moveDir = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation( moveDir );
                transform.rotation = targetRot;
                a_hook.InitForRoll();
                a_hook.rm_multi = rollSpeed;
            }
            else
            {
                a_hook.rm_multi = 1.3f;
            }


            anim.SetFloat( "vertical", v );
            anim.SetFloat( "horizontal", h );

            canMove = false;
            inAction = true;
            anim.CrossFade( "Rolls", 0.2f );

        }

        private void HandleMovementAnimations ( )
        {
            anim.SetBool( "run", run );
            anim.SetFloat( "vertical", moveAmount, 0.4f, delta );
        }

        private void HandleLockOnAnimations ( Vector3 moveDir )
        {
            Vector3 relativeDir = transform.InverseTransformDirection( moveDir );
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat( "vertical", v, 0.2f, delta );
            anim.SetFloat( "horizontal", h, 0.2f, delta );

        }

        private bool OnGround ( )
        {
            bool r = false;

            Vector3 origin = transform.position + ( Vector3.up * toGround );
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;
            RaycastHit hit;
            Debug.DrawRay( origin, dir * dis );
            if ( Physics.Raycast( origin, dir, out hit, dis, ignoreLayers ) )
            {
                r = true;
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }

            return r;
        }
        #endregion

        #region Public Methods
        public void FixedTick ( float d )
        {
            delta = d;

            isBlocking = false;
            isAttacking = false;
            isParrying = false;
            isCasting = false;

            usingItem = anim.GetBool( "interacting" );

            DetectAction();
            DetectItemAction();

            inventoryManager.rightHandWeapon.weaponModel.SetActive( !usingItem );

            anim.SetBool( "blocking", isBlocking );
            anim.SetBool( "isLeft", isLeftHand );

            if ( inAction )
            {
                anim.applyRootMotion = true;

                _actionDelay += delta;
                if ( _actionDelay > 0.3f )
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }

            canMove = anim.GetBool( "canMove" );
            if ( !canMove )
                return;

            DetectRollAction();

            anim.applyRootMotion = false;
            rigid.drag = ( moveAmount > 0 || onGround == false ) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if ( usingItem )
            {
                run = false;
                moveAmount = Mathf.Clamp( moveAmount, 0, 0.45f );
            }

            if ( run )
            {
                targetSpeed = runSpeed;
                lockOn = false;
            }

            if ( onGround )
                rigid.velocity = moveDir * ( targetSpeed * moveAmount );

            Vector3 targetDir = ( lockOn == false ) ?
                moveDir
                :
                ( lockOnTransform != null ) ?
                    lockOnTransform.transform.position - transform.position
                    :
                    moveDir;

            targetDir.y = 0;
            if ( targetDir == Vector3.zero )
                targetDir = transform.forward;
            Quaternion tr = Quaternion.LookRotation( targetDir );
            Quaternion targetRotation = Quaternion.Slerp( transform.rotation, tr, delta * moveAmount * rotateSpeed );
            transform.rotation = targetRotation;

            anim.SetBool( "lockon", lockOn );

            if ( lockOn == false )
                HandleMovementAnimations();
            else
                HandleLockOnAnimations( moveDir );
        }

        public void Tick ( float d )
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool( "onGround", onGround );
        }

        public void HandleTwoHanded ( )
        {
            anim.SetBool( "two_handed", isTwoHanded );

            actionManager.UpdateActionsWithCurrentWeapon( isTwoHanded );
        }

        public void IsGettingParried ( )
        {

        }
        #endregion
    }
}
