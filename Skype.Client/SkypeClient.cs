﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Skype.Client.Channel;
using Skype.Client.Protocol.Contacts;
using Skype.Client.Protocol.Conversation;
using Skype.Client.Protocol.Events;
using Skype.Client.Protocol.Events.Resource;
using Skype.Client.Protocol.Events.Resource.Content;
using Skype.Client.Protocol.People;
using Skype.Client.Protocol.Signaling.CallNotification;
using Properties = Skype.Client.Protocol.General.Properties;

namespace Skype.Client
{
    public class SkypeClient
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        protected MessageChannel CallSignalingChannel { get; }

        protected MessageChannel EventChannel { get; }

        protected MessageChannel PropertiesChannel { get; }

        protected MessageChannel ProfilesChannel { get; }

        protected MessageChannel ContactsChannel { get; }

        protected MessageChannel UserPresenceChannel { get; }

        protected MessageChannel ConversationHistoryChannel { get; }

        protected MessageChannel ConversationChatHistoryChannel { get; }

        public event EventHandler<CallEventArgs> IncomingCall;
        public event EventHandler<CallEventArgs> CallStatusChanged;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<EventMessageEventArgs> UnhandledEventMessage;

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public AppStatus Status { get; private set; }

        public SkypeClient() : this(NullLoggerFactory.Instance)
        {
        }

        public SkypeClient(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(SkypeClient));
            _loggerFactory = loggerFactory;

