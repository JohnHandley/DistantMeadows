using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

using DistantMeadows.Core.Enum;
using System;
using DistantMeadows.Core.Models;

namespace DistantMeadows.Core.Controllers {
    public class PostProcessingManager : MonoBehaviour {
        #region Properties

        #region Editor Variables
        [SerializeField]
        private Volume freeLookPost;
        [SerializeField]
        private Volume lockonPost;
        [SerializeField]
        private Volume dialoguePost;
        #endregion

        #region Event Systems
        private DialogueEvents dialogue;
        #endregion

        private GameModeType previousGameMode;
        #endregion

        #region Unity States
        private void Start ( ) {
            ConnectToDependencies();
            InitializeDialogueHandling();
        }

        private void OnDestroy ( ) {
            TearDownDialogueHandling();
        }
        #endregion

        #region Gameplay
        private void ConnectToDependencies ( ) {
            dialogue = DialogueEvents.current;
        }

        private void ChangeGameModeTo ( GameModeType type ) {
            previousGameMode = type;

            float dialogueWeight = type == GameModeType.Dialogue ? 1 : 0;
            float lockonWeight = type == GameModeType.Lockon ? 1 : 0;

            DOVirtual.Float( dialoguePost.weight, dialogueWeight, .8f, DialoguePost );
            DOVirtual.Float( lockonPost.weight, lockonWeight, .8f, LockonPost );
        }
        #endregion

        #region Lockon
        public void LockonPost ( float x ) {
            lockonPost.weight = x;
        }
        #endregion

        #region Dialogue
        public void DialoguePost ( float x ) {
            dialoguePost.weight = x;
        }

        private void InitializeDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart += OnDialogueStarted;
            dialogue.onEnd += OnDialogueEnded;
        }

        private void TearDownDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart -= OnDialogueStarted;
            dialogue.onEnd -= OnDialogueEnded;
        }

        public void OnDialogueStarted ( DialogueAction<DialogueStartPayload> conversation ) {
            ChangeGameModeTo( GameModeType.Dialogue );
        }

        public void OnDialogueEnded ( DialogueAction<DialogueEndPayload> conversation ) {
            ChangeGameModeTo( previousGameMode );
        }
        #endregion
    }
}
