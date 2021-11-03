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
        /// <summary>
        /// A saved reference to the UI  thread Dispatcher.
        /// </summary>
        public static Dispatcher Dispatcher;

        /// <summary>
        /// Executes the action as a fire-and-forget Task on the ThreadPool.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static Task Async(this Action action) => Task.Run(action);

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void BeginOnUIThread(this Action action)
        {
            ValidateDispatcher();
            if (CheckAccess())
            {
                action();
            }
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

                Dispatcher.BeginInvoke(method);

                if (exception != null)
                    throw new System.Reflection.TargetInvocationException(
                        "An error occurred while dispatching a call to the UI Thread",
                        exception);
            }
        }

        /// <summary>
        /// Initializes the Dispatcher.
        /// NOTE: Call this from UI thread.
        /// </summary>
        public static void Initialize()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///   Executes the action on the UI thread.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static void OnUIThread(this Action action)
        {
            ValidateDispatcher();

            if (CheckAccess())
            {
                action();
            }
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

                Dispatcher.Invoke(method);

                if (exception != null)
                    throw new System.Reflection.TargetInvocationException(
                        "An error occurred while dispatching a call to the UI Thread",
                        exception);
            }
        }

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        public static Task OnUIThreadAsync(this Action action)
        {
            ValidateDispatcher();
            return Dispatcher.InvokeAsync(action).Task;
        }

        private static bool CheckAccess()
        {
            return Dispatcher == null || Dispatcher.CheckAccess();
        }

        private static void ValidateDispatcher()
        {
            if (Dispatcher == null)
                throw new InvalidOperationException("Not initialized with dispatcher.");
        }
    }
}