            EventChannel = new MessageChannel(nameof(EventChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            CallSignalingChannel = new MessageChannel(nameof(CallSignalingChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            PropertiesChannel = new MessageChannel(nameof(PropertiesChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            ProfilesChannel = new MessageChannel(nameof(ProfilesChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            ContactsChannel = new MessageChannel(nameof(ContactsChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            UserPresenceChannel = new MessageChannel(nameof(UserPresenceChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            ConversationHistoryChannel = new MessageChannel(nameof(ConversationHistoryChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));
            ConversationChatHistoryChannel = new MessageChannel(nameof(ConversationChatHistoryChannel), loggerFactory.CreateLogger(typeof(MessageChannel)));

            EventChannel.MessagePublished += EventChannelOnMessagePublished;
            CallSignalingChannel.MessagePublished += CallSignalingChannelOnMessagePublished;
            PropertiesChannel.MessagePublished += PropertiesChannelOnMessagePublished;
            ProfilesChannel.MessagePublished += ProfilesChannelOnMessagePublished;
            ContactsChannel.MessagePublished += ContactsChannelOnMessagePublished;
            UserPresenceChannel.MessagePublished += UserPresenceChannelOnMessagePublished;
            ConversationHistoryChannel.MessagePublished += ConversationHistoryChannelOnMessagePublished;
            ConversationChatHistoryChannel.MessagePublished += ConversationChatHistoryChannelOnMessagePublished;
        }

        private void ConversationChatHistoryChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var frame = JsonConvert.DeserializeObject<MessagesFrame>(e.Message);

            if (frame?.Messages != null)
            {
                _logger.LogInformation("Received {numberOfMessages} messages of past conversations", frame.Messages.Length);
            }

        }

        private void ConversationHistoryChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var frame = JsonConvert.DeserializeObject<ConversationsFrame>(e.Message);

            if (frame?.conversations != null)
            {
                _logger.LogInformation("Received {numberOfPastConversations} past conversations", frame.conversations.Length);



                foreach (var contact in frame.conversations)
                {
                    if (!this.Contacts.Exists(p => p.Id == contact.id))
                    {
                        if (!contact.targetLink.Contains(@"/users/"))
                        {
                            var profile = new Profile(contact.id, contact.threadProperties.topic, contact.targetLink);

                            this.Contacts.Add(profile);
                            //_logger.LogInformation("Found new contact: '{displayName}' ({id})", profile.DisplayName, profile.Id);
                            _logger.LogInformation("Found new contact: '{displayName}' ({id}) ({targetLink})", contact.threadProperties.topic, profile.Id, profile.TargetLink);
                        }


                    }
                }





            }
        }

        private void ContactsChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var contactsFrame = JsonConvert.DeserializeObject<ContactsFrame>(e.Message);

            if (contactsFrame.Contacts != null)
            {
                foreach (var contact in contactsFrame.Contacts)
                {
                    if (!this.Contacts.Exists(p => p.Id == contact.Mri))
                    {
                        var contactDisplayName = !string.IsNullOrWhiteSpace(contact.DisplayName) ? contact.DisplayName : contact.Profile.Name.First;
                        var profile = new Profile(contact.Mri, contactDisplayName, contact.TargetLink);

                        this.Contacts.Add(profile);
                        //_logger.LogInformation("Found new contact: '{displayName}' ({id})", profile.DisplayName, profile.Id);
                        _logger.LogInformation("Found new contact: '{displayName}' ({id}) ({targetLink})", profile.DisplayName, profile.Id, $@"https://azscus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/{profile.Id}");
                    }
                }
            }



        }

        private void ProfilesChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var profileFrame = JsonConvert.DeserializeObject<ProfileFrame>(e.Message);

            if (profileFrame == null)
            {
                return;
            }

            foreach (var item in profileFrame.Profiles)
            {
                //var profile = new Profile(item.Key, item.Value.Profile.DisplayName, item.Value.Profile.TargetLink);
                var profile = new Profile(item.Key, item.Value.Profile.DisplayName, $@"https://azscus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/{item.Key}");

                if (item.Value.Authorized)
                {
                    this.Me = profile;

                    if (this.Status != AppStatus.Ready)
                    {
                        _logger.LogInformation("Logged in as '{}', Id: {id}. Client is ready for interactions.", profile.DisplayName, profile.Id);
                        this.UpdateStatus(AppStatus.Ready);
                    }

                }
                else
                {
                    var existing = Contacts.SingleOrDefault(c => c.Id == profile.Id);
                    if (existing != null)
                    {
                        _logger.LogInformation("Updating existing contact '{displayName}' ({id})", existing.DisplayName, existing.Id);
                        existing.DisplayName = profile.DisplayName;
                    }
                    else
                    {
                        _logger.LogInformation("Found new contact: '{displayName}' ({id}) ({targetLink})", profile.DisplayName, profile.Id, profile.TargetLink);
                        this.Contacts.Add(profile);

                    }
                }
            }
        }

        private void PropertiesChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var props = JsonConvert.DeserializeObject<Properties>(e.Message);

            if (props != null)
            {
                this.Properties = props;

                if (e.Response.Headers.AllKeys.Contains("Set-RegistrationToken"))
                {
                    var registrationToken = e.Response.Headers["Set-RegistrationToken"];

                    if (!registrationToken.Equals(Credentials.RegistrationToken))
                    {
                        _logger.LogInformation("Received registrationToken: {registrationToken} (Length: {len})", registrationToken.Substring(0, 50) + "...", registrationToken.Length);
                        this.Credentials.RegistrationToken = registrationToken;
                    }
                }
            }

            if (this.Status != AppStatus.Connected)
            {
                this.UpdateStatus(AppStatus.Connected);
            }
        }

        public CredentialsStore Credentials { get; set; } = new CredentialsStore();

        public Properties Properties { get; private set; }

        public Profile Me { get; set; }

        public List<Profile> Contacts { get; } = new List<Profile>();

        //public async Task<bool> SendMessage(Profile recipient, string message)
        public async Task<bool> SendMessage(MessageReceivedEventArgs recipient, string message, string redirectID = "")
        {

            HttpClient client = new HttpClient();

            var r = new Random(DateTime.Now.Millisecond);

            var barray = new byte[64 / 8];
            r.NextBytes(barray);

            var rint64 = BitConverter.ToUInt64(barray, 0);
            string composetime = $"{DateTime.UtcNow:yyyy-MM-dd}T{DateTime.UtcNow:HH:mm:ss}.{DateTime.UtcNow.Millisecond:D3}Z";
            string clientMessageId = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}{rint64.ToString()}";
            string conversationLink = recipient.ConversationLink + "/messages";

            if (redirectID != "")
                conversationLink = $@"https://azwcus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/{redirectID}/messages";

            string userId = recipient.Sender.Id;
            string messageType = recipient.MessageType;
            //messagetype = "RichText",
            string contentType = recipient.ContentType;
            //contenttype = "text"

            if (clientMessageId.Length > 20)
                clientMessageId = clientMessageId.Substring(0, 20);


            Console.WriteLine("--------------------------------");

            foreach (var item in Contacts)
            {
                Console.WriteLine(item.DisplayName);
                Console.WriteLine(item.Id);
                Console.WriteLine(item.TargetLink);
            }


            Console.WriteLine("--------------------------------");


            var content = JsonConvert.SerializeObject(new
            {
                clientmessageid = clientMessageId,
                composetime = composetime,
                startComposeTime = composetime,
                content = message,
                messagetype = messageType,
                contenttype = contentType
            }
            );



            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = HttpMethod.Post;
            //var userId = recipient.Sender.Id;

            httpRequestMessage.RequestUri = new Uri($"{conversationLink}");
            httpRequestMessage.Headers.Add("RegistrationToken", this.Credentials.RegistrationToken);
            httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpRequestMessage);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return true;
            }




            var responseString = await response.Content.ReadAsStringAsync();
            //_logger.LogError("Error while sending message to '{recipient}'. Status code: {statusCode}, content: {content}", recipient.Sender.DisplayName, response.StatusCode, responseString);
            _logger.LogError("Error while sending message to '{recipient}'. Status code: {statusCode}, content: {content}", "null", response.StatusCode, responseString);
            return false;
        }

        private void UserPresenceChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {

        }

        private void CallSignalingChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            // Video or Audio Call Notification
            var notification = JsonConvert.DeserializeObject<CallNotificationFrame>(e.Message);

            if (notification?.Participants != null)
            {
                _logger.LogInformation("Incoming call from '{caller}'. Call-Id: {callId}", notification.Participants.From.DisplayName, notification.DebugContent.CallId);

                OnIncomingCall(new CallEventArgs
                {
                    Type = CallAction.Incoming,
                    CallerName = notification.Participants.From.DisplayName,
                    CallId = notification.DebugContent.CallId
                });
            }
        }

        private void EventChannelOnMessagePublished(object sender, PublishMessageEventArgs e)
        {
            var messageFrame = JsonConvert.DeserializeObject<EventMessageFrame>(e.Message);

            if (messageFrame.EventMessages != null)
            {
                foreach (var eventMessage in messageFrame.EventMessages)
                {
                    if (HandleUserPresence(eventMessage)) continue;

                    if (HandleEndpointPresence(eventMessage)) continue;

                    if (HandleCallLogMessages(eventMessage)) continue;

                    if (HandleCallPreFlightEvent(eventMessage)) continue;

                    if (HandleChatMessage(eventMessage)) continue;

                    if (HandleTypingMessage(eventMessage)) continue;

                    if (HandleCallUpdates(eventMessage)) continue;

                    if (HandleCustomUserProperties(eventMessage)) continue;

                    OnUnhandledEventMessage(new EventMessageEventArgs { EventMessage = eventMessage });
                    _logger.LogWarning("Unable to handle eventMessage '{id}' of type '{type}' with resource type '{resourceType}'", eventMessage.Id, eventMessage.Type, eventMessage.ResourceType);
                }
            }
        }

        private bool HandleCustomUserProperties(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is CustomUserPropertiesResource res)) return false;

            return true;
        }

