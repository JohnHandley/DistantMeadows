using UnityEngine;

namespace Player.Domain
{
    public static class MouseAndKeyboard
    {
        public static Controls GetControls ( )
        {
            Controls cont = new Controls();

            cont.Vertical = Input.GetAxis( "Vertical" );
            cont.Horizontal = Input.GetAxis( "Horizontal" );
            cont.Sprint = Input.GetButton( "Sprint" );
            cont.Jump = Input.GetButtonUp( "Jump" );
            cont.LeftHandAction = Input.GetButton( "Block" );
            cont.ActionGesture = Input.GetButtonUp( "ActionGesture" );
            cont.TwoHand = Input.GetButtonUp( "TwoHand" );
            cont.Lockon = Input.GetButtonUp( "Lockon" );
            cont.Roll = cont.Sprint;
            cont.ChangeTarget = Input.GetButtonUp( "ChangeTarget" );

            cont.MouseX = Input.GetAxis( "Mouse X" );
            cont.MouseY = Input.GetAxis( "Mouse Y" );

            return cont;
        }

    }
}


