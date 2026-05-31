using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
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
        private InputBuffer inputBuffer = new InputBuffer();

        [Tooltip("输入缓冲时长")]
        [SerializeField] private float _bufferDuration = 0.1f;

        private Vector2 _moveInput;
        public Vector2 MoveInput => _moveInput;

        private bool _interactInput;
        public bool InteractInput => _interactInput;

        private bool _lockInput;
        public bool LockInput => _lockInput;

        private bool _attackInput;
        public bool AttackInput => _attackInput;

        private Vector2 _viewScaleInput;
        public Vector2 ViewScaleInput => _viewScaleInput;

        private bool _jumpInput;
        public bool JumpInput => _jumpInput;

        private bool _dodgeInput;
        public bool DodgeInput => _dodgeInput;

        private bool _skillInput;
        public bool SkillInput => _skillInput;

        private bool _finisherInput;
        public bool FinisherInput => _finisherInput;

        private bool _backInput;
        public bool BackInput => _backInput;

        // 公开的事件
        public event Action OnInteractPressed;
        public event Action OnLockPressed;
        public event Action OnAttackPressed;
        public event Action OnJumpPressed;
        public event Action OnDodgePressed;
        public event Action OnSkillPressed;
        public event Action OnFinisherPressed;
        public event Action OnBackPressed;

        protected override void Awake()
        {
            base.Awake();
            _inputActions = new InputSystem();
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _inputActions.GamePlay.Move.performed += OnMovePerformed;
            _inputActions.GamePlay.Move.canceled += OnMoveCanceled;

            _inputActions.GamePlay.Interact.performed += OnInteractPerformed;
            _inputActions.GamePlay.Interact.canceled += OnInteractCanceled;

            _inputActions.GamePlay.Lock.performed += OnLockPerformed;
            _inputActions.GamePlay.Lock.canceled += OnLockCanceled;

            _inputActions.GamePlay.Attack.performed += OnAttackPerformed;
            _inputActions.GamePlay.Attack.canceled += OnAttackCanceled;

            _inputActions.GamePlay.ViewScale.performed += OnViewScalePerformed;
            _inputActions.GamePlay.ViewScale.canceled += OnViewScaleCanceled;

            _inputActions.GamePlay.Jump.performed += OnJumpPerformed;
            _inputActions.GamePlay.Jump.canceled += OnJumpCanceled;

            _inputActions.GamePlay.Dodge.performed += OnDodgePerformed;
            _inputActions.GamePlay.Dodge.canceled += OnDodgeCanceled;

            _inputActions.GamePlay.Skill.performed += OnSkillPerformed;
            _inputActions.GamePlay.Skill.canceled += OnSkillCanceled;

            _inputActions.GamePlay.Finisher.performed += OnFinisherPerformed;
            _inputActions.GamePlay.Finisher.canceled += OnFinisherCanceled;

            _inputActions.GamePlay.Back.performed += OnBackPerformed;
            _inputActions.GamePlay.Back.canceled += OnBackCanceled;
        }

        private void OnDisable()
        {
            _inputActions.GamePlay.Move.performed -= OnMovePerformed;
            _inputActions.GamePlay.Move.canceled -= OnMoveCanceled;

            _inputActions.GamePlay.Interact.performed -= OnInteractPerformed;
            _inputActions.GamePlay.Interact.canceled -= OnInteractCanceled;

            _inputActions.GamePlay.Lock.performed -= OnLockPerformed;
            _inputActions.GamePlay.Lock.canceled -= OnLockCanceled;

            _inputActions.GamePlay.Attack.performed -= OnAttackPerformed;
            _inputActions.GamePlay.Attack.canceled -= OnAttackCanceled;

            _inputActions.GamePlay.ViewScale.performed -= OnViewScalePerformed;
            _inputActions.GamePlay.ViewScale.canceled -= OnViewScaleCanceled;

            _inputActions.GamePlay.Jump.performed -= OnJumpPerformed;
            _inputActions.GamePlay.Jump.canceled -= OnJumpCanceled;

            _inputActions.GamePlay.Dodge.performed -= OnDodgePerformed;
            _inputActions.GamePlay.Dodge.canceled -= OnDodgeCanceled;

            _inputActions.GamePlay.Skill.performed -= OnSkillPerformed;
            _inputActions.GamePlay.Skill.canceled -= OnSkillCanceled;

            _inputActions.GamePlay.Finisher.performed -= OnFinisherPerformed;
            _inputActions.GamePlay.Finisher.canceled -= OnFinisherCanceled;

            _inputActions.GamePlay.Back.performed -= OnBackPerformed;
            _inputActions.GamePlay.Back.canceled -= OnBackCanceled;

            _inputActions.Disable();

            OnInteractPressed = null;
            OnLockPressed = null;
            OnAttackPressed = null;
            OnJumpPressed = null;
            OnDodgePressed = null;
            OnSkillPressed = null;
            OnFinisherPressed = null;
            OnBackPressed = null;
        }

        private void Update()
        {
            inputBuffer.TryExecute();
        }

        public void EnableGameInput()
        {
            _inputActions.GamePlay.Enable();
        }

        public void DisableGameInput()
        {
            _inputActions.GamePlay.Disable();
        }

        public void ExecuteOrBuffer(Action action, Func<bool> canExecute)
        {
            if (canExecute())
            {
                action.Invoke();
                inputBuffer.Clear();
            }
            else
            {
                inputBuffer.Buffer(action, canExecute, _bufferDuration);
            }
        }

        public void ClearBuffer()
        {
            inputBuffer.Clear();
        }

        #region Input Callbacks

        private void OnMovePerformed(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => _moveInput = Vector2.zero;

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            _interactInput = true;
            OnInteractPressed?.Invoke();
        }
        private void OnInteractCanceled(InputAction.CallbackContext ctx) => _interactInput = false;

        private void OnLockPerformed(InputAction.CallbackContext ctx)
        {
            _lockInput = true;
            OnLockPressed?.Invoke();
        }
        private void OnLockCanceled(InputAction.CallbackContext ctx) => _lockInput = false;

        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            _attackInput = true;
            OnAttackPressed?.Invoke();
        }
        private void OnAttackCanceled(InputAction.CallbackContext ctx) => _attackInput = false;

        private void OnViewScalePerformed(InputAction.CallbackContext ctx) => _viewScaleInput = ctx.ReadValue<Vector2>();
        private void OnViewScaleCanceled(InputAction.CallbackContext ctx) => _viewScaleInput = Vector2.zero;

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            _jumpInput = true;
            OnJumpPressed?.Invoke();
        }
        private void OnJumpCanceled(InputAction.CallbackContext ctx) => _jumpInput = false;

        private void OnDodgePerformed(InputAction.CallbackContext ctx)
        {
            _dodgeInput = true;
            OnDodgePressed?.Invoke();
        }
        private void OnDodgeCanceled(InputAction.CallbackContext ctx) => _dodgeInput = false;

        private void OnSkillPerformed(InputAction.CallbackContext ctx)
        {
            _skillInput = true;
            OnSkillPressed?.Invoke();
        }
        private void OnSkillCanceled(InputAction.CallbackContext ctx) => _skillInput = false;

        private void OnFinisherPerformed(InputAction.CallbackContext ctx)
        {
            _finisherInput = true;
            OnFinisherPressed?.Invoke();
        }
        private void OnFinisherCanceled(InputAction.CallbackContext ctx) => _finisherInput = false;

        private void OnBackPerformed(InputAction.CallbackContext ctx)
        {
            _backInput = true;
            OnBackPressed?.Invoke();
        }
        private void OnBackCanceled(InputAction.CallbackContext ctx) => _backInput = false;

        #endregion
    }
}
