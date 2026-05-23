using System;

/// <summary>
/// 只读的可绑定属性，提供给 View 层。
/// </summary>
public interface IReadonlyBindableProperty<T> : IObservable<T>
{
    T Value { get; }
    event Action<T, T> OnValueChanged;
    
    /// <summary>
    /// 注册监听并立即用当前值回调（旧值 == 新值 == 当前值）
    /// </summary>
    void RegisterWithInitValue(Action<T, T> callback);
    
    /// <summary>
    /// 取消注册
    /// </summary>
    void Unregister(Action<T, T> callback);
}

/// <summary>
/// 完整的可绑定属性，供 ViewModel 内部使用。
/// </summary>
public interface IBindableProperty<T> : IReadonlyBindableProperty<T>, IDisposable
{
    new T Value { get; set; }
    
    /// <summary>
    /// 强制触发变化通知（即使新旧值相同）
    /// </summary>
    void ForceNotify();
    
    /// <summary>
    /// 静默设置值，不触发任何通知
    /// </summary>
    void SetValueWithoutNotify(T value);
    
    /// <summary>
    /// 释放所有资源，向观察者发送 OnCompleted
    /// </summary>
    new void Dispose();
}