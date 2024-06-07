using System;
using System.Diagnostics;
using System.Windows.Input;

namespace NextGen.src.UI.Helpers
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            bool result = _canExecute == null || (parameter is T t && _canExecute(t));
            Debug.WriteLine($"CanExecute called with parameter: {parameter}, result: {result}");
            return result;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
            {
                Debug.WriteLine($"Execute called with parameter: {parameter}");
                _execute(t);
            }
            else
            {
                throw new ArgumentException("Invalid command parameter type.", nameof(parameter));
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            bool result = _canExecute == null || _canExecute();
            Debug.WriteLine($"CanExecute called with result: {result}");
            return result;
        }

        public void Execute(object? parameter)
        {
            Debug.WriteLine($"Execute called");
            _execute();
        }
    }

}
