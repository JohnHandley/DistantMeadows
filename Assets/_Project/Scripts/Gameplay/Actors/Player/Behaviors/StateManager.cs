using DistantMeadows.Actors.Core.Models;
using DistantMeadows.Actors.Enemies.Behaviors;
using DistantMeadows.Core.Constants;
using DistantMeadows.Items.Models;
using UnityEngine;

namespace DistantMeadows.Actors.Player.Behaviors
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
        public float moveSpeed = 4.5f;
        public float runSpeed = 6.5f;
        public float rotateSpeed = 9.0f;
        public float toGround = 0.5f;
        public float rollSpeed = 8.0f;
        public float parryOffset = 1.3f;
        public float backStabOffset = 1.4f;

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

        [SerializeField] private string _name;

        [Header( "Health" )]
        [SerializeField] private float _maxiumumHealth = 100;
        [SerializeField] private float _currentHealth;

        float _actionDelay;

        #region Private Methods
        private void InitializeHealth ( )
        {
            _currentHealth = _maxiumumHealth;
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Health <b>Initialized</b></size>", gameObject );
        }

        private void InitializeName ( )
        {
            _name = $"Player:{Random.Range( 0, 100 )}";
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Name <b>Initialized</b></size>", gameObject );
        }

        private void InitializeRigidbody ( )
        {
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Rigidbody <b>Initialized</b></size>", gameObject );
        }

        private void InitializeInventory ( )
        {
            inventoryManager = GetComponent<InventoryManager>();
            if ( inventoryManager == null )
            {
                Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Inventory <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }

            inventoryManager.Init( this );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Inventory <b>Initialized</b></size>", gameObject );
        }

        private void InitializeActions ( )
        {
            actionManager = GetComponent<ActionManager>();
            if ( actionManager == null )
            {
                Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Actions <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }

            actionManager.Init( this );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Actions <b>Initialized</b></size>", gameObject );
        }

        private void InitializeAnimator ( )
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
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Animator <b>Initialized</b></size>", gameObject );
        }

        private void InitializeAnimationHook ( )
        {
            a_hook = activeModel.GetComponent<AnimatorHook>();
            if ( a_hook == null )
            {
                Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: AnimatorHook <color=green><b>Added</b></color></size>", gameObject );
                a_hook = activeModel.AddComponent<AnimatorHook>();
            }
            a_hook.Init( this, null );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: AnimatorHook <b>Initialized</b></size>", gameObject );
        }

        private void InitializeMiscellaneous ( )
        {
            gameObject.layer = 8;
            ignoreLayers = ~( 1 << 9 );

            anim.SetBool( AnimationConstants.AnimatorParamOnGround, true );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color>: Miscellaneous <b>Initialized</b></size>", gameObject );
        }

        private void DetectItemAction ( )
        {
            if ( !canMove || usingItem || isBlocking )
                return;

            if ( !itemInput )
                return;

            if ( actionManager == null )
            {
                return;
            }

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

            if ( actionManager == null )
            {
                return;
            }

            ActorAction currentAction = actionManager.GetActionSlot();
            if ( currentAction == null )
            {
                return;
            }
            isLeftHand = currentAction.mirror;
            anim.SetBool( AnimationConstants.AnimatorParamMirror, isLeftHand );

            switch ( currentAction.actorActionType )
            {
                case ActorActionType.attack:
                    AttackAction( currentAction );
                    break;
                case ActorActionType.block:
                    BlockAction( currentAction );
                    break;
                case ActorActionType.parry:
                    ParryAction( currentAction );
                    break;
                case ActorActionType.spells:
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

            Debug.Log( "Parry Attacking Target" );

            // Make the parry target run their parry operations
            parryTarget.IsGettingParried();

            RunActionAnimation( AnimationConstants.AnimationParryAttack );
            return true;
        }

        private bool CheckForBackStab ( ActorAction action )
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

            Vector3 targetPosition = ( -backStabTargetDirection * backStabOffset ) + backStabTargetPosition;
            transform.position = targetPosition; // Move to target position

            if ( backStabTargetDirection == Vector3.zero )
            {
                backStabTargetDirection = backStabTarget.transform.forward;
            }

            // Make the backstab target look away from the player
            Quaternion backStabTargetRotation = Quaternion.LookRotation( backStabTargetDirection );
            backStabTarget.transform.rotation = backStabTargetRotation;

            // Make the player look at the backstab target
            Quaternion personalRotation = Quaternion.LookRotation( backStabTargetDirection );
            transform.rotation = personalRotation;

            Debug.Log( "Backstabbing Target" );

            // Make the parry target run their parry operations
            backStabTarget.IsGettingBackStabbed();

            RunActionAnimation( AnimationConstants.AnimationParryAttack );
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
            float angleOfTarget = Vector3.Angle( transform.forward, targetDirection );
            bool targetIsInFront = angleOfTarget < maxAngleToCheck;
            if ( !targetIsInFront )
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

        private void AttackAction ( ActorAction action )
        {
            if ( action == null )
            {
                return;
            }

            if ( lockOn && ( CheckForBackStab( action ) || CheckForParry() ) )
            {
                return;
            }

            isAttacking = true;
            AttemptAction( action );
        }

        private void BlockAction ( ActorAction action )
        {
            Debug.Log( "Blocking!" );
            if ( action == null )
            {
                return;
            }

            isBlocking = true;
        }

        private void ParryAction ( ActorAction action )
        {
            Debug.Log( "Parrying!" );
            if ( action == null )
            {
                return;
            }

            isParrying = true;
            AttemptAction( action );
        }

        private void SpellAction ( ActorAction action )
        {
            if ( action == null )
            {
                return;
            }

            isCasting = true;
        }

        private void AttemptAction ( ActorAction action )
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
            anim.SetFloat( AnimationConstants.AnimatorParamAnimSpeed, targetSpeed );
            anim.CrossFade( targetAnimation, 0.2f );
        }

        private void DetectRollAction ( )
        {
            if ( a_hook == null )
            {
                return;
            }

            a_hook.CloseRoll();
            if ( !rollInput || usingItem )
                return;

            float v = vertical;
            float h = horizontal;

            if ( !lockOn )
            {
                v = ( moveAmount > 0.3f ) ? 1 : 0;
                h = 0;
            }

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


            anim.SetFloat( AnimationConstants.AnimatorParamVertical, v );
            anim.SetFloat( AnimationConstants.AnimatorParamHorizontal, h );

            canMove = false;
            inAction = true;
            anim.CrossFade( AnimationConstants.AnimationRolls, 0.2f );
            Debug.Log( "Rolled!" );
        }

        private void HandleMovementAnimations ( )
        {
            anim.SetBool( AnimationConstants.AnimatorParamRun, moveAmount > 0 && run );
            anim.SetFloat( AnimationConstants.AnimatorParamVertical, moveAmount, 0.4f, delta );
        }

        private void HandleLockOnAnimations ( Vector3 moveDir )
        {
            Vector3 relativeDir = transform.InverseTransformDirection( moveDir );
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat( AnimationConstants.AnimatorParamVertical, v, 0.2f, delta );
            anim.SetFloat( AnimationConstants.AnimatorParamHorizontal, h, 0.2f, delta );

        }

        private bool OnGround ( )
        {
            bool r = false;

            Vector3 origin = transform.position + ( Vector3.up * toGround );
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;
            RaycastHit hit;
            Debug.DrawRay( origin, dir * dis, Color.red );
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
        public void Init ( )
        {
            InitializeName();
            InitializeHealth();
            InitializeAnimator();
            InitializeRigidbody();
            InitializeInventory();
            InitializeActions();
            InitializeAnimationHook();
            InitializeMiscellaneous();
        }

        public void FixedTick ( float d )
        {
            delta = d;

            isBlocking = false;
            isAttacking = false;
            isParrying = false;
            isCasting = false;

            usingItem = anim.GetBool( AnimationConstants.AnimatorParamInteracting );

            DetectAction();
            DetectItemAction();

            if ( inventoryManager != null )
            {
                inventoryManager.rightHandWeapon.weaponModel.SetActive( !usingItem );
            }


            anim.SetBool( AnimationConstants.AnimatorParamBlocking, isBlocking );
            anim.SetBool( AnimationConstants.AnimatorParamIsLeft, isLeftHand );

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

            canMove = anim.GetBool( AnimationConstants.AnimatorParamCanMove );
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

            if ( onGround )
            {
                rigid.velocity = targetDir * ( targetSpeed * moveAmount );
            }

            Debug.DrawRay( transform.position, targetDir, Color.blue );

            Quaternion tr = Quaternion.LookRotation( targetDir );
            Quaternion targetRotation = Quaternion.Slerp( transform.rotation, tr, delta * moveAmount * rotateSpeed );
            transform.rotation = targetRotation;

            anim.SetBool( AnimationConstants.AnimatorParamLockOn, lockOn );

            if ( lockOn == false )
                HandleMovementAnimations();
            else
                HandleLockOnAnimations( moveDir );
        }

        public void Tick ( float d )
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool( AnimationConstants.AnimatorParamOnGround, onGround );
        }

        public void HandleTwoHanded ( )
        {
            if ( actionManager == null )
            {
                return;
            }

            anim.SetBool( AnimationConstants.AnimatorParamTwoHanded, isTwoHanded );

            actionManager.UpdateActionsWithCurrentWeapon( isTwoHanded );
        }

        public void IsGettingParried ( )
        {

        }

        public void Heal ( float amount )
        {
            _currentHealth = Mathf.Min( _maxiumumHealth, _currentHealth + amount );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color> <b>healed</b> for <b><color=green>{amount}</color></b> health.</size>", gameObject );
        }

        public void Damage ( float amount )
        {
            _currentHealth = Mathf.Max( 0, _currentHealth - amount );
            Debug.Log( $"<size=24>StateManager -> <color=#0d9eff>{_name}</color> <b>damaged</b> for <b><color=red>{amount}</color></b> health.</size>", gameObject );
        }
        #endregion

        #region Simulation Methods

#if UNITY_EDITOR
        [ContextMenu( "Simulating Initialization" )]
        private void SimulatingInitialization ( )
        {
            Init();
        }

        [ContextMenu( "Simulating Healing" )]
        private void SimulatingHealing ( )
        {
            Heal( 10 );
        }

        [ContextMenu( "Simulating Damage" )]
        private void SimulatingDamage ( )
        {
            Damage( 10 );
        }
#endif

        #endregion
    }
}
