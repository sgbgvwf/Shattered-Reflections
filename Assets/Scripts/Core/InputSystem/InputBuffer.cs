using System;
using Core.OwnTimer;   // 引入 Timer
using Core.Time;

namespace Core
{
    /// <summary>
    /// 单槽输入缓冲，使用 Timer 实现过期自动清空。
    /// 只使用真实时间
    /// </summary>
    public class InputBuffer
    {
        // 缓冲的操作与条件
        private Action pendingAction;
        private Func<bool> condition;

        // 过期定时器
        private Timer expireTimer;

        /// <summary>
        /// 是否有待执行的缓冲操作
        /// </summary>
        public bool HasPending => pendingAction != null;

        /// <summary>
        /// 将操作及其可执行条件放入缓冲。
        /// 同时启动一个定时器，到期后自动清空缓冲。
        /// </summary>
        /// <param name="action">实际要执行的操作</param>
        /// <param name="canExecute">返回 true 时表示操作现在可以执行，每帧会用此条件检查</param>
        /// <param name="bufferDuration">缓冲有效时长（秒）</param>
        public void Buffer(Action action, Func<bool> canExecute, float bufferDuration)
        {
            // 取消之前的过期定时器（如果有且未完成）
            if (expireTimer != null && !expireTimer.IsDone)
                Timer.Cancel(expireTimer);

            pendingAction = action;
            condition = canExecute;

            // 创建过期定时器
            expireTimer = CreateExpireTimer(bufferDuration);
        }

        /// <summary>
        /// 若条件满足且缓冲尚未过期，则立即执行缓冲的操作并清空缓冲。
        /// 该方法应在每帧的 Update 中调用（由 InputSystemManager 负责）。
        /// </summary>
        public void TryExecute()
        {
            if (pendingAction == null || condition == null)
                return;

            // 注意：过期定时器到期时会自动调用 Clear()，所以这里无需再检查时间，
            // 但为了避免极少数情况（例如过期回调还没执行），如果定时器已过期就清空。
            if (expireTimer != null && expireTimer.IsDone)
            {
                Clear();
                return;
            }

            if (condition())
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
            if (expireTimer != null && !expireTimer.IsDone)
                Timer.Cancel(expireTimer);
            expireTimer = null;
            pendingAction = null;
            condition = null;
        }

        // 创建过期定时器
        private Timer CreateExpireTimer(float duration)
        {
            return Timer.Register(duration, Clear, useRealTime: true);
        }
    }
}