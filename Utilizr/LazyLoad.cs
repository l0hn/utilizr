using System;

namespace Utilizr
{
    public class LazyLoad<T>
    {
        public bool IsValueLoaded => _loaded;

        public T Value
        {
            get
            {
                if (!_loaded)
                {
                    lock (this)
                    {
                        if (!_loaded)
                        {
                            Load();
                        }
                    }
                }
                return _loadedObject!;
            }
        }

        protected T? _loadedObject;
        protected bool _loaded;
        protected Func<T> _delegate;
        private Type? _typeHint;

        public LazyLoad(Func<T> loader, Type? typeHint = null)
        {
            _delegate = loader;
            _typeHint = typeHint;
        }

        protected virtual void Load()
        {
            _loadedObject = _delegate();
            _loaded = true;
        }

        public bool IsType(Type typeCompare)
        {
            if (_typeHint == null)
                return false;

            return typeCompare == _typeHint;
        }

        public static implicit operator T(LazyLoad<T> ui)
        {
            return ui.Value;
        }
    }
}