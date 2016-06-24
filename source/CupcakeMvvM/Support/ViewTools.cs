using System.ComponentModel;
using System.Windows;

namespace CupcakeMvvM.Support
{
    public static class ViewTools
    {
        private static bool? _InDesignMode;

        /// <summary>
        /// Gets a value that indicates whether the process is running in design mode.
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                if (_InDesignMode == null)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
                    _InDesignMode = (bool)descriptor.Metadata.DefaultValue;
                }

                return _InDesignMode.GetValueOrDefault(false);
            }
        }
    }
}