using System;
using GameSystem.Display;

namespace GameSystem.Settings
{
    [Serializable]
    public class SettingsData
    {
        // 音频设置
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;

        // 相机设置
        public float cameraSensitivityScale_X = 1;
        public float cameraSensitivityScale_Y = 1;
        public bool cameraControlInvert_X = false;
        public bool cameraControlInvert_Y = false;

        // 画面设置
        public bool isFullscreen = true;
        public bool verticalSynchronization = false;
        public FrameRate frameRate = FrameRate.Unlimited;


        // 操控设置
        public float mouseSensitivity = 0.5f;
        public bool invertY = false;

        // 游戏玩法设置
        public bool showMinimap = true;
        public string language = "zh-CN";
    }


}