        private bool HandleCallPreFlightEvent(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is NewMessageResource res)) return false;

            if (res.MessageType == "Signal/Flamingo")
            {
                _logger.LogInformation("Incoming call from '{}'", res.CallerDisplayName);
                return true;
            }

            return false;
        }

        private bool HandleTypingMessage(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is NewMessageResource res)) return false;

            if (res.MessageType == "Control/Typing")
            {
                _logger.LogInformation("User '{}' is typing", res.ImDisplayName);
                return true;
            }

            return false;
        }

        private bool HandleEndpointPresence(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is EndpointPresenceResource res)) return false;

            return true;
        }

        private bool HandleUserPresence(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is UserPresenceResource res)) return false;

            return true;
        }

        protected void UpdateStatus(AppStatus appStatus)
        {
            if (this.Status == appStatus)
            {
                return;
            }

            var oldStatus = appStatus;
            this.Status = appStatus;

            OnStatusChanged(new StatusChangedEventArgs { Old = oldStatus, New = appStatus });
        }

        private bool HandleCallUpdates(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is NewMessageResource res)) return false;

            var messageType = res.MessageType;
            if (messageType != "Event/Call")
            {
                return false;
            }

            var callInformationXmlString = res.Content;
            if (callInformationXmlString == null)
            {
                return false;
            }

            var serializer = new XmlSerializer(typeof(ParticipantList));
            var byteArray = Encoding.UTF8.GetBytes(callInformationXmlString);
            var callStartedXmlStream = new MemoryStream(byteArray);
            var partsList = (ParticipantList)serializer.Deserialize(callStartedXmlStream);
            if (partsList == null || (partsList.Type != "started" && partsList.Type != "ended"))
            {
                return false;
            }

            if (partsList.Type == "started")
            {
                _logger.LogInformation("Call with '{caller}' has started. Call-Id: {callId}", res.ImDisplayName, partsList.CallId);
            }
            else
            {
                _logger.LogInformation("Call with '{caller}' has ended. Call-Id: {callId}", res.ImDisplayName, partsList.CallId);
            }

            OnCallStatusChanged(new CallEventArgs
            {
                Type = partsList.Type == "started" ? CallAction.Accepted : CallAction.Ended,
                CallerName = res.ImDisplayName,
                CallId = partsList.CallId
            });

            return true;

        }

        private bool HandleChatMessage(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is NewMessageResource res))
            {
                return false;
            }

            var messageType = res.MessageType;
            if (messageType != "RichText" && messageType != @"RichText/UriObject" && messageType != @"RichText/Media_Video" && messageType != @"RichText/Contacts" && messageType != @"RichText/Media_GenericFile")
            {
                return false;
            }

            var messageContent = res.Content;
            if (messageContent == null)
            {
                return false;
            }

            var senderProfileUrl = res.From;
            var contentType = res.ContentType;


            if (senderProfileUrl.Contains(this.Me.Id))
            {
                // Hide own own chat messages and return unhandled.
                return true;
            }

            _logger.LogInformation("Chat message received from '{sender}'. Content: {rawContent}", res.ImDisplayName, messageContent);

            var profile = this.Contacts.SingleOrDefault(c => res.From.Contains(c.Id));

            OnMessageReceived(new MessageReceivedEventArgs
            {
                Sender = profile,
                SenderName = res.ImDisplayName,
                MessageHtml = messageContent,
                MessageType = messageType,
                ContentType = contentType,
                ConversationLink = res.ConversationLink
            });

            return true;

        }

        private bool HandleCallLogMessages(EventMessage eventMessage)
        {
            if (!(eventMessage.Resource is NewMessageResource res))
            {
                return false;
            }

            var callLog = res?.Properties?.CallLog;
            if (callLog == null || callLog.CallState != "declined" && callLog.CallState != "missed")
            {
                return false;
            }

            if (callLog.CallState == "declined")
            {
                _logger.LogInformation("Declined call from '{caller}'. Call-Id: {callId}", callLog.OriginatorParticipant.DisplayName, callLog.CallId);
            }
            else
            {
                _logger.LogInformation("Missed call from '{caller}'. Call-Id: {callId}", callLog.OriginatorParticipant.DisplayName, callLog.CallId);
            }


            OnCallStatusChanged(new CallEventArgs
            {
                Type = callLog.CallState == "declined" ? CallAction.Declined : CallAction.Missed,
                CallerName = callLog.OriginatorParticipant.DisplayName,
                CallId = callLog.CallId
            });

            return true;
        }

        protected virtual void OnIncomingCall(CallEventArgs e)
        {
            IncomingCall?.Invoke(this, e);
        }

        protected virtual void OnCallStatusChanged(CallEventArgs e)
        {
            CallStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnUnhandledEventMessage(EventMessageEventArgs e)
        {
            UnhandledEventMessage?.Invoke(this, e);
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }
    }

    public class CredentialsStore
    {
        public string RegistrationToken { get; set; }
    }
}