using System;
using System.IO;
using UnityEngine;

namespace GameSystem.Settings
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        /// <summary>
        /// 当前设置数据
        /// </summary>
        public SettingsData CurrentSettings { get; private set; }

        /// <summary>
        /// 设置变更事件(设置ID, 新值)
        /// </summary>
        public event Action<string, object> OnSettingChanged;

        private string saveFilePath;

        protected override void Awake()
        {
            // 保存路径：持久化数据目录下的 settings.json
            saveFilePath = Path.Combine(Application.persistentDataPath, "settings.json");

            LoadSettings();
        }

        // 加载设置（若文件不存在则使用默认值）
        private void LoadSettings()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
                    Debug.Log("设置已加载：" + saveFilePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("设置文件损坏，使用默认设置。错误：" + e.Message);
                    CurrentSettings = new SettingsData();
                }
            }
            else
            {
                Debug.Log("未找到设置文件，创建默认设置。");
                CurrentSettings = new SettingsData();
                SaveSettings(); // 首次运行时保存一份默认配置
            }
        }

        // 保存设置到 JSON 文件
        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentSettings, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log("设置已保存：" + saveFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("保存设置失败：" + e.Message);
            }
        }

        public T Get<T>(string settingId)
        {
            var field = typeof(SettingsData).GetField(settingId);
            if (field != null)
                return (T)field.GetValue(CurrentSettings);
            
            Debug.LogError($"设置项不存在: {settingId}");
            return default;
        }

        // 通用设置方法（修改值、保存、发送事件）
        public void Set<T>(string settingId, T value)
        {
            var field = typeof(SettingsData).GetField(settingId);
            if (field == null)
            {
                Debug.LogError($"设置项不存在: {settingId}");
                return;
            }

            object oldValue = field.GetValue(CurrentSettings);
            if (Equals(oldValue, value))
                return;

            field.SetValue(CurrentSettings, value);
            SaveSettings();

            // 通知所有订阅者
            OnSettingChanged?.Invoke(settingId, value);
        }

        public void ModifyAndSave(Action<SettingsData> modification)
        {
            modification?.Invoke(CurrentSettings);
            SaveSettings();
        }
    }
}
