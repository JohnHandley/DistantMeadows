using UnityEngine;

using DistantMeadows.Core.Utilities;
using DistantMeadows.Actors.Player.Models;

namespace DistantMeadows.Actors.Player.Controllers {
    public class PlayerInputManager : MonoBehaviour {
        public PlayerActionInputs _controls;

        public void Awake ( ) {
            _controls = new PlayerActionInputs();
        }

        public Controls Initialize ( ) {
            if ( _controls == null ) {
                return null;
            }

            Controls cont = new Controls();

            _controls.Player.InteractPickup.performed += ctx => { cont.InteractOrPickup = true; };
            _controls.Player.UseItem.performed += ctx => { cont.UseItem = true; };

            _controls.Player.Run.performed += ctx => { cont.Sprint = ctx.ReadValueAsButton(); };
            _controls.Player.Run.canceled += ctx => { cont.Sprint = ctx.ReadValueAsButton(); };

            _controls.Player.Dodge.performed += ctx => { cont.Dodge = true; };

            _controls.Player.Move.started += ctx => { cont.Move = ctx.ReadValue<Vector2>(); };
            _controls.Player.Move.performed += ctx => { cont.Move = ctx.ReadValue<Vector2>(); };
            _controls.Player.Move.canceled += ctx => { cont.Move = ctx.ReadValue<Vector2>(); };

            _controls.Player.TargetLockResetCamera.performed += ctx => { cont.TargetLockOrResetCamera = true; };
            _controls.Player.TwoHandRightWeapon.performed += ctx => { cont.TwoHandRightHand = true; };
            _controls.Player.TwoHandLeftWeapon.performed += ctx => { cont.TwoHandLeftHand = true; };

            _controls.Player.RightHandAction.performed += ctx => { cont.RightHandPrimaryAction = true; };
            _controls.Player.RightHandAlternateAction.performed += ctx => { cont.RightHandSecondaryAction = true; };
            _controls.Player.LeftHandAction.performed += ctx => { cont.LeftHandPrimaryAction = true; };
            _controls.Player.LeftHandAlternativeAction.performed += ctx => { cont.LeftHandSecondaryAction = true; };

            _controls.Player.Guard.performed += ctx => { cont.Block = ctx.ReadValueAsButton(); };
            _controls.Player.Guard.canceled += ctx => { cont.Block = ctx.ReadValueAsButton(); };

            _controls.Player.Look.started += ctx => { cont.Look = ctx.ReadValue<Vector2>(); };
            _controls.Player.Look.performed += ctx => { cont.Look = ctx.ReadValue<Vector2>(); };
            _controls.Player.Look.canceled += ctx => { cont.Look = ctx.ReadValue<Vector2>(); };

            return cont;
        }

        public void OnEnable ( ) {
            _controls.Enable();
        }

        public void OnDisable ( ) {
            _controls.Disable();
        }
    }
}
