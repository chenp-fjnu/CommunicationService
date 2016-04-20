using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Message
{
    public interface IMessage
    {
        MessageKind Kind { get; }
    }
}
