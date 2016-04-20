using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Message
{
    [Serializable]
    public class HeartBeatMessage : IMessage
    {
        public string User { get; set; }
        public string Env { get; set; }
        public string Application { get; set; }
        public DateTime TimeStamp { get; set; }
        public override string ToString()
        {
            return string.Format("HeartBeatMessage: User = {0}, Env = {1}, Application = {2}, TimeStamp = {3}", User, Env, Application, TimeStamp.ToString("yyyy-MM-dd HH:mm:ss fff"));
        }

        public MessageKind Kind
        {
            get { return MessageKind.HeartBeat; }
        }
    }
}
