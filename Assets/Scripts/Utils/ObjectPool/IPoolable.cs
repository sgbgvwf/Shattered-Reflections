// IPoolable.cs
namespace Core.Pool
{
    /// <summary>
    /// 可回收对象接口。实现此接口的对象在被回收到池中时会收到回调。
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象被回收时调用，可用于重置状态。
        /// </summary>
        void OnRecycledPool();
    }
}