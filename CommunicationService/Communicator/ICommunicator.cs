using CommunicationService.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Communicator
{
    public interface ICommunicator : IDisposable
    {
        event Action<CommunicatorEventArgs> DataReceived;
        void StartReader();
        void CloseReader();
        void Write(IMessage data, MessageKind kind);
        void Write(CommunicatorEventArgs data);
    }
}
