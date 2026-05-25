namespace Core.Time
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 自定义时间管理器
    /// </summary>
    public class CustomTimeManager : Singleton<CustomTimeManager>
    {
        /// <summary>
        /// 时间组数据类
        /// </summary>
        [System.Serializable]
        public class TimeGroupData
        {
            public TimeGroupType group;
            [Range(0f, 1f)] public float timeScale = 1f;
        }

        /// <summary>
        /// 时间组列表
        /// </summary>
        [SerializeField] private List<TimeGroupData> initialGroups = new List<TimeGroupData>();

        /// <summary>
        /// 时间组种类对应的时间缩放量
        /// </summary>
        private Dictionary<TimeGroupType, float> groupScales = new Dictionary<TimeGroupType, float>();
        
        /// <summary>
        /// 每个组的虚拟时间累计
        /// </summary>
        private Dictionary<TimeGroupType, float> groupVirtualTimes = new Dictionary<TimeGroupType, float>();

        protected override void Awake()
        {
            base.Awake();

            foreach (var entry in initialGroups)
            {
                groupScales[entry.group] = entry.timeScale;
                groupVirtualTimes[entry.group] = 0f;   // 初始化虚拟时间
            }
        }

        private void Update()
        {
            float realDelta = Time.deltaTime; // 不受任何缩放影响的真实帧间隔
            foreach (var kvp in groupScales)
            {
                float increment = realDelta * kvp.Value;
                groupVirtualTimes[kvp.Key] = groupVirtualTimes.GetValueOrDefault(kvp.Key) + increment;
            }
        }

        /// <summary>
        /// 设置指定组的时间缩放系数
        /// </summary>
        public void SetTimeScale(TimeGroupType group, float scale)
        {
            groupScales[group] = Mathf.Clamp(scale, 0f, 1f);
        }

        /// <summary>
        /// 获取指定组的时间缩放系数
        /// </summary>
        public float GetTimeScale(TimeGroupType group)
        {
            return groupScales.TryGetValue(group, out float scale) ? scale : 1f;
        }

        /// <summary>
        /// 获取当前帧该组的 deltaTime（用于每帧移动/动画，同原有功能）
        /// </summary>
        public float GetDeltaTime(TimeGroupType group)
        {
            return Time.deltaTime * GetTimeScale(group);
        }

        /// <summary>
        /// 获取某个时间组的全局虚拟时间
        /// 适合用作 Timer 的自定义时间源。
        /// </summary>
        public float GetGroupVirtualTime(TimeGroupType group)
        {
            return groupVirtualTimes.TryGetValue(group, out float time) ? time : 0f;
        }

        /// <summary>
        /// 重置某时间组内虚拟时间
        /// </summary>
        /// <param name="group">时间组</param>
        public void ResetGroupVirtualTime(TimeGroupType group)
        {
            if (groupVirtualTimes.ContainsKey(group))
                groupVirtualTimes[group] = 0f;
        }

        /// <summary>
        /// 重置所有时间组的虚拟时间
        /// </summary>
        public void ResetAllGroupsVirtualTime()
        {
            var keys = new List<TimeGroupType>(groupVirtualTimes.Keys);
            foreach (var group in keys)
                groupVirtualTimes[group] = 0f;
        }
    }
}