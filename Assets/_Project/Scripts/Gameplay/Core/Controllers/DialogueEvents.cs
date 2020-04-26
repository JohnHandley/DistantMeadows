using System;
using UnityEngine;
using System.Collections;

using DistantMeadows.Actors.Core.Enums;
using DistantMeadows.Core.Models;

public class DialogueEvents : MonoBehaviour {
    public static DialogueEvents current;

    [SerializeField]
    private float speed = 10;

    private void Awake ( ) {
        if ( current == null ) {
            current = this;
        } else if ( current != null ) {
            Destroy( gameObject );
        }

        Debug.Log( "Dialogue System Started" );
    }

    public event Action<DialogueAction<DialogueRequestPayload>> onRequest;
    public void TriggerRequest ( DialogueAction<DialogueRequestPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onRequest?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueResponsePayload>> onResponse;
    public void TriggerResponse ( DialogueAction<DialogueResponsePayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;

        onResponse?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueStartPayload>> onStart;
    public void TriggerStart ( DialogueAction<DialogueStartPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onStart?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueContinuePayload>> onContinue;
    public void TriggerContinue ( DialogueAction<DialogueContinuePayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        if ( !String.IsNullOrEmpty( conversation.payload.dialogueScript ) ) {
            ReadText( conversation );
        }
        onContinue?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueNextTextPayload>> onNextText;
    public void TriggerNextText ( DialogueAction<DialogueNextTextPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onNextText?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueTextRevealPayload>> onTextReveal;
    public void TriggerTextReveal ( DialogueAction<DialogueTextRevealPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onTextReveal?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueEndPayload>> onEnd;
    public void TriggerEnd ( DialogueAction<DialogueEndPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onEnd?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueEffectPayload>> onEffect;
    public void TriggerEffect ( DialogueAction<DialogueEffectPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onEffect?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueEmotionPayload>> onEmotion;
    public void TriggerEmotion ( DialogueAction<DialogueEmotionPayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onEmotion?.Invoke( conversation );
    }

    public event Action<DialogueAction<DialogueGesturePayload>> onGesture;
    public void TriggerGesture ( DialogueAction<DialogueGesturePayload> conversation ) {
        if ( String.IsNullOrEmpty( conversation.requester ) || String.IsNullOrEmpty( conversation.responder ) )
            return;
        onGesture?.Invoke( conversation );
    }

    public void ReadText ( DialogueAction<DialogueContinuePayload> conversation ) {
        // split the whole text into parts based off the <> tags 
        // even numbers in the array are text, odd numbers are tags
        string[] subTexts = conversation.payload.dialogueScript.Split( '<', '>' );

        // textmeshpro still needs to parse its built-in tags, so we only include noncustom tags
        string displayText = "";
        for ( int i = 0; i < subTexts.Length; i++ ) {
            if ( i % 2 == 0 )
                displayText += subTexts[ i ];
            else if ( !isCustomTag( subTexts[ i ].Replace( " ", "" ) ) )
                displayText += $"<{subTexts[ i ]}>";
        }
        // check to see if a tag is our own
        bool isCustomTag ( string tag ) {
            return tag.StartsWith( "speed=" )
                || tag.StartsWith( "pause=" )
                || tag.StartsWith( "emotion=" )
                || tag.StartsWith( "gesture=" )
                || tag.StartsWith( "effect=" );
        }

        // send that string to textmeshpro and hide all of it, then start reading
        TriggerNextText(
            new DialogueAction<DialogueNextTextPayload>(
                conversation.requester,
                conversation.responder,
                new DialogueNextTextPayload( displayText )
            )
        );
        ;
        StartCoroutine( Read() );

        IEnumerator Read ( ) {
            int subCounter = 0;
            int visibleCounter = 0;
            while ( subCounter < subTexts.Length ) {
                if ( subCounter % 2 == 1 ) {
                    yield return EvaluateTag( subTexts[ subCounter ].Replace( " ", "" ) );
                } else {
                    while ( visibleCounter < subTexts[ subCounter ].Length ) {
                        TriggerTextReveal(
                            new DialogueAction<DialogueTextRevealPayload>(
                                conversation.requester,
                                conversation.responder,
                                new DialogueTextRevealPayload(
                                    subTexts[ subCounter ][ visibleCounter ]
                                )
                            )
                        );
                        visibleCounter++;
                        yield return new WaitForSeconds( 1f / speed );
                    }
                    visibleCounter = 0;
                }
                subCounter++;
            }
            yield return null;

            WaitForSeconds EvaluateTag ( string tag ) {
                if ( tag.Length > 0 ) {
                    if ( tag.StartsWith( "speed=" ) ) {
                        speed = float.Parse( tag.Split( '=' )[ 1 ] );
                    } else if ( tag.StartsWith( "pause=" ) ) {
                        return new WaitForSeconds( float.Parse( tag.Split( '=' )[ 1 ] ) );
                    } else if ( tag.StartsWith( "emotion=" ) ) {
                        onEmotion?.Invoke(
                            new DialogueAction<DialogueEmotionPayload>(
                                conversation.requester,
                                conversation.responder,
                                new DialogueEmotionPayload(
                                    (Emotion) System.Enum.Parse( typeof( Emotion ), tag.Split( '=' )[ 1 ] )
                                )
                            )
                        );
                    } else if ( tag.StartsWith( "gesture=" ) ) {
                        onGesture?.Invoke(
                            new DialogueAction<DialogueGesturePayload>(
                                conversation.requester,
                                conversation.responder,
                                new DialogueGesturePayload(
                                    tag.Split( '=' )[ 1 ]
                                )
                            )
                        );
                    } else if ( tag.StartsWith( "effect=" ) ) {
                        onEffect?.Invoke(
                            new DialogueAction<DialogueEffectPayload>(
                                conversation.requester,
                                conversation.responder,
                                new DialogueEffectPayload(
                                    tag.Split( '=' )[ 1 ]
                                )
                            )
                        );
                    }
                }
                return null;
            }
        }
    }
}
