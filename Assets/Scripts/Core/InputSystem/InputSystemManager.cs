using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class InputSystemManager : Singleton<InputSystemManager>
    {
        /// <summary>
        /// 输入控制器的注册字典
        /// </summary>
        public Dictionary<InputEvent, IInputController> IsRegisterInput = new Dictionary<InputEvent, IInputController>();
        
        /// <summary>
        /// 输入控制器列表
        /// </summary>
        public List<IInputController> CurrentInput = new List<IInputController>();

        /// <summary>
        /// 全局唯一的输入系统实例
        /// </summary>
        public InputSystem inputSystem;

        /// <summary>
        /// 输入缓冲
        /// </summary>
        private InputBuffer inputBuffer = new InputBuffer();

        protected override void Awake()
        {
            base.Awake();

            inputSystem = new InputSystem();


        }



        private void OnEnable()
        {
            inputSystem.Enable();
        }

        private void OnDisable()
        {
            inputSystem.Disable();
        }

        private void Update()
        {
            if (CurrentInput.Count == 0)
            {
                LoadInputController(InputEvent.GamePlay);

            }
            inputBuffer.TryExecute();
        }


        public void RegisterInputController(InputEvent inputScene,IInputController inputController)
        {
            if(!IsRegisterInput.ContainsKey(inputScene))
                IsRegisterInput.Add(inputScene, inputController);
        }

        /// <summary>
        /// 加载输入控制
        /// </summary>
        /// <param name="inputScene"></param>
        public void LoadInputController(InputEvent inputScene)
        {
            IsRegisterInput.TryGetValue(inputScene, out IInputController inputController);
            inputController.LoadAction();
            if(!CurrentInput.Contains(inputController))
                CurrentInput.Add(inputController);
        }
        
        /// <summary>
        /// 卸载输入控制
        /// </summary>
        /// <param name="inputScene"></param>
        public void UnloadInputController(InputEvent inputScene)
        {
            IsRegisterInput.TryGetValue(inputScene, out IInputController inputController);
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
                inputBuffer.Buffer(action, canExecute);
            }
        }

        /// <summary>
        /// 主动清空缓冲
        /// </summary>
        public void ClearBuffer()
        {
            inputBuffer.Clear();
        }









    }

}
