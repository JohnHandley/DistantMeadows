using UnityEngine;

using DistantMeadows.Actors.Enemies.Behaviors;
using DistantMeadows.UI.Behaviors;
using DistantMeadows.Core.Utilities;

namespace DistantMeadows.Actors.Player.Behaviors {
    public class InputHandler : MonoBehaviour {
        private StateManager _states;
        private CameraManager _camManager;
        private PlayerActionInputs _controls;
        private QuickSlotManager _quickSlotManager;
        private EnemyManager _enemyManager;

        public void Awake ( ) {
            _controls = new PlayerActionInputs();
        }

        public void OnEnable ( ) {
            _controls.Enable();
        }

        public void OnDisable ( ) {
            _controls.Disable();
        }

        private void Start ( ) {
            InitializeInputs();
            InitializeState();
            InitializeCamera();
            InitializeQuickSlots();
            InitalizeEnemyManager();
        }

        private void FixedUpdate ( ) {
            if ( _states == null || _camManager == null ) {
                return;
            }

            Vector3 v = _states.vertical * _camManager.transform.forward;
            Vector3 h = _states.horizontal * _camManager.transform.right;
            _states.moveDir = ( v + h ).normalized;

            _states.FixedTick( Time.fixedDeltaTime );
            _camManager.Tick( Time.fixedDeltaTime );

            ResetInputState();
        }

        private void Update ( ) {
            if ( _states == null ) {
                return;
            }

            if ( _states.lockOnTarget != null ) {
                if ( _states.lockOnTarget.eStates.isDead ) {
                    _states.lockOn = false;
                    _states.lockOnTarget = null;
                    _states.lockOnTransform = null;
                    _camManager.lockon = false;
                    _camManager.lockonTarget = null;
                }
            }

            _states.Tick( Time.deltaTime );
        }

        private void InitializeInputs ( ) {
            _controls.Player.UseItem.performed += ctx => { _states.itemInput = true; };

            _controls.Player.Run.performed += ctx => { _states.run = ctx.ReadValueAsButton(); };
            _controls.Player.Run.canceled += ctx => { _states.run = ctx.ReadValueAsButton(); };

            _controls.Player.Dodge.performed += ctx => { _states.rollInput = true; };

            _controls.Player.Move.started += ctx => { Move( ctx.ReadValue<Vector2>() ); };
            _controls.Player.Move.performed += ctx => { Move( ctx.ReadValue<Vector2>() ); };
            _controls.Player.Move.canceled += ctx => { Move( ctx.ReadValue<Vector2>() ); };

            _controls.Player.TargetLockResetCamera.performed += ctx => { LockOn(); };
            _controls.Player.TwoHandRightWeapon.performed += ctx => { TwoHandRightWeapon(); };
            _controls.Player.TwoHandLeftWeapon.performed += ctx => { TwoHandLeftWeapon(); };

            _controls.Player.RightHandAction.performed += ctx => { _states.rb = true; };
            _controls.Player.RightHandAlternateAction.performed += ctx => { _states.rt = true; };
            _controls.Player.LeftHandAction.performed += ctx => { _states.lb = true; };
            _controls.Player.LeftHandAlternativeAction.performed += ctx => { _states.lt = true; };

            _controls.Player.Guard.performed += ctx => { _states.lb = ctx.ReadValueAsButton(); };
            _controls.Player.Guard.canceled += ctx => { _states.lb = ctx.ReadValueAsButton(); };
        }

        private void InitializeState ( ) {
            _states = GetComponent<StateManager>();
            if ( _states == null ) {
                Debug.Log( $"<size=24>InputHandler -> StateManager <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }
            _states.Init();
            Debug.Log( $"<size=24>InputHandler -> StateManager <b>Initialized</b></size>", gameObject );
        }

        private void InitializeCamera ( ) {
            _camManager = CameraManager.singleton;
            if ( _camManager == null ) {
                Debug.Log( $"<size=24>InputHandler -> CameraManager <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }
            _camManager.Init( _states, _controls );
            Debug.Log( $"<size=24>InputHandler -> CameraManager <b>Initialized</b></size>", gameObject );
        }

        private void InitializeQuickSlots ( ) {
            _quickSlotManager = QuickSlotManager.singleton;
            if ( _quickSlotManager == null ) {
                Debug.Log( $"<size=24>InputHandler -> QuickSlotManager <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }
            _quickSlotManager.Init();
            Debug.Log( $"<size=24>InputHandler -> QuickSlotManager <b>Initialized</b></size>", gameObject );
        }

        private void InitalizeEnemyManager ( ) {
            _enemyManager = EnemyManager.singleton;
            if ( _enemyManager == null ) {
                Debug.Log( $"<size=24>InputHandler -> EnemyManager <color=red><b>Not Found</b></color></size>", gameObject );
                return;
            }
            Debug.Log( $"<size=24>InputHandler -> EnemyManager <b>Initialized</b></size>", gameObject );
        }

        private void Move ( Vector2 movementDelta ) {
            if ( _states == null ) {
                return;
            }

            _states.horizontal = movementDelta.x;
            _states.vertical = movementDelta.y;

            float m = Mathf.Abs( _states.horizontal ) + Mathf.Abs( _states.vertical );
            _states.moveAmount = Mathf.Clamp01( m );
        }

        private void LockOn ( ) {
            Debug.Log( $"Locking On" );
            if ( _states == null || _enemyManager == null || _camManager == null ) {
                return;
            }


            _states.lockOn = !_states.lockOn;

            _states.lockOnTarget = _enemyManager.GetEnemy( transform.position );
            if ( _states.lockOnTarget == null ) {
                _states.lockOn = false;
            }

            _states.lockOnTransform = _states.lockOnTarget.GetTarget();
            _camManager.lockonTarget = _states.lockOnTarget;
            _camManager.lockonTransform = _states.lockOnTransform;
            _camManager.lockon = _states.lockOn;
        }

        private void TwoHandRightWeapon ( ) {
            Debug.Log( $"Two Handing Right Weapon" );
            if ( _states == null ) {
                return;
            }
            _states.isTwoHanded = !_states.isTwoHanded;
            _states.HandleTwoHanded();
        }

        private void TwoHandLeftWeapon ( ) {
            Debug.Log( $"Two Handing Left Weapon" );
            // _states.isTwoHanded = !_states.isTwoHanded;
            // _states.HandleTwoHanded();
        }

        private void ResetInputState ( ) {
            _states.itemInput = false;
            _states.rollInput = false;

            _states.rb = false;
            _states.rt = false;
            _states.lt = false;
        }
    }
}
