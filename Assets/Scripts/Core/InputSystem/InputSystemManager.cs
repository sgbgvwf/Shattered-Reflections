using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class InputSystemManager : Singleton<InputSystemManager>
    {
        /// <summary>
        /// 全局唯一的输入系统
        /// </summary>
        private InputSystem _inputActions;

        public InputSystem InputActions => _inputActions;

        /// <summary>
        /// 输入缓冲
        /// </summary>
        private InputBuffer inputBuffer = new InputBuffer();   // 现在使用的是增强版 InputBuffer

        [Tooltip("输入缓冲时长")]
        [SerializeField]private float _bufferDuration = 0.1f;

        private Vector2 _moveInput;

        public Vector2 MoveInput => _moveInput;

        private bool _interactInput;
        /// <summary>
        /// 交互输入
        /// </summary>
        public bool InteractPressed => _interactInput;













        protected override void Awake()
        {
            base.Awake();
            _inputActions = new InputSystem();
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _inputActions.GamePlay.Move.performed += _ => _moveInput = _.ReadValue<Vector2>();
            _inputActions.GamePlay.Move.canceled += _ => _moveInput = Vector2.zero;

            _inputActions.GamePlay.Interact.performed += _ => _interactInput = true;
            _inputActions.GamePlay.Interact.canceled += _ => _interactInput = false;
        }
        private void OnDisable()
        {
            _inputActions.GamePlay.Move.performed -= _ => _moveInput = _.ReadValue<Vector2>();
            _inputActions.GamePlay.Move.canceled -= _ => _moveInput = Vector2.zero;

            _inputActions.GamePlay.Interact.performed -= _ => _interactInput = true;
            _inputActions.GamePlay.Interact.canceled -= _ => _interactInput = false;

            _inputActions.Disable();
        }

        private void Update()
        {
            inputBuffer.TryExecute();   // 每帧尝试执行缓冲
        }

        public void EnableGameInput()
        {
            _inputActions.GamePlay.Enable();
        }

        public void DisableGameInput()
        {
            _inputActions.GamePlay.Disable();
        }


        /// <summary>
        /// 尝试立即执行一个操作，如果当前条件不满足则放入缓冲
        /// </summary>
        public void ExecuteOrBuffer(Action action, Func<bool> canExecute)
        {
            if (canExecute())
            {
                action.Invoke();
                inputBuffer.Clear(); // 成功执行则清空缓冲
            }
            else
            {
                inputBuffer.Buffer(action, canExecute, _bufferDuration);
            }
        }

        /// <summary>
        /// 清空缓冲
        /// </summary>
        public void ClearBuffer()
        {
            inputBuffer.Clear();
        }
    }
}