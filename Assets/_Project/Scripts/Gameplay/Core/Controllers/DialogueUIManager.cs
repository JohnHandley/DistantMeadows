using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;
using DistantMeadows.Core.Models;

namespace DistantMeadows.Core.Controllers {
    public class DialogueUIManager : MonoBehaviour {
        #region Properties

        #region Editor Variables
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private TextMeshProUGUI animatedText;
        [SerializeField]
        private Image nameBubble;
        [SerializeField]
        private TextMeshProUGUI nameTMP;
        [SerializeField]
        private bool inDialogue;
        #endregion

        #region Event Systems
        private DialogueEvents dialogue;
        #endregion

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
        #endregion

        #region Dialogue
        private void InitializeDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart += OnDialogueStart;
            dialogue.onNextText += OnDialogueNextText;
            dialogue.onEnd += OnDialogueEnd;
            dialogue.onTextReveal += OnDialogueTextReveal;
        }

        private void TearDownDialogueHandling ( ) {
            if ( dialogue == null )
                return;

            dialogue.onStart -= OnDialogueStart;
            dialogue.onNextText -= OnDialogueNextText;
            dialogue.onEnd -= OnDialogueEnd;
            dialogue.onTextReveal -= OnDialogueTextReveal;
        }

        private void OnDialogueStart ( DialogueAction<DialogueStartPayload> conversation ) {
            ResetTextBubble();
            SetCharNameAndColor();
            inDialogue = true;
            FadeUI( true, .2f, .65f );
        }

        private void OnDialogueEnd ( DialogueAction<DialogueEndPayload> conversation ) {
            FadeUI( false, .2f, .65f );
            ResetTextBubble();
            inDialogue = false;
        }

        private void OnDialogueNextText ( DialogueAction<DialogueNextTextPayload> conversation ) {
            ResetTextBubble();
            animatedText.text = conversation.payload.text;
        }

        private void OnDialogueTextReveal ( DialogueAction<DialogueTextRevealPayload> conversation ) {
            animatedText.maxVisibleCharacters++;
        }
        #endregion

        #region Private Methods
        private void ResetTextBubble ( ) {
            animatedText.text = string.Empty;
            animatedText.maxVisibleCharacters = 0;
        }

        private void SetCharNameAndColor ( ) {
            nameTMP.text = "Name";
            nameTMP.color = Color.black;
            nameBubble.color = Color.white;
        }

        private void FadeUI ( bool show, float time, float delay ) {
            Sequence s = DOTween.Sequence();
            s.AppendInterval( delay );
            s.Append( canvasGroup.DOFade( show ? 1 : 0, time ) );
            if ( show ) {
                s.Join( canvasGroup.transform.DOScale( 0, time * 2 ).From().SetEase( Ease.OutBack ) );
            }
        }
        #endregion
    }
}