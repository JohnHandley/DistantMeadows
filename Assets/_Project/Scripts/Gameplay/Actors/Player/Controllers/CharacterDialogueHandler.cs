using UnityEngine;
using System;

using DistantMeadows.Actors.Core.Behaviors;
using DistantMeadows.Actors.NPC.Models;
using DistantMeadows.Actors.Core.Enums;
using DistantMeadows.Core.Models;

[System.Serializable]
public class CharacterDialogueHandler {
    [SerializeField]
    private CharacterDialogueTrigger dialogueTrigger;
    [SerializeField]
    private DialogueData dialogueLines;
    [SerializeField]
    private Transform dialogueParticles;
    [SerializeField]
    private DialogueAudio dialogueAudio;
    [SerializeField]
    private CharacterStateManager me;
    [SerializeField]
    private string potentialDialoguePartner;
    [SerializeField]
    private string currentDialoguePartner;
    [SerializeField]
    private bool isInDialogue = false;
    [SerializeField]
    private int currentDialogueLine = 0;
    private DialogueEvents dialogue;

    public CharacterDialogueHandler ( ) {
    }

    public void Init ( CharacterStateManager character ) {
        me = character;
        dialogue = DialogueEvents.current;
        InitializeDialogueHandling();
    }

    public void Update ( ) {
        string partner = dialogueTrigger.GetDialogueTarget();
        if ( !String.IsNullOrEmpty( partner ) ) {
            potentialDialoguePartner = partner;
        }
    }

    public void Destroy ( ) {
        TearDownDialogueHandling();
    }

    #region Dialogue
    private void InitializeDialogueHandling ( ) {
        if ( dialogue == null )
            return;

        dialogue.onRequest += OnDialogueRequest;
        dialogue.onResponse += OnDialogueResponse;
        dialogue.onStart += OnDialogueStart;
        dialogue.onContinue += OnDialogueContinue;
        dialogue.onEnd += OnDialogueEnd;
        dialogue.onEmotion += OnDialogueEmotion;
        dialogue.onEffect += OnDialogueEffect;
        dialogue.onGesture += OnDialogueGesture;
    }

    private void TearDownDialogueHandling ( ) {
        if ( dialogue == null )
            return;

        dialogue.onRequest -= OnDialogueRequest;
        dialogue.onResponse -= OnDialogueResponse;
        dialogue.onStart -= OnDialogueStart;
        dialogue.onContinue -= OnDialogueContinue;
        dialogue.onEnd -= OnDialogueEnd;
        dialogue.onEmotion -= OnDialogueEmotion;
        dialogue.onEffect -= OnDialogueEffect;
        dialogue.onGesture -= OnDialogueGesture;
    }

    private void OnDialogueRequest ( DialogueAction<DialogueRequestPayload> conversation ) {
        if ( conversation.responder == me.characterInfo.name ) {
            if ( !isInDialogue && String.IsNullOrEmpty( currentDialoguePartner ) ) {
                currentDialoguePartner = conversation.requester;
            }

            Debug.Log( $"{me.characterInfo.name} Is {( !isInDialogue ? "" : "Not " )}Accepting A Request From {currentDialoguePartner}" );
            dialogue.TriggerResponse(
                new DialogueAction<DialogueResponsePayload>(
                    me.characterInfo.name,
                    currentDialoguePartner,
                    new DialogueResponsePayload( !isInDialogue )
                )
            );
        }
    }

    private void OnDialogueResponse ( DialogueAction<DialogueResponsePayload> conversation ) {
        if ( conversation.requester == potentialDialoguePartner
            && conversation.responder == me.characterInfo.name
            && conversation.payload.accept ) {

            if ( String.IsNullOrEmpty( currentDialoguePartner ) ) {
                currentDialoguePartner = conversation.requester;
            }

            Debug.Log( $"{me.characterInfo.name} Starting Conversation With {currentDialoguePartner}" );
            dialogue.TriggerStart(
                new DialogueAction<DialogueStartPayload>(
                    me.characterInfo.name,
                    currentDialoguePartner,
                    new DialogueStartPayload()
                )
            );
        }
    }

    private void OnDialogueStart ( DialogueAction<DialogueStartPayload> conversation ) {
        me.actorStates.isAbleToMove = false;
        isInDialogue = true;

        if ( conversation.responder == me.characterInfo.name ) {
            Debug.Log( $"Dialogue Started, {me.characterInfo.name}'s talking to {currentDialoguePartner}" );

            dialogue.TriggerContinue(
                new DialogueAction<DialogueContinuePayload>(
                    me.characterInfo.name,
                    currentDialoguePartner,
                    new DialogueContinuePayload(
                        dialogueLines != null
                            ? dialogueLines.conversationBlock[ currentDialogueLine ]
                            : ""
                    )
                )
            );
        }
    }

