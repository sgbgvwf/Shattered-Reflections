// PoolManager.cs
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
    /// <summary>
    /// 对象池管理器，负责创建、查找和销毁不同预制体对应的对象池。
    /// 不依赖具体 Prefab，通过静态字典管理多个池。
    /// </summary>
    public static class PoolManager
    {
        // 使用预制体的 GameObject 作为键，保证同一资源只有一个池
        private static readonly Dictionary<GameObject, object> pools = new();

        /// <summary>
        /// 获取指定预制体的对象池。若池不存在则自动创建。
        /// </summary>
        /// <typeparam name="T">预制体上的组件类型</typeparam>
        /// <param name="prefab">预制体组件实例（必须来自资源）</param>
        /// <param name="initialSize">预热数量</param>
        /// <param name="parent">池对象默认父节点</param>
        /// <returns>对象池实例</returns>
        public static ObjectPool<T> GetPool<T>(T prefab, int initialSize = 0, Transform parent = null) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolManager] prefab is null");
                return null;
            }

            GameObject key = prefab.gameObject;

            if (pools.TryGetValue(key, out object existing))
            {
                if (existing is ObjectPool<T> pool)
                    return pool;

                Debug.LogError($"[PoolManager] Type mismatch for prefab '{key.name}'");
                return null;
            }

            var newPool = new ObjectPool<T>(prefab, initialSize, parent);
            pools[key] = newPool;
            return newPool;
        }

        /// <summary>
        /// 销毁指定预制体对应的对象池（同时销毁池内所有对象）。
        /// </summary>
        public static void DestroyPool<T>(T prefab) where T : Component
        {
            if (prefab == null) return;

            GameObject key = prefab.gameObject;
            if (pools.TryGetValue(key, out object obj) && obj is ObjectPool<T> pool)
            {
                pool.Clear();
                pools.Remove(key);
            }
        }

        /// <summary>
        /// 清理所有对象池，释放所有缓存对象。
        /// </summary>
        public static void ClearAll()
        {
            foreach (var kvp in pools)
            {
                // 通过反射调用 Clear，因为字典存储的是 object
                var clearMethod = kvp.Value.GetType().GetMethod("Clear");
                clearMethod?.Invoke(kvp.Value, null);
            }
            pools.Clear();
        }
    }
}