using CupcakeMvvM.Extensions;
using CupcakeMvvM.Support;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Input;

namespace CupcakeMvvM.Framework
{
    /// <summary>
    /// A base class that implements the infrastructure for property change notification and automatically performs UI thread marshalling.
    /// </summary>
    /// <remarks>
    /// source: https://github.com/Caliburn-Micro/Caliburn.Micro
    /// </remarks>
    public class PropertyChangedBase : INotifyPropertyChangedEx
    {
        /// <summary>
        /// Creates an instance of <see cref = "PropertyChangedBase" />.
        /// </summary>
        public PropertyChangedBase()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            IsNotifying = true;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Enables/Disables property change notification.
        /// Virtualized in order to help with document oriented view models.
        /// </summary>
        public virtual bool IsNotifying { get; set; }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public virtual void Refresh()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// Supports `CallerMemberName` when `propertyName` is null.
        /// </summary>
        /// <param name = "propertyName">Name of the property, or `null` for `CallerMemberName` support.</param>
        public virtual void NotifyOfPropertyChange([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (IsNotifying && PropertyChanged != null)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                //Execute.OnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));

                // this is here specifically for updating the CanExecute status for ICommands.
                // might be a bit of a HACK putting it here, but... ¯\_(ツ)_/¯
                CommandManager.InvalidateRequerySuggested();
                //Execute.OnUIThread(CommandManager.InvalidateRequerySuggested);
            }
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifyOfPropertyChange(property.GetMemberInfo().Name);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged" /> event directly.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, e);
        }
    }
}