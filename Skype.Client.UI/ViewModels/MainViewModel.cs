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

namespace Skype.Client.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region private fields and commands
        private FilterDbContext db = SingletonDb.FilterDb;
        private string _newFilterName;
        private Filter _selectedFilter;
        private SourceProfileVM _selectedSource;
        private string _selectedTriger;
        private DestinationProfileVM _selectedDestination;
        private string _log;

        private RelayCommand _addFilterCommand;
        private RelayCommand _removeFilterCommand;
        private RelayCommand _addSourceCommand;
        private RelayCommand _removeSourceCommand;
        private RelayCommand _updateTriggerCommand;
        private RelayCommand _addDestinationCommand;
        private RelayCommand _removeDestionationCommand;

        private RelayCommand _editFilterCommand;
        #endregion

        #region Properties
        public string NewFilterName
        {
            get { return _newFilterName; }
            set
            {
                _newFilterName = value;
                OnPropertyChanged();
            }
        }
        public Filter SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                TriggerText = SelectedFilter?.Trigger;
                SelectedFilter.PropertyChanged += SelectedFilter_PropertyChanged;
                OnPropertyChanged();
            }
        }

      

        public SourceProfileVM SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                _selectedSource = value;
                OnPropertyChanged();
            }
        }
        public string TriggerText
        {
            get { return _selectedTriger; }
            set
            {
                _selectedTriger = value;
                OnPropertyChanged();
            }
        }
        public DestinationProfileVM SelectedDestination
        {
            get { return _selectedDestination; }
            set
            {
                _selectedDestination = value;
                OnPropertyChanged();
            }
        }
        public string Log
        {
            get { return _log; }
            set { _log = value; }
        }

        #endregion

        public ObservableCollection<Filter> Filters { get; set; }
        public MainViewModel()
        {
            Filters = new ObservableCollection<Filter>(SingletonDb.FilterDb.Filters
                .Include(f => f.SourceChats)
                .Include(f => f.DestinationChats)
                .ToList());
            Helpers.SkypeClient.MessageReceived += SkypeClient_MessageReceived;
            Helpers.SkypeClient.IncomingCall += SkypeClient_IncomingCall;
            Helpers.LogEvent += Helpers_OnLogEvent;
            this.Log = Helpers.RecordedLog.ToString();
        }

        private void Helpers_OnLogEvent(string obj)
        {
            Log = Helpers.RecordedLog.ToString();
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
                        (p) => p.UserId == e.Sender.Id);

                    if (profile != null
                        && e.MessageHtml.Contains(filter.Trigger))
                    {
                        foreach (var item in filter.DestinationChats)
                        {
                            Task.Run(async () =>
                            {
                                if (await Helpers.SkypeClient.SendMessage(e, $"{e.MessageHtml}", item.UserId))
                                    Debug.Write("Message Sent succesfully");
                                else
                                    Debug.Write("Failed");
                            });
                        }

                    }
                }
            }
        }

        #region Commands
        public RelayCommand AddFilterCommand
        {
            get
            {
                return _addFilterCommand ??= new RelayCommand(async o =>
                {
                    var filter = new Filter
                    {
                        Name = _newFilterName
                    };
                    Filters.Insert(0,filter);
                    SelectedFilter = filter;
                    db.Add(filter);
                    await db.SaveChangesAsync();
                }, (o) => !String.IsNullOrWhiteSpace(_newFilterName));
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
                    
                    Filters.Remove(filter);
                    db.Filters.Remove(filter);
                    await db.SaveChangesAsync();
                });
            }
        }

        public RelayCommand AddSourceCommand
        {
            get
            {
                return _addSourceCommand ??= new RelayCommand(async o =>
                {

                    if (SelectedFilter.SourceChats == null)
                        SelectedFilter.SourceChats = new ObservableCollection<SourceProfileVM>();

                    new AddSourceChatWindow(SelectedFilter.SourceChats).ShowDialog();


                    await db.SaveChangesAsync();
                }, (o) => SelectedFilter != null);
            }
        }

        public RelayCommand RemoveSourceCommand
        {
            get
            {
                return _removeSourceCommand ??= new RelayCommand(async o =>
                {
                    if (SelectedFilter.SourceChats != null)
                    {
                        SelectedFilter.SourceChats.Remove(SelectedSource);
                        await db.SaveChangesAsync();
                    }
                }, (o) => SelectedSource != null);
            }
        }

        public RelayCommand UpdateTriggerCommand
        {
            get
            {
                return _updateTriggerCommand ??= new RelayCommand(async o =>
                {
                    SelectedFilter.Trigger = TriggerText;
                    await db.SaveChangesAsync();
                }, o => !string.IsNullOrEmpty(TriggerText)
                 && SelectedFilter.Trigger != TriggerText);
            }
        }

        public RelayCommand AddDestinationCommand
        {
            get
            {
                return _addDestinationCommand ??= new RelayCommand(async o =>
                {
                    if (SelectedFilter.DestinationChats == null)
                        SelectedFilter.DestinationChats = new ObservableCollection<DestinationProfileVM>();

                    new AddSourceChatWindow(SelectedFilter.DestinationChats).ShowDialog();
                    await db.SaveChangesAsync();
                }, (o) => SelectedFilter != null);
            }
        }

        public RelayCommand RemoveDestionationCommand
        {
            get
            {
                return _removeDestionationCommand ??= new RelayCommand(async o =>
                {
                    if (SelectedFilter.DestinationChats != null)
                    {
                        SelectedFilter.DestinationChats.Remove(SelectedDestination);
                        await db.SaveChangesAsync();
                    }
                }, (o) => SelectedDestination != null);
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
                    SelectedFilter = filter;
                   // new UpdateFilterWindow(this).ShowDialog();

                });
            }
        }

        #endregion
    }
}
