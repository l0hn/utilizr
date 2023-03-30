using System;
using System.Windows;

namespace Utilizr.WPF.Util
{
    public class LazyUI: LazyLoadDispatch<UIElement>
    {
        /// <summary>
        /// Evaluates the lambda returning UIElement only when needed.
        /// </summary>
        /// <param name="loader">The object to return</param>
        /// <param name="uiDispatcher">If not null, will be used to invoke <paramref name="loader"/>. Useful for invoking on the UI thread.</param>
        public LazyUI(Func<UIElement> loader, Type? typeHint = null) 
            : base(loader, Application.Current.Dispatcher, typeHint)
        {

        }
    }
}
