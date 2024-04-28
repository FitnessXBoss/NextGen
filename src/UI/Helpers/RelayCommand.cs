using System;
using System.Windows.Input;

namespace NextGen.src.UI.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;  // Объявлено как nullable

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
            return _canExecute == null || _canExecute(); // Безопасный вызов, учитывающий возможность null
        }

        public void Execute(object? parameter)
        {
            _execute(); // Выполнение действия, предполагается, что _execute не может быть null из-за проверки в конструкторе
        }
    }
}
