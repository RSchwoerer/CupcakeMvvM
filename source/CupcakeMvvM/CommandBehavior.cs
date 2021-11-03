namespace CupcakeMvvM
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    ///
    /// source: https://sachabarbs.wordpress.com/2009/05/02/wpf-attached-commands/
    ///
    /// </remarks>
    public class CommandBehavior
    {
        public static readonly DependencyProperty CommandParameterProperty =
                    DependencyProperty.RegisterAttached("CommandParameter",
                        typeof(object),
                        typeof(CommandBehavior),
                        new FrameworkPropertyMetadata(null));

        /// <summary>
        /// TheCommandToRun : The actual ICommand to run
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(CommandBehavior),
                new FrameworkPropertyMetadata((ICommand)null));

        /// <summary>
        /// RoutedEventName : The event that should actually execute the
        /// ICommand
        /// </summary>
        public static readonly DependencyProperty RoutedEventNameProperty =
            DependencyProperty.RegisterAttached("RoutedEventName", typeof(string),
            typeof(CommandBehavior),
                new FrameworkPropertyMetadata((string)string.Empty,
                    new PropertyChangedCallback(OnRoutedEventNameChanged)));

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        public static object GetCommandParameter(DependencyObject d)
        {
            return d.GetValue(CommandParameterProperty);
        }

        public static string GetRoutedEventName(DependencyObject d)
        {
            return (string)d.GetValue(RoutedEventNameProperty);
        }

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static void SetCommandParameter(DependencyObject d, object value)
        {
            d.SetValue(CommandParameterProperty, value);
        }

        public static void SetRoutedEventName(DependencyObject d, string value)
        {
            d.SetValue(RoutedEventNameProperty, value);
        }

        /// <summary>
        /// Hooks up a Dynamically created EventHandler (by using the
        /// <see cref="EventHooker">EventHooker</see> class) that when
        /// run will run the associated ICommand
        /// </summary>
        private static void OnRoutedEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string routedEvent = (string)e.NewValue;

            //If the RoutedEvent string is not null, create a new
            //dynamically created EventHandler that when run will execute
            //the actual bound ICommand instance (usually in the ViewModel)
            if (!string.IsNullOrEmpty(routedEvent))
            {
                EventHooker eventHooker = new EventHooker(d, routedEvent);
                eventHooker.HookEvent();

                //EventInfo eventInfo = d.GetType().GetEvent(
                //                                           routedEvent,
                //                                           BindingFlags.Public | BindingFlags.Instance);

                ////Hook up Dynamically created event handler
                //eventInfo?.AddEventHandler(d, eventHooker.GetNewEventHandlerToRunCommand(eventInfo));
            }
        }
    }

    /// <summary>
    /// Contains the event that is hooked into the source RoutedEvent
    /// that was specified to run the ICommand
    /// </summary>
    public sealed class EventHooker
    {
        private string RoutedEventName;

        public EventHooker(DependencyObject objectWithAttachedCommand, string routedEventName)
        {
            ObjectWithAttachedCommand = objectWithAttachedCommand;
            RoutedEventName = routedEventName;
        }


        /// <summary>
        /// The DependencyObject, that holds a binding to the actual
        /// ICommand to execute
        /// </summary>
        public DependencyObject ObjectWithAttachedCommand { get; set; }

        public void HookEvent()
        {
            EventInfo eventInfo = ObjectWithAttachedCommand.GetType().GetEvent(
                                                         RoutedEventName,
                                                         BindingFlags.Public | BindingFlags.Instance);

            //Hook up Dynamically created event handler
            eventInfo?.AddEventHandler(ObjectWithAttachedCommand, GetNewEventHandlerToRunCommand(eventInfo));
        }

        /// <summary>
        /// Creates a Dynamic EventHandler that will be run the ICommand
        /// when the user specified RoutedEvent fires
        /// </summary>
        /// <param name="eventInfo">The specified RoutedEvent EventInfo</param>
        /// <returns>An Delegate that points to a new EventHandler
        /// that will be run the ICommand</returns>
        public Delegate GetNewEventHandlerToRunCommand(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            if (eventInfo.EventHandlerType == null)
                throw new ArgumentException("EventHandlerType is null");

            var del = Delegate.CreateDelegate(
                                              eventInfo.EventHandlerType,
                                              this,
                                              GetType().GetMethod(
                                                                  "OnEventRaised",
                                                                  BindingFlags.NonPublic | BindingFlags.Instance));

            return del;
        }

        /// <summary>
        /// Runs the ICommand when the requested RoutedEvent fires
        /// </summary>
        private void OnEventRaised(object sender, EventArgs e)
        {
            var command = (ICommand)(sender as DependencyObject).GetValue(CommandBehavior.CommandProperty);
            var parameter = (sender as DependencyObject).GetValue(CommandBehavior.CommandParameterProperty);
            command?.Execute(parameter);
        }
    }
}