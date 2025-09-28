using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PizzaShed.Commands
{
    // Class to bind UI controls to methods in our ViewModel
    public class RelayCommand<T> : ICommand
    {
        // Holds references to the functions defined in the ViewModel
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        
        public RelayCommand(Action<T?> execute) 
            : this(execute, null)
        { }                
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute; 
            _canExecute = canExecute;   
        }

        // Function to check if the UI control (button) is enabled
        public bool CanExecute(object? parameter)
        {           
            return _canExecute == null || _canExecute((T?)parameter);                        
        }

        // Function called when user triggers the UI control (clicks button)
        // calls the function passed in from the ViewModel along with the command parameter
        public void Execute(object? parameter)
        {
            if(parameter != null)
            {
                _execute((T?)parameter);
            }            
        }

        // Function to recheck bound commands 
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value;  }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
