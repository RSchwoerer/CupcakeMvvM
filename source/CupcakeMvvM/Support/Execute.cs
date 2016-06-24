using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CupcakeMvvM.Support
{
    /// <summary>
    ///   Enables easy marshalling of code to the UI thread.
    /// </summary>
    public static class Execute
    {
        private static Dispatcher _Dispatcher;

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void BeginOnUIThread(this Action action)
        {
            ValidateDispatcher();
            _Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Initializes the Dispatcher.
        /// NOTE: Call this from UI thread.
        /// </summary>
        public static void Initialize()
        {
            _Dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///   Executes the action on the UI thread.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static void OnUIThread(this Action action)
        {
            if (CheckAccess())
                action();
            else
            {
                Exception exception = null;
                System.Action method = () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                };
                _Dispatcher.Invoke(method);
                if (exception != null)
                    throw new System.Reflection.TargetInvocationException("An error occurred while dispatching a call to the UI Thread", exception);
            }
        }

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static Task OnUIThreadAsync(this Action action)
        {
            ValidateDispatcher();
            return _Dispatcher.InvokeAsync(action).Task;
        }

        private static bool CheckAccess()
        {
            return _Dispatcher == null || _Dispatcher.CheckAccess();
        }

        private static void ValidateDispatcher()
        {
            if (_Dispatcher == null)
                throw new InvalidOperationException("Not initialized with dispatcher.");
        }
    }
}