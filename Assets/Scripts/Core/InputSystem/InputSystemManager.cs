using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class InputSystemManager : Singleton<InputSystemManager>
    {
        /// <summary>
        /// 已注册的输入
        /// </summary>
        public Dictionary<InputEvent, IInputController> IsRegisterInput = new Dictionary<InputEvent, IInputController>();

        /// <summary>
        /// 当前输入列表
        /// </summary>
        public List<IInputController> CurrentInput = new List<IInputController>();

        /// <summary>
        /// 全局唯一的输入系统
        /// </summary>
        public InputSystem inputSystem;

        /// <summary>
        /// 输入缓冲
        /// </summary>
        private InputBuffer inputBuffer = new InputBuffer();   // 现在使用的是增强版 InputBuffer

        [Tooltip("输入缓冲时长")]
        [SerializeField]private float _bufferDuration = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            inputSystem = new InputSystem();
        }

        private void OnEnable() => inputSystem.Enable();
        private void OnDisable() => inputSystem.Disable();

        private void Update()
        {
            if (CurrentInput.Count == 0)
                LoadInputController(InputEvent.GamePlay);
            inputBuffer.TryExecute();   // 每帧尝试执行缓冲
        }

        /// <summary>
        /// 输入控制器注册
        /// </summary>
        /// <param name="inputEvent">输入事件类型</param>
        /// <param name="inputController"></param>
        public void RegisterInputController(InputEvent inputEvent, IInputController inputController)
        {
            if (!IsRegisterInput.ContainsKey(inputEvent))
                IsRegisterInput.Add(inputEvent, inputController);
        }

        /// <summary>
        /// 输入控制器加载
        /// </summary>
        /// <param name="inputEvent"></param>
        public void LoadInputController(InputEvent inputEvent)
        {
            IsRegisterInput.TryGetValue(inputEvent, out IInputController inputController);
            inputController.LoadAction();
            if (!CurrentInput.Contains(inputController))
                CurrentInput.Add(inputController);
        }

        /// <summary>
        /// 输入控制器卸载
        /// </summary>
        /// <param name="inputEvent"></param>
        public void UnloadInputController(InputEvent inputEvent)
        {
            IsRegisterInput.TryGetValue(inputEvent, out IInputController inputController);
            inputController.UnloadAction();
            if (CurrentInput.Contains(inputController))
                CurrentInput.Remove(inputController);
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