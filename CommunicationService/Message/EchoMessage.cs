using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Message
{
    [Serializable]
    public class EchoMessage : IMessage
    {
        public string User { get; set; }
        public string Env { get; set; }
        public string Application { get; set; }
        public string Echo { get; set; }

        public override string ToString()
        {
            return string.Format("EchoMessage: User = {0}, Env = {1}, Application = {2}, Echo = {3}", User, Env, Application, Echo);
        }

        public MessageKind Kind
        {
            get { return MessageKind.Echo; }
        }
    }
}
