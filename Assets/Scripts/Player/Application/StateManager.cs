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
            Vector3 parryTargetPosition;
            Vector3 parryTargetDirection;
            EnemyStates parryTarget = GetEnemyInfront(
                out parryTargetPosition,
                out parryTargetDirection,
                3.0f
            );
            if ( parryTarget == null )
            {
                Debug.Log( "CheckForParry: Couldn't find the parry target" );
                return false;
            }

            if ( !parryTarget.canBeParried || !parryTarget.parryIsOn )
            {
                Debug.Log( "CheckForParry: Target is not able to be parried currently" );
                return false;
            }

            // Make sure you are behind the target
            float absAngle;
            if ( BehindTarget( parryTarget, out absAngle ) || absAngle < 165 )
            {
                Debug.Log( "CheckForParry: Not in front of target" );
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

            Debug.Log( "Parry Attacking Target" );

            RunActionAnimation( "parry_attack" );
            return true;
        }

        private bool CheckForBackStab ( Action action )
        {
            if ( !action.canBackStab )
            {
                Debug.Log( "CheckForBackStab: Action is not able to initiate backstab" );
                return false;
            }

            Vector3 backStabTargetPosition;
            Vector3 backStabTargetDirection;
            EnemyStates backStabTarget = GetEnemyInfront(
                out backStabTargetPosition,
                out backStabTargetDirection
            );
            if ( backStabTarget == null )
            {
                Debug.Log( "CheckForBackStab: Couldn't find the backstab target" );
                return false;
            }

            // Make sure you are behind the target
            float absAngle;
            if ( !BehindTarget( backStabTarget, out absAngle ) || absAngle > 15 )
            {
                Debug.Log( "CheckForBackStab: Not behind the target" );
                return false;
            }

            Vector3 targetPosition = ( -backStabTargetDirection * parryOffset ) + backStabTargetPosition;
            transform.position = targetPosition; // Move to target position

            if ( backStabTargetDirection == Vector3.zero )
            {
                backStabTargetDirection = backStabTarget.transform.forward;
            }

            // Make the backstab target look away from the player
            Quaternion parryTargetRotation = Quaternion.LookRotation( backStabTargetDirection );
            backStabTarget.transform.rotation = parryTargetRotation;

            // Make the player look at the backstab target
            Quaternion personalRotation = Quaternion.LookRotation( backStabTargetDirection );
            transform.rotation = personalRotation;

            Debug.Log( "Backstabbing Target" );

            // Make the parry target run their parry operations
            backStabTarget.IsGettingParried();

            RunActionAnimation( "parry_attack" );
            return true;
        }

        private EnemyStates GetEnemyInfront (
            out Vector3 targetPosition,
            out Vector3 targetDirection,
            float maxDistanceToCheck = 1.0f,
            float maxAngleToCheck = 60.0f
        )
        {
            // Get the enemy state from a ray hit
            EnemyStates target = null;

            // Get the target withing the max distance in front of the user
            Vector3 currentPosition;
            currentPosition = transform.position;
            currentPosition.y += 1;
            Vector3 directionCurrentlyFacing = transform.forward;
            RaycastHit hit;
            if ( !Physics.Raycast( currentPosition, directionCurrentlyFacing, out hit, maxDistanceToCheck, ignoreLayers ) )
            {
                Debug.Log( "GetEnemyInfrontOfPlayer: Raycast Failed" );

                targetPosition = Vector3.zero;
                targetDirection = Vector3.zero;
                return null;
            }

            target = hit.transform.GetComponentInParent<EnemyStates>();
            currentPosition = transform.position;
            targetPosition = target.transform.position;
            targetDirection = targetPosition - currentPosition;
            targetDirection.Normalize();
            targetDirection.y = 0; // Make sure we aren't messing up the look rotation

            // Make sure the target is in front of the player within acceptable angle
            float angleOfParryTarget = Vector3.Angle( transform.forward, targetDirection );
            bool parryTargetIsInFront = angleOfParryTarget < maxAngleToCheck;
            if ( !parryTargetIsInFront )
            {
                Debug.Log( "GetEnemyInfrontOfPlayer: Angle Failed" );
                return null;
            }

            return target;
        }

        private bool BehindTarget ( EnemyStates target, out float absAngle )
        {
            Vector3 directionToTarget = target.transform.position - transform.position;
            float angle = Vector3.Angle( target.transform.forward, directionToTarget );
            absAngle = Mathf.Abs( angle );

            Debug.Log( "BehindTarget: Player is at angle: " + absAngle );

            return absAngle < 90;
        }

        private void AttackAction ( Action action )
        {
            if ( action == null || CheckForBackStab( action ) || CheckForParry() )
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
            float targetSpeed = 1.0f;
            if ( action.changeSpeed )
            {
                targetSpeed = action.animSpeed;
                if ( targetSpeed == 0 )
                {
                    targetSpeed = 1;
                }
            }
            RunActionAnimation( action.targetAnim, action.canBeParried, false, true, targetSpeed );
        }

        private void RunActionAnimation (
            string targetAnimation,
            bool cbp = true,
            bool cm = false,
            bool ia = true,
            float targetSpeed = 1.0f
        )
        {
            if ( string.IsNullOrEmpty( targetAnimation ) )
            {
                return;
            }

            canBeParried = cbp;
            canMove = cm;
            inAction = ia;
            anim.SetFloat( "animSpeed", targetSpeed );
            anim.CrossFade( targetAnimation, 0.2f );
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
