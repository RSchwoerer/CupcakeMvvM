using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CupcakeMvvM.Behaviors
{
    /// <summary>
    /// Activates the Window Loaded
    /// </summary>
    public static class ActivateWindowBehavior
    {
        public static readonly DependencyProperty ActivateOnLoadProperty =
            DependencyProperty.RegisterAttached(
                                                "ActivateOnLoad",
                                                typeof(bool),
                                                typeof(ActivateWindowBehavior),
                                                new PropertyMetadata(false, OnActivateOnLoadPropertyChanged));

        public static bool GetActivateOnLoad(Control control)
        {
            return (bool)control.GetValue(ActivateOnLoadProperty);
        }

        public static void SetActivateOnLoad(Control control, bool value)
        {
            control.SetValue(ActivateOnLoadProperty, value);
        }

        private static void OnActivateOnLoadPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var window = obj as Window;
            if (window == null || !(args.NewValue is bool))
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                window.Loaded += (sender, e) =>
                                 window.Activate();
            }
        }
    }
}