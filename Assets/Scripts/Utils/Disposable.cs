using System;

namespace Core.Utils
{
    public static class Disposable
    {
        /// <summary>
        /// 一个什么都不做的空 IDisposable
        /// </summary>
        public static IDisposable Empty { get; } = new EmptyDisposable();

        /// <summary>
        /// 根据 Action 创建 IDisposable
        /// </summary>
        public static IDisposable Create(Action disposeAction)
        {
            return new AnonymousDisposable(disposeAction);
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }

        private sealed class AnonymousDisposable : IDisposable
        {
            private Action _disposeAction;
            public AnonymousDisposable(Action disposeAction) => _disposeAction = disposeAction;
            public void Dispose()
            {
                var action = _disposeAction;
                _disposeAction = null;
                action?.Invoke();
            }
        }
    }
}