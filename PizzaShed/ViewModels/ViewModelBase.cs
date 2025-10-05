using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace PizzaShed.ViewModels
{
    // Base class to implement INotifyPropertyChanged
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }        
        protected bool SetProperty<T>(ref T field, T newvalue, [CallerMemberName] string? name = null) 
        {
            if (EqualityComparer<T>.Default.Equals(field, newvalue))
            {
                return false;
            }

            field = newvalue;

            OnPropertyChanged(name);

            return true;
        }

    }
}




