using UnityEngine;

namespace SA
{
    public class CameraManager : MonoBehaviour
    {
        public bool lockon;
        public float followSpeed = 9;
        public float mouseSpeed = 2;
        public float controllerSpeed = 7;

        public Transform target;
        public EnemyTarget lockonTarget;
        public Transform lockonTransform;

        [HideInInspector]
        public Transform pivot;
        [HideInInspector]
        public Transform camTrans;
        StateManager states;

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

        public void Init(StateManager st)
        {
            states = st;
            target = st.transform;

            camTrans = Camera.main.transform;
            pivot = camTrans.parent;
        }


        public void Tick(float d, Controls cont)
        {
            float targetSpeed = mouseSpeed;

            if (lockonTarget != null)
            {
                if (lockonTransform == null)
                {
                    lockonTransform = lockonTarget.GetTarget();
                    states.lockOnTransform = lockonTransform;
                }

                if (cont.ChangeTarget)
                {
                    if (!usedRightAxis)
                    {
                        lockonTransform = lockonTarget.GetTarget(cont.ChangeTarget);
                        states.lockOnTransform = lockonTransform;
                        usedRightAxis = true;
                    }
                }
            }

            if (usedRightAxis)
            {
                if (!cont.ChangeTarget)
                {
                    usedRightAxis = false;
                }
            }

            FollowTarget(d);
            HandleRotations(d, cont.MouseY, cont.MouseX, targetSpeed);
        }

        void FollowTarget(float d)
        {
            float speed = d * followSpeed;
            Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
            transform.position = targetPosition;
        }

        void HandleRotations(float d, float v, float h, float targetSpeed)
        {
            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYvelocity, turnSmoothing);
            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            tiltAngle -= smoothY * targetSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

            if (lockon && lockonTarget != null)
            {
                Vector3 targetDir = lockonTransform.position - transform.position;
                targetDir.Normalize();

                if (targetDir == Vector3.zero)
                {
                    targetDir = transform.forward;
                }

                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, d * 9);
                lookAngle = transform.eulerAngles.y;
                return;
            }

            lookAngle += smoothX * targetSpeed;
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        }

        public static CameraManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
