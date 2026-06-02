using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameSO
{
    [CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
    public class SceneLoadEventSO : ScriptableObject
    {
        // 全量卸载所有 + 加载新场景
        public UnityAction<List<GameSceneSO>, Vector2> UnLoadAllThenLoadEvent;
        public void RaiseUnLoadAllThenLoadEvent(List<GameSceneSO> scenesToLoad, Vector2 positionToGo)
        {
            UnLoadAllThenLoadEvent?.Invoke(scenesToLoad, positionToGo);
        }

        // 自定义卸载/加载
        public UnityAction<List<GameSceneSO>, List<GameSceneSO>, Vector2> CustomSceneOperationEvent;
        public void RaiseCustomSceneOperation(List<GameSceneSO> scenesToUnLoad, List<GameSceneSO> scenesToLoad, Vector2 positionToGo)
        {
            CustomSceneOperationEvent?.Invoke(scenesToUnLoad, scenesToLoad, positionToGo);
        }
    }
}
