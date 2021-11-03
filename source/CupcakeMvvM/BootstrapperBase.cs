using CupcakeMvvM.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace CupcakeMvvM
{
    public abstract class BootstrapperBase
    {
        private bool isInitialized;

        /// <summary>
        /// Creates an instance of the bootstrapper.
        /// </summary>
        protected BootstrapperBase()
        { }

        /// <summary>
        /// The application.
        /// </summary>
        protected System.Windows.Application Application { get; set; }

        protected Window RootWindow { get; private set; }

        /// <summary>
        /// Initialize the framework.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            Execute.Initialize();

            if (ViewTools.InDesignMode)
            {
                try
                {
                    StartDesignTime();
                }
                catch
                {
                    //if something fails at design-time, there's really nothing we can do...
                    isInitialized = false;
                    throw;
                }
            }
            else
            {
                StartRuntime();
            }
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <param name="rootModel">The view model.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The window.</returns>
        protected virtual Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        {
            var view = EnsureWindow(rootModel, ViewLocator.CreateViewForModel(rootModel.GetType()), isDialog);
            view.DataContext = rootModel;
            return view;

            //ViewModelBinder.Bind(rootModel, view, context);

            //var haveDisplayName = rootModel as IHaveDisplayName;
            //if (haveDisplayName != null && !ConventionManager.HasBinding(view, Window.TitleProperty))
            //{
            //   var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
            //   view.SetBinding(Window.TitleProperty, binding);
            //}

            //ApplySettings(view, settings);

            //new WindowConductor(rootModel, view);
        }

        protected void DisplayRootViewFor(object viewModel)
        {
            RootWindow = CreateWindow(viewModel, false, null, null);
            RootWindow.Show();
        }

        /// <summary>
        /// Makes sure the view is a window is is wrapped by one.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <returns>The window.</returns>
        protected virtual Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = view as Window;

            if (window == null)
            {
                window = new Window
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }

            return window;
        }

        /// <summary>
        /// Infers the owner of the window.
        /// </summary>
        /// <param name="window">The window to whose owner needs to be determined.</param>
        /// <returns>The owner.</returns>
        protected virtual Window InferOwnerOf(Window window)
        {
            var application = Application.Current;
            if (application == null)
            {
                return null;
            }

            var active = application.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            active = active ?? (PresentationSource.FromVisual(application.MainWindow) == null ? null : application.MainWindow);
            return active == window ? null : active;
        }

        /// <summary>
        /// Override this to add custom behavior on exit.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnExit(object sender, EventArgs e) { }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected virtual void OnStartup(object sender, StartupEventArgs e) { }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(object sender, Exception e) { }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            Application.Startup += OnStartup;
            Application.DispatcherUnhandledException += ApplicationOnDispatcherUnhandledException;
            Application.Exit += OnExit;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        /// <summary>
        /// Override to tell the framework where to find assemblies to inspect for views, etc.
        /// </summary>
        /// <returns>A list of assemblies to inspect.</returns>
        /// <example>
        ///
        ///   protected override IEnumerable<Assembly> SelectAssemblies()
        ///          {
        ///              /*
        ///              add assembly for each control that this shell will host.
        ///              */
        ///              var assemblies = base.SelectAssemblies().ToList();
        ///              assemblies.Add(typeof(VeeCommonControls.VeeViewModelBase).GetTypeInfo().Assembly);
        ///              assemblies.Add(typeof(MollyConfigViewModel).GetTypeInfo().Assembly);
        ///              assemblies.Add(typeof(MollyObjectHelpers.ConfigViewModelBase).GetTypeInfo().Assembly);
        ///
        ///              return assemblies;
        ///          }
        ///
        ///
        ///
        ///
        /// </example>
        protected virtual IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { GetType().Assembly };
        }

        /// <summary>
        /// Called by the bootstrapper's constructor at design time to start the framework.
        /// </summary>
        protected virtual void StartDesignTime()
        {
            AssemblySource.Initialize(SelectAssemblies());
            Configure();
        }

        /// <summary>
        /// Called by the bootstrapper's constructor at runtime to start the framework.
        /// </summary>
        protected virtual void StartRuntime()
        {
            Application = Application.Current;
            PrepareApplication();

            // TODO [rs]: add EventAggregator support.
            //EventAggregator.HandlerResultProcessing = (target, result) =>
            //{
            //    var task = result as System.Threading.Tasks.Task;
            //    if (task != null)
            //    {
            //        result = new IResult[] { task.AsResult() };
            //    }

            //    var coroutine = result as IEnumerable<IResult>;
            //    if (coroutine != null)
            //    {
            //        var viewAware = target as IViewAware;
            //        var view = viewAware != null ? viewAware.GetView() : null;
            //        var context = new CoroutineExecutionContext { Target = target, View = view };

            //        Coroutine.BeginExecute(coroutine.GetEnumerator(), context);
            //    }
            //};

            AssemblySource.Initialize(SelectAssemblies());

            Configure();
        }

        private void ApplicationOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            OnUnhandledException(sender, ex);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // should be safe to cast e.ExceptionObject to Exception (http://stackoverflow.com/q/913472)
            Exception ex = (Exception)e.ExceptionObject;
            OnUnhandledException(sender, ex);
        }
    }
}