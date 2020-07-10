using System;
using System.Windows.Input;

namespace WaveEngine_MRTK_Demo.Common.ViewModels.Base
{
    public class RelayCommand<T> : ICommand
    {
        private Action<T> execute;
        private Func<T, bool> canExecute;

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            this.execute?.Invoke((T)parameter);
        }
    }
}
