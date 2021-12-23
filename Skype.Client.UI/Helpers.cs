using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using Skype.Client.CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Skype.Client.UI
{
    static class Helpers 
    {
        public static Frame MainFrame { get; set; }
        public readonly static SkypeCefOffScreenClient SkypeClient;
        private static NotificationManager _notificationManager;
        public static event Action<string> LogEvent;
        public static MetroWindow MetroWindow
        {
            get
            {
                return Application.Current.Windows.OfType<MetroWindow>().SingleOrDefault(w => w.IsActive);
            }
        }
        public static StringBuilder RecordedLog { get; set; }
        static Helpers()
        {
            SkypeClient = Service.Provider.GetService<SkypeCefOffScreenClient>();
            _notificationManager = new NotificationManager();
            LogEvent += Helpers_LogEvent;
            RecordedLog = new StringBuilder();
        }

        private static void Helpers_LogEvent(string obj)
        {
            RecordedLog.Insert(0,obj);
        }

        public static void Navigate(Page page)
        {
            MainFrame.Navigate(page);
        }

        public static async Task ShowSuccessFullySavedNotificationAsync()
        {
            await ShowNotificationAsync(new NotificationContent
            {
                Title = "Successfully Saved",
                Message = "The item was successfully saved",
                Type = NotificationType.Success
            });
        }
        public static async Task ShowSuccessFullyDeletedNotificationAsync()
        {
            await ShowNotificationAsync(new NotificationContent
            {
                Title = "Successfully Deleted",
                Message = "The item was successfully deleted",
                Type = NotificationType.Success
            });
        }
        public static async Task ShowNotificationAsync(NotificationContent notificationContent)
        {
            await _notificationManager.ShowAsync(notificationContent, "notificationArea");
        }
        public static Task ShowNotification(NotificationContent notificationContent)
        {
            return _notificationManager.ShowAsync(notificationContent, "notificationArea");
        }

        public static async Task<MessageDialogResult> ShowMessageAsync(string title, string message = "", MessageDialogStyle messageDialogStyle = MessageDialogStyle.Affirmative)
        {
            return await MetroWindow.ShowMessageAsync(title, message, messageDialogStyle);
        }
        public static void AddLog(string log)
        {
            LogEvent?.Invoke(log);
        }
    }
}
