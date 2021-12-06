﻿namespace Skype.Client
{
    public class MessageReceivedEventArgs
    {
        public Profile Sender { get; set; }

        public string SenderName { get; set; }

        public string MessageHtml { get; set; }

        public string MessageType { get; set; }

        public string ContentType { get; set; }

        public string ConversationLink { get; set; }

        public override string ToString()
        {
            return $"New Message from '{SenderName}'. Content: '{MessageHtml}'";
        }
    }
}