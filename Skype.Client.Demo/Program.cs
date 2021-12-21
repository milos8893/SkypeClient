using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skype.Client.CefSharp.OffScreen;

namespace Skype.Client.Demo
{
    class Program
    {
        private static SkypeCefOffScreenClient client;

        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection(); 
            services.AddLogging(logging =>
            {
                logging.AddDebug();
                logging.AddConsole();
            });

#if DEBUG
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
#endif

            services.AddSingleton<SkypeCefOffScreenClient>();

            var serviceProvider = services.BuildServiceProvider();


            Console.WriteLine("Creating new instance of client");

            client = serviceProvider.GetService<SkypeCefOffScreenClient>();

            client.StatusChanged += OnAppOnStatusChanged;
            client.IncomingCall += (sender, eventArgs) => Console.WriteLine(eventArgs);
            client.CallStatusChanged += (sender, eventArgs) => Console.WriteLine(eventArgs);
            client.MessageReceived += OnClientOnMessageReceived;

            if (args.Length == 2)
            {


                Console.WriteLine("Starting authentication. This might take a few seconds.");

                client.Login(args[0], args[1]);


                //System.Threading.Thread.Sleep(30000);
                //client.SendMessage("8:milos8893", "ALO BRE!");


                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("NO COMMAND LINE!");

                Console.WriteLine("Enter the Skype username:");
                string user = Console.ReadLine();

                Console.WriteLine("Enter the Skype password:");
                string pass = Console.ReadLine();

                Console.WriteLine("Starting authentication. This might take a few seconds.");

                client.Login(user, pass);

                Console.ReadKey();
            }
        }

        private static void OnClientOnMessageReceived(object? sender, MessageReceivedEventArgs eventArgs)
        {
            Console.WriteLine(eventArgs);

            Task.Run(async () => {




                //IMPORTANT IMPORTANT IMPORTANT IMPORTANT 
                //IMPORTANT IMPORTANT IMPORTANT IMPORTANT 
                // 
                // Use client.Login(args[0], args[1]); (see the line ~63 above), to login the skype using the data the user provided in a gui
                // Set the login as success somewhere around the line 110 below.
                // if (eventArgs.New == AppStatus.Ready) means we are logged in!
                // 
                // When he does that, he could choose the filter name, filter trigger text and the group chat where the messages are to be forwarded
                // The user will be able to choose from a dropdown list of groups/contacts.
                // The <list> of the contacts is located at SkypeClient.Contacts, located in Skype.Client.cs
                // See the line ~256 in Skype.Client.cs on more info on the Contacts object

                // Here in OnClientOnMessageReceived you need to check if the eventArgs.ConversationLink is the link of a group the user has been set to monitor
                // Then you need to check if the eventArgs.MessageHtml contains any of the filters the user has set in the interface
                // And if it contains the filter text, then forward the message to the corresponding filter group 
                // The example below just sends the received message back to the conversation where it originated from
                // You just need to change the eventArgs.ConversationLink to corresponding filter group chat
                // simplified example, if we receive the message that contains HelloWorld and if in the gui list one of the filters is set to forward messages containing HelloWorld text to 'Some Group Name' then
                // forward that message to that group
                // 
                //IMPORTANT IMPORTANT IMPORTANT IMPORTANT 
                //IMPORTANT IMPORTANT IMPORTANT IMPORTANT 


                //eventArgs.ConversationLink = @"https://azscus1-client-s.gateway.messenger.live.com/v1/users/ME/conversations/8:milos8893";
                //Console.WriteLine("!!!!!!!!!!!!OVDE IDE: " + eventArgs.ConversationLink);

                //see here the last sent parameter is the chat ID where the message will be redirected
                if (await client.SendMessage(eventArgs, $"{eventArgs.MessageHtml} back!", "19:b1d68239ae60460cb1172c76c947733b@thread.skype"))
                {
                    
                    Console.WriteLine("Automated response sent");
                };
            });
        }

        private static void OnAppOnStatusChanged(object sender, StatusChangedEventArgs eventArgs)
        {
            if (eventArgs.New == AppStatus.Ready)
            {
                Console.WriteLine("Ready! :). You will see incoming messages and calls on this command line shell. Press any key to exit.");
            }
        }
    }
}
