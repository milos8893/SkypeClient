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
        private string _username;
        private string _password;
        private string _status;

        private RelayCommand _loginCommand;
        #endregion

        public LoginViewModel()
        {
            Helpers.SkypeClient.StatusChanged += SkypeClient_StatusChanged;
            UserName = Properties.Settings.Default.UserName;
            Password = Base64Decode(Properties.Settings.Default.Password);
        }

        private void SkypeClient_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            Status = e.New.ToString();
            if (e.New == AppStatus.Authenticating)
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
                    SaveUserCredentials();

                }, (o) => !String.IsNullOrWhiteSpace(UserName)
                 && !String.IsNullOrWhiteSpace(Password));
            }
        }
        public void SaveUserCredentials()
        {
            Properties.Settings.Default.UserName = UserName;
            Properties.Settings.Default.Password = Base64Encode(Password);

            Properties.Settings.Default.Save();
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public void Dispose()
        {
            Helpers.SkypeClient.StatusChanged -= SkypeClient_StatusChanged;
        }
        #endregion

    }
}
