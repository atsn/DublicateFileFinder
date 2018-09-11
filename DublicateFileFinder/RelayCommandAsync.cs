using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DublicateFileFinder
{
    public class RelayCommandAsync : ICommand
    {
        private Func<object, Task> execute;

        private Predicate<object> canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public RelayCommandAsync(Func<object, Task> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommandAsync(Func<object, Task> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute != null && this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
            handler?.Invoke(this, EventArgs.Empty);
        }

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }
    }
}