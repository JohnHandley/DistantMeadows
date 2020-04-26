using UnityEngine;

using DistantMeadows.Actors.Enemies.Behaviors;
using DistantMeadows.Core.Contants;
using DistantMeadows.Core.Utilities;
using UnityEngine.InputSystem;
using Cinemachine;

namespace DistantMeadows.Actors.Player.Behaviors
{
    public class CameraManager : MonoBehaviour
    {
        public bool useManualCam = false;

        public bool lockon;
        public float followSpeed = 9;
        public float mouseSpeed = 2;
        public float controllerSpeed = 7;

        public Transform target;
        public EnemyTarget lockonTarget;
        public Transform lockonTransform;
        public Transform camTrans;

        private StateManager states;
        private PlayerActionInputs _controls;
        private Vector2 _look;

        float turnSmoothing = .1f;
        public float minAngle = -35;
        public float maxAngle = 35;

        float smoothX;
        float smoothY;
        float smoothXvelocity;
        float smoothYvelocity;
        public float lookAngle;
        public float tiltAngle;

        bool usedRightAxis;

        public bool ChangeTargetLeft;
        public bool ChangeTargetRight;

        public static CameraManager singleton = null;
        void Awake ( )
        {
            if ( camTrans != null )
            {
                singleton = this;
            }
        }

        public void Init ( StateManager st, PlayerActionInputs cont )
        {
            states = st;
            target = st.transform;

            _controls = cont;
            _controls.Player.Look.started += ctx => { _look = ctx.ReadValue<Vector2>(); };
            _controls.Player.Look.performed += ctx => { _look = ctx.ReadValue<Vector2>(); };
            _controls.Player.Look.canceled += ctx => { _look = ctx.ReadValue<Vector2>(); };

            CinemachineCore.GetInputAxis = GetAxisCustom;
        }

        public void Tick ( float d )
        {
            float h = _look.x;
            float v = _look.y;

            float c_h = _look.x;
            float c_v = _look.y;

            if ( lockonTarget != null )
            {
                if ( lockonTransform == null )
                {
                    lockonTransform = lockonTarget.GetTarget();
                    states.lockOnTransform = lockonTransform;
                }

                if ( Mathf.Abs( c_h ) > 0.6f )
                {
                    if ( !usedRightAxis )
                    {
                        lockonTransform = lockonTarget.GetTarget( ( c_h > 0 ) );
                        states.lockOnTransform = lockonTransform;
                        usedRightAxis = true;
                    }
                }
            }

            if ( ChangeTargetLeft || ChangeTargetRight )
            {
                lockonTransform = lockonTarget.GetTarget( ChangeTargetLeft );
                states.lockOnTransform = lockonTransform;
            }

            if ( usedRightAxis )
            {
                if ( Mathf.Abs( c_h ) < 0.6f )
                {
                    usedRightAxis = false;
                }
            }

            if ( !useManualCam )
            {
                HandleRotation2();
            }
            else
            {
                FollowTarget( d );
                HandleRotations( d, v, h );
            }
        }

        void FollowTarget ( float d )
        {
            float speed = d * followSpeed;
            Vector3 targetPosition = Vector3.Lerp( transform.position, target.position, speed );
            transform.position = targetPosition;
        }

        void HandleRotations ( float d, float v, float h )
        {
            if ( turnSmoothing > 0 )
            {
                smoothX = Mathf.SmoothDamp( smoothX, h, ref smoothXvelocity, turnSmoothing );
                smoothY = Mathf.SmoothDamp( smoothY, v, ref smoothYvelocity, turnSmoothing );
            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            tiltAngle -= smoothY;
            tiltAngle = Mathf.Clamp( tiltAngle, minAngle, maxAngle );
            //pivot.localRotation = Quaternion.Euler( tiltAngle, 0, 0 );


            if ( lockon && lockonTarget != null )
            {
                Vector3 targetDir = lockonTransform.position - transform.position;
                targetDir.Normalize();
                //targetDir.y = 0;

                if ( targetDir == Vector3.zero )
                    targetDir = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation( targetDir );
                transform.rotation = Quaternion.Slerp( transform.rotation, targetRot, d * 9 );
                lookAngle = transform.eulerAngles.y;
                return;
            }

            lookAngle += smoothX;
            transform.rotation = Quaternion.Euler( 0, lookAngle, 0 );
        }

        void HandleRotation2 ( )
        {
            float h = states.horizontal;
            float v = states.vertical;

            Vector3 targetDir = camTrans.transform.forward * v;
            targetDir += camTrans.transform.right * h;
            targetDir.Normalize();

            targetDir.y = 0;
            if ( targetDir == Vector3.zero )
                targetDir = states.transform.forward;

            Quaternion tr = Quaternion.LookRotation( targetDir );
            Quaternion targetRotation = Quaternion.Slerp(
                states.transform.rotation, tr,
                states.delta * states.moveAmount * states.rotateSpeed );

            states.transform.rotation = targetRotation;
        }

        public float GetAxisCustom ( string axisName )
        {
            if ( axisName == "Mouse X" )
            {
                return _look.x;
            }
            else if ( axisName == "Mouse Y" )
            {
                return _look.y;
            }
            return 0;
        }
    }
}
