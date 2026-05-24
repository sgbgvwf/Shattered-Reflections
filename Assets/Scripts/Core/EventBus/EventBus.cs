namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.iOS;
    using UnityEngine.XR;

    public class EventBus : Singleton<EventBus>
    {
        private readonly Dictionary<GameEvent, List<Delegate>> _handlers = new();

        /// <summary>
        /// 私有构造防止外部new
        /// </summary>
        private EventBus() {}

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="evt">事件标识</param>
        /// <param name="handler">处理函数</param>
        public void Subscribe<T>(GameEvent evt, Action<T> handler)
        {
            if(_handlers.TryGetValue(evt, out var list))
            {
                list.Add(handler);
            }
            else
            {
                _handlers[evt] = new List<Delegate> { handler };
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="evt">事件标识</param>
        /// <param name="handler">处理函数</param>
        public void Unsubscribe<T>(GameEvent evt, Action<T> handler)
        {
            if(_handlers.TryGetValue(evt, out var list))
            {
                list.Remove(handler);
                if(list.Count == 0) // 顺手的事
                {
                    _handlers.Remove(evt);
                }
            }
        }

        /// <summary>
        /// 发布事件通知所有订阅者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        /// <param name="eventArgs"></param>
        public void Publish<T>(GameEvent evt, T eventArgs)
        {
            // Debug日志
            Debug.Log($"[EventBus] Publish Event: {evt} | Type: {typeof(T).Name} | Data: {JsonUtility.ToJson(eventArgs, true)}");

            if(!_handlers.TryGetValue(evt, out var list)) return;

            // 复制一份名单方式遍历时被修改
            Delegate[] handlers = list.ToArray();

            foreach(var del in handlers)
            {
                var action = del as Action<T>;
                action?.Invoke(eventArgs);
            }
        }

        /// <summary>
        /// 清理所有订阅名单
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// 移除某个事件的所有订阅
        /// </summary>
        public void ClearEvent(GameEvent evt)
        {
            _handlers.Remove(evt);
        }

        protected override void OnApplicationQuit()
        {
            Clear();
            base.OnApplicationQuit();
        }

        protected override  void OnDestroy()
        {   
            Clear();
            base.OnDestroy();
        }

    }
}