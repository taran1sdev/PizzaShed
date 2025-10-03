using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PizzaShed.Commands
{
    // Class to bind UI controls in our ViewModel - relays methods with no parameters
    class RelayGenericCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        public RelayGenericCommand(Action execute)
            :this(execute, null)
        { }
    
        public RelayGenericCommand(Action execute, Func<bool>? canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;   
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        // Function called when the user triggers the UI control
        // calls the bound function
        public void Execute(object? parameter) 
        {     
            _execute();
        }

        // Rechecks bound commands
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
