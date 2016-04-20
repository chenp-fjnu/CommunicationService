using CommunicationService.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Communicator
{
    [Serializable]
    public class CommunicatorEventArgs
    {
        public MessageKind Kind { get; set; }
        public IMessage Data { get; set; }
        private CommunicatorEventArgs() { }
        public CommunicatorEventArgs(IMessage data, MessageKind kind)
            : this()
        {
            if (data != null)
            {
                Data = data;
                Kind = kind;
            }
        }
        public override string ToString()
        {
            return string.Format("Kind = {0}, Data = {1}.", Kind, Data.ToString());
        }
    }
}
