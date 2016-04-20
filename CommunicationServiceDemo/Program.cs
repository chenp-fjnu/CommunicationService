using CommunicationService.Message;
using CommunicatorService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicatorServiceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var msg = new HeartBeatMessage()
            {
                User = "me",
                Env = "DEV",
                Application = "test_app",
                TimeStamp = DateTime.Now
            };
            Service.Instance.Subscrible(msg.User, msg.Env, msg.Application, (eventArgs) => Console.WriteLine(eventArgs));
            Console.WriteLine("Input 'C' to exit, input any key to send message.");
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    msg.TimeStamp = DateTime.Now;
                    Service.Instance.Publish(msg.User, msg.Env, msg.Application, msg, true);
                    Thread.Sleep(5000);
                }
            });

            var input = Console.ReadLine();

            var echoMsg = new EchoMessage()
            {
                User = "me",
                Env = "DEV",
                Application = "test_app",
                Echo = "Hello from Application"
            };
            while (input.ToLower() != "c")
            {
                echoMsg.Echo = input;
                Service.Instance.Publish(echoMsg.User, echoMsg.Env, echoMsg.Application, echoMsg, true);
                input = Console.ReadLine();
            }
        }
    }
}
