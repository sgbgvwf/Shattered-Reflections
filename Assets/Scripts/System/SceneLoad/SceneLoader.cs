using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameSO;

namespace GameSystem.SceneLoad
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("事件监听")]
        public SceneLoadEventSO loadEventSO;

        [Header("广播")]
        public GameEvent afterSceneLoadedEvent = GameEvent.SceneLoadedEvent;

        [Header("玩家")]
        public Transform playerTransform;

        [Header("初始加载场景（可选）")]
        public GameSceneSO firstLoadScene;

        private List<GameSceneSO> allLoadedScenes = new List<GameSceneSO>();
        private bool isLoading = false;

        private void Start()
        {
            if (firstLoadScene != null)
            {
                // 初始加载：只加载，不卸载任何东西（使用全量模式但卸载列表为空？直接调用加载协程）
                StartCoroutine(LoadScenesCoroutine(new List<GameSceneSO> { firstLoadScene }, Vector2.zero, true));
            }
        }

        private void OnEnable()
        {
            loadEventSO.UnLoadAllThenLoadEvent += OnUnLoadAllThenLoad;
            loadEventSO.CustomSceneOperationEvent += OnCustomSceneOperation;
        }

        private void OnDisable()
        {
            loadEventSO.UnLoadAllThenLoadEvent -= OnUnLoadAllThenLoad;
            loadEventSO.CustomSceneOperationEvent -= OnCustomSceneOperation;
        }

        // 全量卸载所有 + 加载
        private void OnUnLoadAllThenLoad(List<GameSceneSO> scenesToLoad, Vector2 cameraPos)
        {
            if (isLoading) return;
            StartCoroutine(UnloadAllThenLoadCoroutine(scenesToLoad, cameraPos));
        }

        private IEnumerator UnloadAllThenLoadCoroutine(List<GameSceneSO> scenesToLoad, Vector2 cameraPos)
        {
            isLoading = true;

            // 卸载所有已加载的场景
            foreach (var scene in allLoadedScenes)
            {
                if (scene != null)
                    yield return scene.sceneReference.UnLoadScene();
            }
            allLoadedScenes.Clear();

            // 加载新场景
            yield return LoadScenesCoroutine(scenesToLoad, cameraPos, true);

            isLoading = false;
            EventBus.Instance.Publish(afterSceneLoadedEvent);
        }

        // 自定义增量操作
        private void OnCustomSceneOperation(List<GameSceneSO> scenesToUnload, List<GameSceneSO> scenesToLoad, Vector2 cameraPos)
        {
            if (isLoading) return;
            StartCoroutine(CustomOperationCoroutine(scenesToUnload, scenesToLoad, cameraPos));
        }

        private IEnumerator CustomOperationCoroutine(List<GameSceneSO> scenesToUnload, List<GameSceneSO> scenesToLoad, Vector2 cameraPos)
        {
            isLoading = true;

            // 卸载指定场景
            if (scenesToUnload != null)
            {
                foreach (var scene in scenesToUnload)
                {
                    if (scene == null || !allLoadedScenes.Contains(scene)) continue;
                    yield return scene.sceneReference.UnLoadScene();
                    allLoadedScenes.Remove(scene);
                }
            }

            // 加载指定场景
            yield return LoadScenesCoroutine(scenesToLoad, cameraPos, true);

            isLoading = false;
            EventBus.Instance.Publish(afterSceneLoadedEvent);
        }

        // 通用加载协程（可选是否设置相机位置）
        private IEnumerator LoadScenesCoroutine(List<GameSceneSO> scenesToLoad, Vector2 cameraPos, bool setCamera)
        {
            if (scenesToLoad == null) yield break;

            foreach (var scene in scenesToLoad)
            {
                if (scene == null) continue;
                if (allLoadedScenes.Contains(scene)) continue;

                var op = scene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
                yield return op;
                allLoadedScenes.Add(scene);
            }

            if (setCamera && playerTransform != null)
            {
                Vector3 pos = playerTransform.position;
                pos.x = cameraPos.x;
                pos.y = cameraPos.y;
                playerTransform.position = pos;
            }
        }
    }
}
