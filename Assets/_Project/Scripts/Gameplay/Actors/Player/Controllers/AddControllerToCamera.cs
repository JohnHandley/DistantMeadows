using Cinemachine;

namespace DistantMeadows.Actors.Player.Controllers {
    public static class AddControllerToCamera {
        public static void Initialize ( PlayerStateManager player ) {
            CinemachineCore.GetInputAxis = ( string axisName ) => {
                if ( axisName == "Mouse X" ) {
                    return player.controls.Look.x;
                } else if ( axisName == "Mouse Y" ) {
                    return player.controls.Look.y;
                }
                return 0;
            };
        }
    }
}