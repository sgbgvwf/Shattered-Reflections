namespace Core.Time
{
    using System.Collections.Generic;
    using UnityEngine;

    public class CustomTimeManager : MonoBehaviour
    {
        public static CustomTimeManager Instance { get; private set; }

        [System.Serializable]
        public class TimeGroupEntry
        {
            public TimeGroupType group;
            [Range(0f, 1f)] public float timeScale = 1f;
        }

        [SerializeField] private List<TimeGroupEntry> initialGroups = new List<TimeGroupEntry>
        {
            new TimeGroupEntry { group = TimeGroupType.Player,      timeScale = 1f },
            new TimeGroupEntry { group = TimeGroupType.Boss,        timeScale = 1f },
            new TimeGroupEntry { group = TimeGroupType.Enemy,       timeScale = 1f },
            new TimeGroupEntry { group = TimeGroupType.Projectile,  timeScale = 1f },
            new TimeGroupEntry { group = TimeGroupType.Environment, timeScale = 1f }
        };

        private Dictionary<TimeGroupType, float> groupScales = new Dictionary<TimeGroupType, float>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            foreach (var entry in initialGroups)
                groupScales[entry.group] = entry.timeScale;
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
        /// 获取当前帧该组的 deltaTime
        /// </summary>
        public float GetDeltaTime(TimeGroupType group)
        {
            return Time.deltaTime * GetTimeScale(group);
        }
    }
}