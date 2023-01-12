using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class SourceProfileVM
    {
        public SourceProfileVM()
        {

        }
        public SourceProfileVM(string userId, string displayName, string targetLink)
        {
            UserId = userId;
            DisplayName = displayName;
            TargetLink = targetLink;
        }
        public SourceProfileVM(string userId, string displayName, string targetLink, Filter filter) :
           this(userId, displayName, targetLink)
        {
            Filter = filter;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string TargetLink { get; set; }


        public string? FilterId { get; set; }
        public virtual Filter? Filter { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class DestinationProfileVM
    {
        public DestinationProfileVM()
        {

        }
        public DestinationProfileVM(string userId, string displayName, string targetLink)
        {
            UserId = userId;
            DisplayName = displayName;
            TargetLink = targetLink;
        }
        public DestinationProfileVM(string userId, string displayName, string targetLink,Filter filter):
            this(userId, displayName, targetLink)
        {
            Filter = filter;
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string TargetLink { get; set; }

        public string FilterId { get; set; }
        public virtual Filter Filter { get; set; }
    }
}
