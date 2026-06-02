using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GameSO
{
    [CreateAssetMenu(fileName = "GameScene", menuName = "Scene/GameSceneSO")]
    public class GameSceneSO : ScriptableObject
    {
        public AssetReference sceneReference;   // Addressable 场景引用

        /// <summary>
        /// 加载场景（叠加模式，自动激活）
        /// </summary>
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode mode = LoadSceneMode.Additive, bool activateOnLoad = true)
        {
            return sceneReference.LoadSceneAsync(mode, activateOnLoad);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public AsyncOperationHandle UnLoadScene()
        {
            return sceneReference.UnLoadScene();
        }
    }
}
