namespace Skype.Client.Protocol.People
{
    public class Profile
    {
        public string DisplayName { get; set; }
        public string Gender { get; set; }
        public string TargetLink { get; set; }
        public Option[] Options { get; set; }
    }
}