using Skype.Client.UI.Data;
using Skype.Client.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.ViewModels
{
    internal class AddSourceChatViewModel : BaseViewModel
    {

        private RelayCommand _addCommand;
        public AddSourceChatViewModel(ObservableCollection<SourceProfileVM> profiles)
        {
            SourceProfiles = profiles;
        }
        public AddSourceChatViewModel(ObservableCollection<DestinationProfileVM> profiles)
        {
            DestinationProfiles = profiles;
        }

        public ObservableCollection<Profile> Contacts { get; set; } = new ObservableCollection<Profile>(Helpers.SkypeClient.Contacts);
        public Profile SelectedContact { get; set; }
        
        public ObservableCollection<SourceProfileVM> SourceProfiles { get; set; }
        public ObservableCollection<DestinationProfileVM> DestinationProfiles { get; set; }

        public RelayCommand AddCommand
        {
            get
            {
                return _addCommand ??= new RelayCommand(async o =>
                {
                    if (SourceProfiles != null)
                    {
                        SourceProfiles.Add(
                             new SourceProfileVM(SelectedContact.Id, SelectedContact.DisplayName, SelectedContact.TargetLink));
                    }
                    else
                        DestinationProfiles.Add(
                             new DestinationProfileVM(SelectedContact.Id, SelectedContact.DisplayName, SelectedContact.TargetLink));

                }, (o) => SelectedContact != null);
            }
        }
    }
}
