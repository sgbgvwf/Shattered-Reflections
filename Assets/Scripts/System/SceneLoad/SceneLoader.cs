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
                isLoading = true;
                StartCoroutine(LoadScenesCoroutine(new List<GameSceneSO> { firstLoadScene }, Vector2.zero, true, () =>
                {
                    isLoading = false;
                    EventBus.Instance.Publish(afterSceneLoadedEvent);
                }));
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

            foreach (var scene in allLoadedScenes)
            {
                if (scene != null)
                {
                    var op = scene.sceneReference.UnLoadScene();
                    yield return op;
                }
            }
            allLoadedScenes.Clear();

            yield return LoadScenesCoroutine(scenesToLoad, cameraPos, true, () =>
            {
                isLoading = false;
                EventBus.Instance.Publish(afterSceneLoadedEvent);
            });
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

            if (scenesToUnload != null)
            {
                foreach (var scene in scenesToUnload)
                {
                    if (scene == null || !allLoadedScenes.Contains(scene)) continue;
                    var op = scene.sceneReference.UnLoadScene();
                    yield return op;

                    if (op.IsValid() && op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        allLoadedScenes.Remove(scene);
                    }
                }
            }

            yield return LoadScenesCoroutine(scenesToLoad, cameraPos, true, () =>
            {
                isLoading = false;
                EventBus.Instance.Publish(afterSceneLoadedEvent);
            });
        }

        private IEnumerator LoadScenesCoroutine(List<GameSceneSO> scenesToLoad, Vector2 cameraPos, bool setCamera, System.Action onComplete = null)
        {
            if (scenesToLoad == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            foreach (var scene in scenesToLoad)
            {
                if (scene == null) continue;
                if (allLoadedScenes.Contains(scene)) continue;

                var op = scene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
                yield return op;

                if (op.IsValid() && op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    allLoadedScenes.Add(scene);
                }
            }

            if (setCamera && playerTransform != null)
            {
                Vector3 pos = playerTransform.position;
                pos.x = cameraPos.x;
                pos.y = cameraPos.y;
                playerTransform.position = pos;
            }

            onComplete?.Invoke();
        }
    }
}
