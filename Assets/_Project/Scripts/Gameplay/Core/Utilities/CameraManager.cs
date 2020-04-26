using UnityEngine;

using DistantMeadows.Core.Enum;
using Cinemachine;
using System;
using DistantMeadows.Core.Models;

namespace DistantMeadows.Core.Controllers {
    public class CameraManager : MonoBehaviour {
        #region Properties

        #region Editor Variables
        [SerializeField]
        private CinemachineBrain cinemachineBrain;
        [SerializeField]
        private CinemachineImpulseSource impulseController;
        [SerializeField]
        private GameObject freeLookCameraObject;
        [SerializeField]
        private GameObject lockonCameraObject;
        [SerializeField]
        private GameObject dialogueCameraObject;
        #endregion

        #region Cinemachine Cameras
        private CinemachineFreeLook freeLookCamera;
        private CinemachineVirtualCamera lockonCamera;
        private CinemachineVirtualCamera dialogueCamera;
        #endregion

        #region Event Systems
        private DialogueEvents dialogue;
        #endregion

        private GameModeType previousGameMode;
        #endregion

        #region Unity States
        public void Awake ( ) {
            InitializeCameras();
        }

        public void Start ( ) {
            ConnectToDependencies();
            InitializeDialogueHandling();
        }

        public void OnDestroy ( ) {
            TearDownDialogueHandling();
        }
        #endregion

        #region Camera Management
        private void InitializeCameras ( ) {
            if ( freeLookCameraObject != null ) {
                freeLookCamera = freeLookCameraObject.GetComponent<CinemachineFreeLook>();
            }
            if ( lockonCameraObject != null ) {
                lockonCamera = lockonCameraObject.GetComponent<CinemachineVirtualCamera>();
            }
            if ( dialogueCameraObject != null ) {
                dialogueCamera = dialogueCameraObject.GetComponent<CinemachineVirtualCamera>();
            }
        }
        #endregion

        #region Gameplay
        private void ConnectToDependencies ( ) {
            dialogue = DialogueEvents.current;
        }

        private void ChangeGameModeTo ( GameModeType type ) {
            previousGameMode = type;
            switch ( type ) {
                case GameModeType.Free:
                    freeLookCameraObject.SetActive( true );
                    lockonCameraObject.SetActive( false );
                    dialogueCameraObject.SetActive( false );
                    break;
                case GameModeType.Lockon:
                    freeLookCameraObject.SetActive( false );
                    lockonCameraObject.SetActive( true );
                    dialogueCameraObject.SetActive( false );
                    break;
                case GameModeType.Dialogue:
                    freeLookCameraObject.SetActive( false );
                    lockonCameraObject.SetActive( false );
                    dialogueCameraObject.SetActive( true );
                    break;
            }
        }
        #endregion

        #region Dialogue
        private void InitializeDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart += OnDialogueStart;
            dialogue.onEnd += OnDialogueEnd;
            dialogue.onEffect += OnDialogueEffect;
        }

        private void TearDownDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart -= OnDialogueStart;
            dialogue.onEnd -= OnDialogueEnd;
            dialogue.onEffect -= OnDialogueEffect;
        }

        public void OnDialogueStart ( DialogueAction<DialogueStartPayload> conversation ) {
            ChangeGameModeTo( GameModeType.Dialogue );
        }

        public void OnDialogueEnd ( DialogueAction<DialogueEndPayload> conversation ) {
            ChangeGameModeTo( previousGameMode );
        }

        public void OnDialogueEffect ( DialogueAction<DialogueEffectPayload> conversation ) {
            if ( impulseController == null )
                return;

            if ( conversation.payload.effect.ToLower() == "shake" ) {
                impulseController.GenerateImpulse();
            }
        }
        #endregion
    }
}

