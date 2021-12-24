using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImAUA.UI
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value;OnPropertyChanged(); }
        }
        private string _password;

        public string Password
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
