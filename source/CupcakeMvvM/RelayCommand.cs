using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CupcakeMvvM
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// Can be used alongside CommandParameter when binding to any object
    /// </summary>
    /// <remarks>
    ///
    /// source: http://mvvmfoundation.codeplex.com/
    ///
    /// </remarks>
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _CanExecute;
        private readonly Action<T> _Execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class and the command can always be executed.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _Execute = execute;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Event for when method changes if it can execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_CanExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_CanExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Checks if the method can execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _CanExecute?.Invoke((T)parameter) ?? true;
        }

        /// <summary>
        /// Execute method of command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(Object parameter)
        {
            _Execute((T)parameter);
        }
    }

    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> _CanExecute;
        private readonly Action _Execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class and the command can always be executed.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _Execute = execute;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Called when method changes if it can execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_CanExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_CanExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// True if method can execute
        /// </summary>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _CanExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// Execute method of a command
        /// </summary>
        public void Execute(object parameter)
        {
            _Execute();
        }
    }
}