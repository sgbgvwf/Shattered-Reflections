using System.Collections.Generic;
using UnityEngine;
using GameSO;

namespace GameSystem.SceneLoad
{
    public class SceneTeleport : MonoBehaviour
    {
        [Header("事件通道")]
        public SceneLoadEventSO loadEventSO;

        public enum SceneOperationType
        {
            UnLoadAllThenLoad,    // 卸载所有已加载场景，再加载列表中的场景
            CustomSceneOperation  // 自定义卸载列表 + 加载列表
        }
        public SceneOperationType operationType;

        [Header("全量模式：要加载的场景")]
        public List<GameSceneSO> scenesToLoadAfterUnloadAll;

        [Header("自定义模式：卸载列表")]
        public List<GameSceneSO> scenesToUnload;
        [Header("自定义模式：加载列表")]
        public List<GameSceneSO> scenesToLoad;

        [Header("玩家目标位置")]
        public Vector2 playerTargetPosition;

        public void ExecuteSceneOperation()
        {
            Time.timeScale = 1f;

            switch (operationType)
            {
                case SceneOperationType.UnLoadAllThenLoad:
                    loadEventSO.RaiseUnLoadAllThenLoadEvent(scenesToLoadAfterUnloadAll, playerTargetPosition);
                    break;
                case SceneOperationType.CustomSceneOperation:
                    loadEventSO.RaiseCustomSceneOperation(scenesToUnload, scenesToLoad, playerTargetPosition);
                    break;
            }
        }
    }
}
