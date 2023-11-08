using System;
using System.Windows;
using System.Windows.Threading;

namespace Utilizr.WPF.Util
{
    /// <summary>
    /// LazyLoad but provide a Dispatcher object to invoke the loader.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyLoadDispatch<T> : LazyLoad<T>
    {
        readonly Dispatcher _dispatcher;

        public Action<T>? BeforeUiShown;
        public Action? BeforeUiRemoved;

        /// <summary>
        /// Creates a new LazyLoadDispatch and use the main UI dispatcher
        /// </summary>
        /// <param name="loader"></param>
        public LazyLoadDispatch(Func<T> loader, Type? typeHint = null, Action<T>? beforeUiShown = null, Action? beforeUiRemoved = null)
            : base(loader, typeHint)
        {
            BeforeUiShown = beforeUiShown;
            BeforeUiRemoved = beforeUiRemoved;
            _dispatcher = Application.Current.Dispatcher;
        }

        public LazyLoadDispatch(Func<T> loader, Dispatcher dispatcher, Type? typeHint = null, Action<T>? beforeUiShown = null, Action? beforeUiRemoved = null)
            : base(loader, typeHint)
        {
            BeforeUiShown = beforeUiShown;
            BeforeUiRemoved = beforeUiRemoved;
            _dispatcher = dispatcher;
        }

        protected override void Load()
        {
            if (_dispatcher == null || _dispatcher.CheckAccess())
            {
                base.Load();
            }
            else
            {
                _dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    () =>
                    {
                        _loadedObject = _delegate();
                        BeforeUiShown?.Invoke(_loadedObject!);
                        _loaded = true;
                    }
                );
            }
        }
        
        public void InvokeBeforeUiShown()
        {
            BeforeUiShown?.Invoke(_loadedObject!);
        }

        public void InvokeBeforeUiRemoved()
        {
            BeforeUiRemoved?.Invoke();
        }
    }
}
