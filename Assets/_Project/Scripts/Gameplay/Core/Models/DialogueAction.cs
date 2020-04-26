using DistantMeadows.Actors.Core.Enums;

namespace DistantMeadows.Core.Models {
    public class DialogueAction<T> {
        public string requester;
        public string responder;
        public T payload;

        public DialogueAction ( string req, string res, T p ) {
            requester = req;
            responder = res;
            payload = p;
        }
    }

    public class DialogueRequestPayload {
    }
    public class DialogueResponsePayload {
        public bool accept;

        public DialogueResponsePayload ( bool acc ) {
            accept = acc;
        }
    }
    public class DialogueStartPayload {
    }
    public class DialogueContinuePayload {
        public string dialogueScript;

        public DialogueContinuePayload ( string ds ) {
            dialogueScript = ds;
        }
    }
    public class DialogueEndPayload {
    }
    public class DialogueNextTextPayload {
        public string text;

        public DialogueNextTextPayload ( string t ) {
            text = t;
        }
    }
    public class DialogueTextRevealPayload {
        public char character;

        public DialogueTextRevealPayload ( char c ) {
            character = c;
        }
    }
    public class DialogueEmotionPayload {
        public Emotion emotion;

        public DialogueEmotionPayload ( Emotion e ) {
            emotion = e;
        }
    }
    public class DialogueEffectPayload {
        public string effect;

        public DialogueEffectPayload ( string e ) {
            effect = e;
        }
    }
    public class DialogueGesturePayload {
        public string gesture;

        public DialogueGesturePayload ( string g ) {
            gesture = g;
        }
    }
}
