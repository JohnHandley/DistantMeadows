﻿using UnityEngine;

namespace SA
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;


        [Header("Inputs")]
        public float moveAmount;
        public Vector3 moveDir;
        public Controls cont;

        [Header("Stats")]
        public float moveSpeed = 3.5f;
        public float runSpeed = 5f;
        public float rotateSpeed = 5.0f;
        public float toGround = 0.5f;
        public float rollSpeed = 1.0f;

        [Header("States")]
        public bool onGround;
        public bool run;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool isTwoHanded;

        [Header("Other")]
        public EnemyTarget lockOnTarget;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public AnimatorHook a_hook;


        [HideInInspector]
        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;

        float _actionDelay;

        public void Init()
        {
            SetupAnimator();
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            a_hook = activeModel.AddComponent<AnimatorHook>();
            a_hook.Init(this);

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool("onGround", true);
        }

        void SetupAnimator()
        {
            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.Log("No model found");
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }

            if (anim == null)
                anim = activeModel.GetComponent<Animator>();

            anim.applyRootMotion = false;
        }

        public void FixedTick(float d)
        {
            delta = d;


            DetectAction();

            if (inAction)
            {
                anim.applyRootMotion = true;

                _actionDelay += delta;
                if (_actionDelay > 0.3f)
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }

            canMove = anim.GetBool("canMove");

            if (!canMove)
                return;

            a_hook.rm_multi = 1.0f;
            HandleRolls();

            anim.applyRootMotion = false;

            rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (run)
                targetSpeed = runSpeed;

            if (onGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);

            if (run)
                lockOn = false;

            Vector3 targetDir = (!lockOn) ? moveDir : lockOnTarget.transform.position - transform.position;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;

            anim.SetBool("lockon", lockOn);

            if(lockOn)
            {
                HandleLockOnAnimations(moveDir);
            }
            else
            {
                HandleMovementAnimations();
            }
        }

        public void DetectAction()
        {
            if (canMove == false)
                return;

            if (!cont.LeftHandAction && !cont.LeftHandHeavyAction && !cont.RightHandAction && !cont.RightHandHeavyAction)
                return;

            string targetAnim = null;

            if (cont.RightHandAction)
                targetAnim = "oh_attack_1";
            if (cont.RightHandHeavyAction)
                targetAnim = "oh_attack_2";
            if (cont.LeftHandAction)
                targetAnim = "oh_attack_3";
            if (cont.LeftHandHeavyAction)
                targetAnim = "th_attack_1";

            if (string.IsNullOrEmpty(targetAnim))
                return;

            canMove = false;
            inAction = true;
            anim.CrossFade(targetAnim, 0.2f);
            //rigid.velocity = Vector3.zero;
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool("onGround", onGround);
        }

        void HandleRolls()
        {
            if(!cont.Roll)
            {
                return;
            }

            float v = (moveAmount > 0.3f) ? 1 : 0;  // cont.Vertical;
            float h = 0; // cont.Horizontal;

            //if (!lockOn)
            //{
            //    v = (moveAmount > 0.3f) ? 1 : 0;
            //    h = 0;
            //}
            //else
            //{
            //    if(Mathf.Abs(v) < 0.3f)
            //    {
            //        v = 0;
            //    }

            //    if(Mathf.Abs(h) < 0.3f)
            //    {
            //        h = 0;
            //    }
            //}

            if(v != 0)
            {
                if(moveDir == Vector3.zero)
                {
                    moveDir = transform.forward;
                }
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = targetRot;
            }

            a_hook.rm_multi = rollSpeed;

            anim.SetFloat("vertical", v);
            anim.SetFloat("horizontal", h);

            canMove = false;
            inAction = true;
            anim.CrossFade("Rolls", 0.2f);
        }

        void HandleMovementAnimations()
        {
            anim.SetBool("run", run);
            anim.SetFloat("vertical", moveAmount, 0.4f, delta);
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relatvieDir = transform.InverseTransformDirection(moveDir);
            float h = relatvieDir.x;
            float v = relatvieDir.z;

            anim.SetFloat("vertical", v, 0.2f, delta);
            anim.SetFloat("horizontal", h, 0.2f, delta);
        }

        public bool OnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;
            RaycastHit hit;
            Debug.DrawRay(origin, dir * dis);
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                r = true;
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }

            return r;
        }

        public void HandleTwoHanded()
        {
            anim.SetBool("two_handed", isTwoHanded);
        }
    }
}
