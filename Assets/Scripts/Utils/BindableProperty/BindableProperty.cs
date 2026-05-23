using System;
using System.Collections.Generic;
using Core.Utils;

[Serializable]
public class BindableProperty<T> : IBindableProperty<T>
    where T : struct
{
    private T _value;
    private List<IObserver<T>> _observers;
    private bool _disposed;

    public event Action<T, T> OnValueChanged;

    public BindableProperty(T initialValue = default)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set => SetValue(value);
    }

    private void SetValue(T newValue)
    {
        if (_disposed)
            return;

        if (EqualityComparer<T>.Default.Equals(_value, newValue))
            return;

        T oldValue = _value;
        _value = newValue;
        NotifyAll(oldValue, newValue);
    }

    public void ForceNotify()
    {
        if (_disposed) return;
        NotifyAll(_value, _value);
    }

    public void SetValueWithoutNotify(T value)
    {
        if (_disposed) return;
        _value = value;
    }

    public void RegisterWithInitValue(Action<T, T> callback)
    {
        if (callback == null || _disposed) return;
        OnValueChanged += callback;
        callback(_value, _value);
    }

    public void Unregister(Action<T, T> callback)
    {
        OnValueChanged -= callback;
    }

    // ---------- IObservable<T> ----------
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
            throw new ArgumentNullException(nameof(observer));
        if (_disposed)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }

        if (_observers == null)
            _observers = new List<IObserver<T>>(2);
        _observers.Add(observer);
        observer.OnNext(_value);

        return new Subscription(this, observer);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 清除传统事件
        OnValueChanged = null;

        // 通知所有观察者结束
        if (_observers != null)
        {
            foreach (var observer in _observers)
            {
                try { observer.OnCompleted(); } catch { /* 吞掉异常，防止影响其他清理 */ }
            }
            _observers.Clear();
        }
    }

    private void NotifyAll(T oldValue, T newValue)
    {
        OnValueChanged?.Invoke(oldValue, newValue);

        if (_observers != null)
        {
            for (int i = _observers.Count - 1; i >= 0; i--)
            {
                try { _observers[i].OnNext(newValue); }
                catch (Exception ex) { _observers[i].OnError(ex); }
            }
        }
    }

    public static implicit operator T(BindableProperty<T> property)
    {
        return property._value;
    }

    public override string ToString() => _value.ToString();

    // 内部订阅句柄
    private class Subscription : IDisposable
    {
        private BindableProperty<T> _owner;
        private IObserver<T> _observer;

        public Subscription(BindableProperty<T> owner, IObserver<T> observer)
        {
            _owner = owner;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null)
            {
                _owner?._observers?.Remove(_observer);
                _observer = null;
                _owner = null;
            }
        }
    }
}