namespace Core.Timer
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;
    public class Timer
    {
        #region Public Properties/Fields

        public float Duration { get; private set; }
        public bool IsLooped { get; set; }
        public bool IsCompleted { get; private set; }
        public bool UsesRealTime { get; private set; }
        public bool IsPaused => _timeElapsedBeforePause.HasValue;
        public bool IsCancelled => _timeElapsedBeforeCancel.HasValue;
        public bool IsDone => IsCompleted || IsCancelled || IsOwnerDestroyed;

        #endregion

        #region Public Static Methods

        /// <summary>
        /// 注册计时器。
        /// </summary>
        /// <param name="duration">时长（秒）</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="onUpdate">每帧更新回调</param>
        /// <param name="isLooped">是否循环</param>
        /// <param name="useRealTime">是否使用真实时间（不受Time.timeScale影响）</param>
        /// <param name="autoDestroyOwner">绑定的MonoBehaviour，对象销毁时计时器自动取消</param>
        /// <param name="customTimeProvider">自定义时间源函数，返回当前时间值。非null时忽略UsesRealTime</param>
        public static Timer Register(float duration, Action onComplete, Action<float> onUpdate = null,
            bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null,
            Func<float> customTimeProvider = null)
        {
            if (_manager == null)
            {
                TimerManager managerInScene = Object.FindObjectOfType<TimerManager>();
                if (managerInScene != null)
                {
                    _manager = managerInScene;
                }
                else
                {
                    GameObject managerObject = new GameObject { name = "TimerManager" };
                    _manager = managerObject.AddComponent<TimerManager>();
                }
            }

            Timer timer = new Timer(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner, customTimeProvider);
            _manager.RegisterTimer(timer);
            return timer;
        }

        public static void Cancel(Timer timer)
        {
            if (timer != null) timer.Cancel();
        }

        public static void Pause(Timer timer)
        {
            if (timer != null) timer.Pause();
        }

        public static void Resume(Timer timer)
        {
            if (timer != null) timer.Resume();
        }

        public static void CancelAllRegisteredTimers()
        {
            if (_manager != null) _manager.CancelAllTimers();
        }

        public static void PauseAllRegisteredTimers()
        {
            if (_manager != null) _manager.PauseAllTimers();
        }

        public static void ResumeAllRegisteredTimers()
        {
            if (_manager != null) _manager.ResumeAllTimers();
        }

        #endregion

        #region Public Methods

        public void Cancel()
        {
            if (IsDone) return;
            _timeElapsedBeforeCancel = GetTimeElapsed();
            _timeElapsedBeforePause = null;
        }

        public void Pause()
        {
            if (IsPaused || IsDone) return;
            _timeElapsedBeforePause = GetTimeElapsed();
        }

        public void Resume()
        {
            if (!IsPaused || IsDone) return;
            _timeElapsedBeforePause = null;
        }

        public float GetTimeElapsed()
        {
            if (IsCompleted || GetWorldTime() >= GetFireTime())
                return Duration;

            return _timeElapsedBeforeCancel ??
                   _timeElapsedBeforePause ??
                   GetWorldTime() - _startTime;
        }

        public float GetTimeRemaining() => Duration - GetTimeElapsed();
        public float GetRatioComplete() => GetTimeElapsed() / Duration;
        public float GetRatioRemaining() => GetTimeRemaining() / Duration;

        #endregion

        #region Private Static Fields

        private static TimerManager _manager;

        #endregion

        #region Private Fields

        private readonly Action _onComplete;
        private readonly Action<float> _onUpdate;
        private float _startTime;
        private float _lastUpdateTime;

        private float? _timeElapsedBeforeCancel;
        private float? _timeElapsedBeforePause;

        private readonly MonoBehaviour _autoDestroyOwner;
        private readonly bool _hasAutoDestroyOwner;

        // 自定义时间源：返回当前时间值（例如虚拟时间）
        private readonly Func<float> _customTimeProvider;

        private bool HasCustomTimeProvider => _customTimeProvider != null;

        #endregion

        #region Private Constructor

        private Timer(float duration, Action onComplete, Action<float> onUpdate,
            bool isLooped, bool usesRealTime, MonoBehaviour autoDestroyOwner,
            Func<float> customTimeProvider)
        {
            Duration = duration;
            _onComplete = onComplete;
            _onUpdate = onUpdate;
            IsLooped = isLooped;
            UsesRealTime = usesRealTime;
            _autoDestroyOwner = autoDestroyOwner;
            _hasAutoDestroyOwner = autoDestroyOwner != null;
            _customTimeProvider = customTimeProvider;

            _startTime = GetWorldTime();
            _lastUpdateTime = _startTime;
        }

        #endregion

        #region Private Methods

        private float GetWorldTime()
        {
            // 优先使用自定义时间源
            if (HasCustomTimeProvider)
                return _customTimeProvider();

            return UsesRealTime ? Time.realtimeSinceStartup : Time.time;
        }

        private float GetFireTime() => _startTime + Duration;

        private float GetTimeDelta() => GetWorldTime() - _lastUpdateTime;

        private void Update()
        {
            if (IsDone) return;

            if (IsPaused)
            {
                _startTime += GetTimeDelta();
                _lastUpdateTime = GetWorldTime();
                return;
            }

            _lastUpdateTime = GetWorldTime();

            _onUpdate?.Invoke(GetTimeElapsed());

            if (GetWorldTime() >= GetFireTime())
            {
                _onComplete?.Invoke();

                if (IsLooped)
                    _startTime = GetWorldTime();
                else
                    IsCompleted = true;
            }
        }

        private bool IsOwnerDestroyed => _hasAutoDestroyOwner && _autoDestroyOwner == null;

        #endregion

        #region Manager Class

        private class TimerManager : MonoBehaviour
        {
            private List<Timer> _timers = new List<Timer>();
            private List<Timer> _timersToAdd = new List<Timer>();

            public void RegisterTimer(Timer timer) => _timersToAdd.Add(timer);

            public void CancelAllTimers()
            {
                foreach (var timer in _timers) timer.Cancel();
                _timers.Clear();
                _timersToAdd.Clear();
            }

            public void PauseAllTimers()
            {
                foreach (var timer in _timers) timer.Pause();
            }

            public void ResumeAllTimers()
            {
                foreach (var timer in _timers) timer.Resume();
            }

            private void Update()
            {
                if (_timersToAdd.Count > 0)
                {
                    _timers.AddRange(_timersToAdd);
                    _timersToAdd.Clear();
                }

                foreach (var timer in _timers) timer.Update();
                _timers.RemoveAll(t => t.IsDone);
            }
        }

        #endregion
    }
}