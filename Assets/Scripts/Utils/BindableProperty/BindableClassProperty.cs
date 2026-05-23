using System;
using System.Collections.Generic;
using Core.Utils;

[Serializable]
public class BindableClassProperty<T> : IBindableProperty<T>
    where T : class
{
    private T _value;
    private List<IObserver<T>> _observers;
    private bool _disposed;

    public event Action<T, T> OnValueChanged;

    public BindableClassProperty(T initialValue = default)
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
        if (_disposed) return;

        // 对引用类型，比较引用相等；字符串等需要内容相等，使用 EqualityComparer
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

        OnValueChanged = null;
        if (_observers != null)
        {
            foreach (var obs in _observers)
            {
                try { obs.OnCompleted(); } catch { }
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

    public static implicit operator T(BindableClassProperty<T> property)
    {
        return property._value;
    }

    public override string ToString() => _value?.ToString();

    private class Subscription : IDisposable
    {
        private BindableClassProperty<T> _owner;
        private IObserver<T> _observer;

        public Subscription(BindableClassProperty<T> owner, IObserver<T> observer)
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