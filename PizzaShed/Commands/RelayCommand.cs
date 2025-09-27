using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PizzaShed.Commands
{
    // Class to bind UI controls to methods in our ViewModel
    public class RelayCommand : ICommand
    {
        // Holds references to the functions defined in the ViewModel
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        
        public RelayCommand(Action<object> execute) 
            : this(execute, null)
        { }                
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute; 
            _canExecute = canExecute;   
        }

        // Function to check if the UI control (button) is enabled
        public bool CanExecute(object? parameter)
        {           
            return _canExecute == null || _canExecute(parameter);                        
        }

        // Function called when user triggers the UI control (clicks button)
        // calls the function passed in from the ViewModel along with the command parameter
        public void Execute(object? parameter)
        {
            if(parameter != null)
            {
                _execute(parameter);
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
