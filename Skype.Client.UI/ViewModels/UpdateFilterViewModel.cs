using Microsoft.EntityFrameworkCore;
using Skype.Client.UI.Data;
using Skype.Client.UI.Models;
using Skype.Client.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Skype.Client.UI.ViewModels
{
    internal class UpdateFilterViewModel : BaseViewModel
    {
        #region private members
        private Filter _filter;
        private FilterDbContext _db;
        private SourceProfileVM _selectedSource;
        private DestinationProfileVM _selectedDestination;
        private string _filterName;
        private string _filterTrigger;

        private RelayCommand _updateFilterCommand;
        private RelayCommand _addSourceCommand;
        private RelayCommand _removeSourceCommand;
        private RelayCommand _addDestinationCommand;
        private RelayCommand _removeDestionationCommand;
        #endregion

        #region Contstrucotrs
        public UpdateFilterViewModel()
        {
            Filter = new Filter();
            _db = SingletonDb.FilterDb;
        }
        public UpdateFilterViewModel(Filter filter) : this()
        {
            _filter = filter;
        }
        #endregion

        #region Properties
        public Filter Filter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); }
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
        public string FilterName
        {
            get { return Filter.Name; }
            set
            {
                Filter.Name = value;
                OnPropertyChanged();
                _db.SaveChanges();
            }
        }
        public string FilterTrigger
        {
            get { return Filter.Trigger; }
            set
            {
                Filter.Trigger = value;
                OnPropertyChanged();
                _db.SaveChanges();
            }
        }
        public Visibility IsNewItem
        {
            get 
            {
                if (string.IsNullOrEmpty(Filter.Id))
                    return Visibility.Visible;
                return Visibility.Collapsed;
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
        #endregion

        #region Commands
        public RelayCommand UpdateFilterCommand
        {
            get => _updateFilterCommand ??= new RelayCommand(o =>
                {
                    if (string.IsNullOrEmpty(Filter.Id))
                    {
                        _db.Filters.Add(Filter);
                    }
                    else
                    {
                        _db.Filters.Remove(_db.Filters.FirstOrDefault(f => f.Id == Filter.Id));
                        Filter.Id = null;
                        _db.Filters.Add(Filter);
                    }
                    _db.SaveChanges();
                });
        }

        public RelayCommand AddSourceCommand
        {
            get
            {
                return _addSourceCommand ??= new RelayCommand(async o =>
                {

                    if (Filter.SourceChats == null)
                        Filter.SourceChats = new ObservableCollection<SourceProfileVM>();

                    new AddSourceChatWindow(Filter.SourceChats).ShowDialog();
                    await _db.SaveChangesAsync();

                }, (o) => Filter != null);
            }
        }

        public RelayCommand RemoveSourceCommand
        {
            get
            {
                return _removeSourceCommand ??= new RelayCommand(async o =>
                {
                    if (Filter.SourceChats != null)
                    {
                        Filter.SourceChats.Remove(SelectedSource);
                        await _db.SaveChangesAsync();
                    }
                }, (o) => SelectedSource != null);
            }
        }

        public RelayCommand AddDestinationCommand
        {
            get
            {
                return _addDestinationCommand ??= new RelayCommand(async o =>
                {
                    if (Filter.DestinationChats == null)
                        Filter.DestinationChats = new ObservableCollection<DestinationProfileVM>();

                    new AddSourceChatWindow(Filter.DestinationChats).ShowDialog();
                    await _db.SaveChangesAsync();
                }, (o) => Filter != null);
            }
        }

        public RelayCommand RemoveDestionationCommand
        {
            get
            {
                return _removeDestionationCommand ??= new RelayCommand(async o =>
                {
                    if (Filter.DestinationChats != null)
                    {
                        Filter.DestinationChats.Remove(SelectedDestination);
                        await _db.SaveChangesAsync();
                    }
                }, (o) => SelectedDestination != null);
            }
        }
        #endregion
    }
}
