using UnityEngine;
using System.Collections.Generic;

using DistantMeadows.Core.Models;
using DistantMeadows.Actors.Core.Behaviors;
using DistantMeadows.Actors.Player.Models;

namespace DistantMeadows.Actors.Player.Controllers
{
    public class PlayerStateManager : CharacterStateManager
    {
        [SerializeField]
        private PlayerInputManager _playerInput;

        public Camera cam;

        public Controls controls;

        public StateMachine<string> playerStateMachine;

        public override void Init()
        {
            base.Init();

            if (_playerInput != null)
            {
                controls = _playerInput.Initialize();
            }

            AddControllerToCamera.Initialize(this);

            State normal = new State(
                "normal",
                new List<StateAction>() //Fixed Update
                {
                    new UpdateCharacterPhysicsForMovement(this),
                    new MovePlayerCharacter(this),
                    new UpdateCharacterMovementAnimation(this)
                },
                new List<StateAction>() //Update
                {
                },
                new List<StateAction>()//Late Update
                {
                }
            );

            normal.onEnter = DisableRootMotion;

            State attackState = new State(
                "attack",
                new List<StateAction>() //Fixed Update
                {
                },
                new List<StateAction>() //Update
                {
                    new MonitorInteractingAnimation(this, "isInteracting", "normal"),
                },
                new List<StateAction>()//Late Update
                {
                }
            );

            attackState.onEnter = EnableRootMotion;

            playerStateMachine = new StateMachine<string>("PlayerState", normal, null);
        }

        private void FixedUpdate()
        {
            base.FixedTick();
            if (playerStateMachine.GetCurrentState() == null)
                return;
            playerStateMachine.GetCurrentState().FixedTick();
        }

        private void Update()
        {
            base.Tick();
            if (playerStateMachine.GetCurrentState() == null)
                return;
            playerStateMachine.GetCurrentState().Tick();
        }

        private void LateUpdate()
        {
            base.LateTick();
            if (playerStateMachine.GetCurrentState() == null)
                return;
            playerStateMachine.GetCurrentState().LateTick();
        }

        #region State Events
        void DisableRootMotion()
        {
            actorStates.useRootMotion = false;
        }

        void EnableRootMotion()
        {
            actorStates.useRootMotion = true;
        }

        #endregion
    }
}