    private void OnDialogueContinue ( DialogueAction<DialogueContinuePayload> conversation ) {
        if ( conversation.requester == currentDialoguePartner
            && conversation.responder == me.characterInfo.name ) {
            Debug.Log( $"{me.characterInfo.name} attempting respond to {currentDialoguePartner}" );
            if ( dialogueLines != null && dialogueLines.conversationBlock != null ) {
                currentDialogueLine++;
                if ( currentDialogueLine >= dialogueLines.conversationBlock.Count ) {
                    Debug.Log( $"{me.characterInfo.name} ending convo with {currentDialoguePartner}" );
                    dialogue.TriggerEnd(
                        new DialogueAction<DialogueEndPayload>(
                            me.characterInfo.name,
                            currentDialoguePartner,
                            new DialogueEndPayload()
                        )
                    );
                } else {
                    Debug.Log( $"{me.characterInfo.name} responding to {currentDialoguePartner}" );
                    dialogue.TriggerContinue(
                        new DialogueAction<DialogueContinuePayload>(
                            me.characterInfo.name,
                            currentDialoguePartner,
                            new DialogueContinuePayload(
                                dialogueLines.conversationBlock[ currentDialogueLine ]
                            )
                        )
                    );
                }
            }
        }
    }

    private void OnDialogueEnd ( DialogueAction<DialogueEndPayload> conversation ) {
        me.actorStates.isAbleToMove = true;
        isInDialogue = false;
    }

    private void OnDialogueEmotion ( DialogueAction<DialogueEmotionPayload> conversation ) {
        if ( conversation.requester != me.characterInfo.name )
            return;

        PlayEmotion( conversation.payload.emotion );
    }

    private void OnDialogueEffect ( DialogueAction<DialogueEffectPayload> conversation ) {
        if ( conversation.requester != me.characterInfo.name )
            return;

        PlayEffect( conversation.payload.effect );
    }

    private void OnDialogueGesture ( DialogueAction<DialogueGesturePayload> conversation ) {
        if ( conversation.requester != me.characterInfo.name )
            return;

        PlayGesture( conversation.payload.gesture );
    }
    #endregion

    #region Private Methods
    private void PlayGesture ( string gesture ) {
        if ( me.anim == null )
            return;

        Debug.Log( $"Villager {me.characterInfo.name}: Playing Gesture {gesture}" );
    }

    private void PlayEmotion ( Emotion emotion ) {
        if ( me.anim == null )
            return;

        Debug.Log( $"Villager {me.characterInfo.name}: Playing Emotion {emotion}" );

        //animator.SetTrigger( e.ToString() );

        //if ( e == Emotion.suprised )
        //    eyesRenderer.material.SetTextureOffset( "_BaseMap", new Vector2( .33f, 0 ) );

        //if ( e == Emotion.angry )
        //    eyesRenderer.material.SetTextureOffset( "_BaseMap", new Vector2( .66f, 0 ) );

        //if ( e == Emotion.sad )
        //    eyesRenderer.material.SetTextureOffset( "_BaseMap", new Vector2( .33f, -.33f ) );
    }

    private void PlayEffect ( string effect ) {
        PlayParticle( effect );
        PlayAudio( effect );

        Debug.Log( $"Villager {me.characterInfo.name}: Playing Effect {effect}" );
    }

    private void PlayParticle ( string action ) {
        if ( dialogueParticles == null )
            return;

        if ( dialogueParticles.Find( action + "Particle" ) == null )
            return;
        dialogueParticles.Find( action + "Particle" ).GetComponent<ParticleSystem>().Play();
    }

    private void PlayAudio ( string action ) {
        if ( dialogueAudio == null )
            return;

        AudioClip clip = null;
        switch ( action ) {
            case "sparkle":
                clip = dialogueAudio.sparkleClip;
                break;
            case "rain":
                clip = dialogueAudio.rainClip;
                break;
        }

        if ( clip != null ) {
            dialogueAudio.effectSource.clip = clip;
            dialogueAudio.effectSource.Play();
        }
    }
    #endregion

    #region Public Methods
    public void RequestConversation ( ) {
        if ( me == null || potentialDialoguePartner == null )
            return;

        if ( isInDialogue ) {
            string message = "";
            if ( dialogueLines != null
                && dialogueLines.conversationBlock != null
                && currentDialogueLine < dialogueLines.conversationBlock.Count ) {
                message = dialogueLines.conversationBlock[ currentDialogueLine ];
            }
            Debug.Log( $"{me.characterInfo.name} Is Requesting Another Response From {currentDialoguePartner}" );
            dialogue.TriggerContinue(
                new DialogueAction<DialogueContinuePayload>(
                    me.characterInfo.name,
                    currentDialoguePartner,
                    new DialogueContinuePayload( message )
                )
            );
        } else {
            Debug.Log( $"{me.characterInfo.name} Is Requesting To Have a Conversation With {potentialDialoguePartner}" );
            dialogue.TriggerRequest(
                new DialogueAction<DialogueRequestPayload>(
                    me.characterInfo.name,
                    potentialDialoguePartner,
                    new DialogueRequestPayload()
                )
            );
        }
    }
    #endregion
}
