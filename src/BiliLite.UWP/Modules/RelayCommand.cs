using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules
{
    public class RelayCommand<T> : ICommand
    {
        private Action<T> _Command;
        private Func<T, bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> command) : this(command, null)
        {
        }

        public RelayCommand(Action<T> command, Func<T, bool> canexecute)
        {
            if (command == null)
            {
                throw new ArgumentException("command");
            }
            _Command = command;
            _CanExecute = canexecute;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute == null ? true : _CanExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _Command((T)parameter);
        }
    }
    public class RelayCommand : ICommand
    {
        private Action _Command;
        private Action<bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action command) : this(command, null)
        {
        }

        public RelayCommand(Action command, Action<bool> canexecute)
        {
            if (command == null)
            {
                throw new ArgumentException("command");
            }
            _Command = command;
            _CanExecute = canexecute;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute == null;
        }

        public void Execute(object parameter)
        {
            _Command();
        }
    }
}
