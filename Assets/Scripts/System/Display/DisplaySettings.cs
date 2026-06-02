using System.Collections;
using System.Collections.Generic;
using GameSystem.Settings;
using UnityEngine;

namespace GameSystem.Display
{
    public enum FrameRate
    {
        FPS_30,
        FPS_60,
        FPS_90,
        FPS_120,
        FPS_144,
        Unlimited,
    }
    public class DisplaySettings : MonoBehaviour
    {
        SettingsData settingsData;
        public bool test;
        [SerializeField] private FrameRate frameRate;

        private Dictionary<FrameRate, int> frameDict = new Dictionary<FrameRate, int>()
        {
            {FrameRate.FPS_30, 30},
            {FrameRate.FPS_60, 60},
            {FrameRate.FPS_90, 90},
            {FrameRate.FPS_120, 120},
            {FrameRate.FPS_144, 144},
            {FrameRate.Unlimited, -1}
        };

        [SerializeField] private bool verticalSynchronization;

        private void Start()
        {
            settingsData = SettingsManager.Instance.CurrentSettings;
            ApplyAllSettings();

            SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
        }

        void Update()
        {
            if (test)
            {
                Application.targetFrameRate = frameDict[frameRate];
                verticalSynchronization = true;
                QualitySettings.vSyncCount = 4;
                test = false;
            }
        }

        private void ApplyAllSettings()
        {
            verticalSynchronization = settingsData.verticalSynchronization;
            if(verticalSynchronization) QualitySettings.vSyncCount = 1;
            else QualitySettings.vSyncCount = 0;

            frameRate = settingsData.frameRate;
            Application.targetFrameRate = frameDict[frameRate];
        }
            
        private void OnSettingChanged(string settingId, object newValue)
        {
            switch (settingId)
            {
                case "frameRate":
                case "verticalSynchronization":
                    ApplyAllSettings();
                    break;
            }
        }

    }

}
