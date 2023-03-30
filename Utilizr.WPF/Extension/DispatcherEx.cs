using System;
using System.Windows.Threading;
using Utilizr.Logging;

namespace Utilizr.WPF.Extension
{
    public static class DispatcherEx
    {
        public static void SafeInvoke(this Dispatcher d, Action action)
        {
            try
            {
                if (!d.CheckAccess())
                {
                    d.Invoke(
                        DispatcherPriority.Normal,
                        () =>
                        {
                            try
                            {
                                action.Invoke();
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e);
                            }
                        }
                    );
                }
                else
                {
                    action.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        public static void SafeBeginInvoke(this Dispatcher d, Action action)
        {
            try
            {
                if (!d.CheckAccess())
                {
                    d.BeginInvoke(
                        () =>
                        {
                            try
                            {
                                action.Invoke();
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e);
                            }
                        },
                        DispatcherPriority.Normal
                    );
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}
