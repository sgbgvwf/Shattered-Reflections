using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Core;

namespace Combat.Move
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveController : MonoBehaviour, IInputController
    {
        public MovePhysicsDetection physicsDetection;

        public bool moveIsOpen;

        [Header("移动设置")]
        public float moveForce = 150;
        public float maxMoveVelocity_forward = 10;
        public float maxMoveVelocity_backward = 6;
        public float maxMoveVelocity_horizontal = 8;
        public float maxRushVelocity = 15;

        [Header("跳跃设置")]
        public float jumpForce = 15;
        [Range(0, 1)]
        public float moveInJump = 0.1f;

        [HideInInspector] public Vector2 _inputMove;
        [HideInInspector] public bool _isRush;

        private InputSystem _inputSystem;
        private Rigidbody _rb;

        private void Awake()
        {
            _inputSystem = InputSystemManager.Instance.inputSystem;
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            RegisterInput(InputEvent.GamePlay, this);
            
        }

        private void FixedUpdate()
        {
            Move(_inputMove);
            // VelocityLimit(_inputMove);
        }

        private void Update()
        {
            _inputMove = _inputSystem.GamePlay.Move.ReadValue<Vector2>();
        }
        
        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public void RegisterInput(InputEvent inputEvent, IInputController inputController)
        {
            InputSystemManager.Instance.RegisterInputController(inputEvent, inputController);
        }

        public void LoadAction()
        {
            _inputSystem.GamePlay.Jump.started += Jump;
            _inputSystem.GamePlay.Rush.performed += isRush;
            _inputSystem.GamePlay.Rush.canceled += noRush;

        // Debug.Log("+Move");
        }

        public void UnloadAction()
        {
            _inputSystem.GamePlay.Jump.started -= Jump;
            _inputSystem.GamePlay.Rush.performed -= isRush;
            _inputSystem.GamePlay.Rush.canceled -= noRush;

        // Debug.Log("-Move");
        }


        /// <summary>
        /// 获取面朝方向
        /// </summary>
        /// <returns></returns>
        public (Vector3 direction_forward, Vector3 direction_right) GetForwardWay()
        {
            Vector3 forwardDir = transform.forward;
            forwardDir.y = 0;
            forwardDir.Normalize();
            Vector3 rightDir = transform.right;
            rightDir.y = 0;
            rightDir.Normalize();

            return (forwardDir, rightDir);
        }

        /// <summary>
        /// 力限制
        /// </summary>
        /// <returns></returns>
        private (float k_f, float k_b, float k_h) ForceLimit()
        {
            var forwardWay = GetForwardWay();

            Vector3 direction_forward = forwardWay.direction_forward;
            Vector3 direction_right = forwardWay.direction_right;

            float vfz = Mathf.Abs(Vector3.Dot(_rb.velocity, direction_forward));
            float vfx = Mathf.Abs(Vector3.Dot(_rb.velocity, direction_right));

            float k_forward;
            if (_isRush)
                k_forward = Mathf.Clamp01(1 - vfz / maxRushVelocity);
            else
                k_forward = Mathf.Clamp01(1 - vfz / maxMoveVelocity_forward);
            float k_backward = Mathf.Clamp01(1 - vfz / maxMoveVelocity_backward);
            float k_horizontal = Mathf.Clamp01(1 - vfx / maxMoveVelocity_horizontal);

            return (k_forward, k_backward, k_horizontal);
        }

        /// <summary>
        /// 移动
        /// </summary>
        private void Move(Vector2 inputVector2)
        {
            var forwardWay = GetForwardWay();

            float localForwardForce = inputVector2.y * moveForce;
            float localRightForce = inputVector2.x * moveForce;

            var forceLimit = ForceLimit();

            if (inputVector2.y > 0) localForwardForce *= forceLimit.k_f;
            if (inputVector2.y < 0) localForwardForce *= forceLimit.k_b;
            if (inputVector2.x != 0) localRightForce *= forceLimit.k_h;

            Vector3 directionForce =
                forwardWay.direction_forward * localForwardForce +
                forwardWay.direction_right * localRightForce;
            directionForce.y = 0;

            if (!physicsDetection.isGround)
            {
                _rb.AddForce(directionForce * moveInJump, ForceMode.Force);
            }
            else
            {
                _rb.AddForce(directionForce, ForceMode.Force);
            }
        }

        /// <summary>
        /// 跳跃
        /// </summary>
        private void Jump(InputAction.CallbackContext obj)
        {
            if (physicsDetection.isGround)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// 冲刺
        /// </summary>
        /// <param name="obj"></param>
        private void isRush(InputAction.CallbackContext obj)
        {
            _isRush = true;
        }

        /// <summary>
        /// 停止冲刺
        /// </summary>
        /// <param name="obj"></param>
        private void noRush(InputAction.CallbackContext obj)
        {
            _isRush = false;
        }





        public void OpenController()
        {

            LoadAction();
            moveIsOpen = true;

        }


        public void CloseController()
        {
            UnloadAction();

            moveIsOpen = false;

        }

    }
}
