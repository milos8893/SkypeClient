namespace Skype.Client
{
    public class Profile
    {
        public Profile()
        {

        }
        public Profile(string userId, string displayName, string targetLink)
        {
            Id = userId;
            DisplayName = displayName;
            TargetLink = targetLink;
        }


        public string Id { get; }
        public string DisplayName { get; set; }
        public string TargetLink { get; set; }

    }
}