// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Pool
{
    /// <summary>
    /// 泛型对象池。满足：Get/Recycle、Queue 存储、自动扩容、支持预热、回收回调。
    /// </summary>
    /// <typeparam name="T">池中对象的组件类型，必须继承 Component</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new();

        // 记录预制体的默认变换，用于重置
        private readonly Vector3 defaultLocalPosition;
        private readonly Quaternion defaultLocalRotation;
        private readonly Vector3 defaultLocalScale;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="prefab">要池化的预制体组件</param>
        /// <param name="initialSize">预热数量</param>
        /// <param name="parent">回收时对象的默认父节点</param>
        public ObjectPool(T prefab, int initialSize = 0, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;

            defaultLocalPosition = prefab.transform.localPosition;
            defaultLocalRotation = prefab.transform.localRotation;
            defaultLocalScale = prefab.transform.localScale;

            WarmUp(initialSize);
        }

        /// <summary>
        /// 当前池中可用对象数量。
        /// </summary>
        public int Count => pool.Count;

        /// <summary>
        /// 从池中获取一个可用对象。池空时自动创建新对象（自动扩容）。
        /// </summary>
        public T Get()
        {
            T obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
                // 重置为预制体默认的本地变换
                obj.transform.localPosition = defaultLocalPosition;
                obj.transform.localRotation = defaultLocalRotation;
                obj.transform.localScale = defaultLocalScale;
                obj.gameObject.SetActive(true);
                return obj;
            }

            // 自动扩容：创建新对象并直接激活
            obj = CreateNewObject();
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 将对象回收到池中。
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null) return;

            // 调用回收回调（如果实现了 IPoolable）
            if (obj is IPoolable poolable)
                poolable.OnRecycledPool();

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parent);       // 放回池的父节点下
            obj.transform.localPosition = defaultLocalPosition;
            obj.transform.localRotation = defaultLocalRotation;
            obj.transform.localScale = defaultLocalScale;
            pool.Enqueue(obj);
        }

        /// <summary>
        /// 预热池，提前创建指定数量的对象并放入池中。
        /// </summary>
        private void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T obj = CreateNewObject();
                pool.Enqueue(obj);   // 直接入队，不激活
            }
        }

        /// <summary>
        /// 创建新对象实例，默认为禁用状态。
        /// </summary>
        private T CreateNewObject()
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 清空池中所有对象并销毁。
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                if (obj != null)
                    Object.Destroy(obj.gameObject);
            }
        }
    }
}