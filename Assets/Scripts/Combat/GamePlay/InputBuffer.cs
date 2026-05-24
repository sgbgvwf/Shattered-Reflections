using System;

namespace Core
{
    /// <summary>
    /// 单槽输入缓冲
    /// </summary>
    public class InputBuffer
    {
        /// <summary>
        /// 缓冲的操作
        /// </summary>
        private Action pendingAction;

        /// <summary>
        /// 判断是否可执行
        /// </summary>
        private Func<bool> condition;

        /// <summary>
        /// 是否有待执行的缓冲操作。
        /// </summary>
        public bool HasPending => pendingAction != null;

        /// <summary>
        /// 将操作及其可执行条件放入缓冲（直接覆盖）
        /// </summary>
        /// <param name="action">实际要执行的操作</param>
        /// <param name="canExecute">返回 true 时表示操作现在可以执行，每帧会用此条件检查</param>
        public void Buffer(Action action, Func<bool> canExecute)
        {
            pendingAction = action;
            condition = canExecute;
        }

        /// <summary>
        /// 若条件满足则立即执行缓冲的操作并清空缓冲
        /// </summary>
        public void TryExecute()
        {
            if (pendingAction != null && condition != null && condition())
            {
                pendingAction.Invoke();
                Clear();
            }
        }

        /// <summary>
        /// 清空缓冲
        /// </summary>
        public void Clear()
        {
            pendingAction = null;
            condition = null;
        }
    }
}