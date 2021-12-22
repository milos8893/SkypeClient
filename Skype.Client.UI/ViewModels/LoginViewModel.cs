using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Skype.Client.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel, IDisposable
    {
        #region private fields and commands
        private string _username = "appbre@gmail.com";
        private string _password = "aabbcc112233";
        private string _status;

        private RelayCommand _loginCommand;
        #endregion

        public LoginViewModel()
        {
            Helpers.SkypeClient.StatusChanged += SkypeClient_StatusChanged;
        }

        private void SkypeClient_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            Status = e.New.ToString();
            if (e.New == AppStatus.Loading)
            {
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    Helpers.MainFrame.NavigationService.Navigate(new Views.HomePage());
                });
            }
        }

        #region Properties
        public string UserName
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region Commands
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??= new RelayCommand(o =>
                {
                    Helpers.SkypeClient.Login(UserName, Password);

                }, (o) => !String.IsNullOrWhiteSpace(UserName)
                 && !String.IsNullOrWhiteSpace(Password));
            }
        }

        public void Dispose()
        {
            Helpers.SkypeClient.StatusChanged -= SkypeClient_StatusChanged;
        }
        #endregion

    }
}
