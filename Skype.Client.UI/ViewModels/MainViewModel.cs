using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using Notifications.Wpf.Core;
using Skype.Client.UI.Data;
using Skype.Client.UI.Models;
using Skype.Client.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Skype.Client.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region private fields and commands
        private FilterDbContext db = SingletonDb.FilterDb;
        private Filter _selectedFilter;
        private StringBuilder _logBuilder;
        private bool _isReady = false;

        private RelayCommand _addFilterCommand;
        private RelayCommand _removeFilterCommand;

        private RelayCommand _editFilterCommand;
        #endregion

        #region Properties
        public Filter SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                SelectedFilter.PropertyChanged += SelectedFilter_PropertyChanged;
                OnPropertyChanged();
            }
        }
        public string Log
        {
            get { return _logBuilder.ToString(); }
            //set { _log = value; }
        }

        #endregion

        public ObservableCollection<Filter> Filters { get; set; }
        public MainViewModel()
        {
            _logBuilder = new StringBuilder();
            LogEvent("Starting ...");
            Refresh();
            Helpers.SkypeClient.MessageReceived += SkypeClient_MessageReceived;
            Helpers.SkypeClient.IncomingCall += SkypeClient_IncomingCall;
            Helpers.SkypeClient.StatusChanged += SkypeClient_StatusChanged;
            //this.Log = Helpers.RecordedLog.ToString();
        }

        private void SkypeClient_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            if (e.New == AppStatus.Ready && _isReady == false)
            {
                LogEvent("Skype login Success");
                _isReady = true;
            }

        }
        private void LogEvent(string log)
        {
            _logBuilder.AppendLine($"{DateTime.Now} > {log}");
            OnPropertyChanged(nameof(Log));
        }
        private void SelectedFilter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;
            if (e.PropertyName == nameof(Filter.Name)
             || e.PropertyName == nameof(Filter.Trigger))
                db.SaveChanges();
        }

        private void SkypeClient_IncomingCall(object? sender, CallEventArgs e)
        {
            Helpers.ShowNotification(new NotificationContent
            {
                Title = e.CallerName,
                Type = NotificationType.Information
            });
        }

        private void SkypeClient_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            Helpers.ShowNotification(new NotificationContent
            {
                Title = e.SenderName,
                Type = NotificationType.Information,
                Message = e.MessageHtml
            });

            if (Filters.Count == 0)
                return;

            foreach (var filter in Filters)
            {
                if (filter.SourceChats != null
                   && filter.SourceChats.Count != 0
                   && !string.IsNullOrEmpty(filter.Trigger)
                   && filter.DestinationChats != null
                   && filter.DestinationChats.Count != 0)
                {
                    var profile = filter.SourceChats.FirstOrDefault(
                    (p) => p.UserId == ConversationLinkToId(e.ConversationLink));

                    if (profile != null
                        && e.MessageHtml.Contains(filter.Trigger))
                    {
                        foreach (var item in filter.DestinationChats)
                        {
                            Task.Run(async () =>
                            {
                                if (await Helpers.SkypeClient.SendMessage(e, $"{e.MessageHtml}", item.UserId))
                                    LogEvent($"Forwarded message to > {item.DisplayName}");
                                else
                                    Debug.Write($"Forward failed to > {item.DisplayName}");
                            });
                        }

                    }
                }
            }
        }
        private string PrivateLinkToId(string link)
        {
            //example of link
            // https://azscus1-client-s.gateway.messenger.live.com/v1/threads/19:b1d68239ae60460cb1172c76c947733b@thread.skype
            return link.Replace("https://azscus1-client-s.gateway.messenger.live.com/v1/threads/", "");
        }
        private string ConversationLinkToId(string link)
        {
            //example of link
            // https://azscus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/19:b1d68239ae60460cb1172c76c947733b@thread.skype
            string id = link.Replace("https://azscus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/", "");
            return id;
        }
        private void Refresh()
        {
            Filters = new ObservableCollection<Filter>(SingletonDb.FilterDb.Filters
                .Include(f => f.SourceChats)
                .Include(f => f.DestinationChats)
                .ToList());
        }

        #region Commands
        public RelayCommand AddFilterCommand
        {
            get
            {
                return _addFilterCommand ??= new RelayCommand(async o =>
                {
                    new UpdateFilterWindow().ShowDialog();
                    Refresh();
                });
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeFilterCommand ??= new RelayCommand(async o =>
                {
                    var filter = o as Filter;
                    if (filter == null)
                        return;

                    var result = await Helpers.ShowMessageAsync("Delete Confirmation",
                        $"Delete {filter.Name} ?\n", MessageDialogStyle.AffirmativeAndNegative);
                    if (result == MessageDialogResult.Affirmative)
                    {
                        Filters.Remove(filter);
                        db.Filters.Remove(filter);
                        await db.SaveChangesAsync();
                    }
                });
            }
        }


        public RelayCommand EditFilterCommand
        {
            get
            {
                return _editFilterCommand ??= new RelayCommand(async o =>
                {

                    var filter = o as Filter;
                    if (filter == null)
                        return;
                    new UpdateFilterWindow(filter).ShowDialog();
                    Refresh();
                });
            }
        }



        #endregion
    }
}
