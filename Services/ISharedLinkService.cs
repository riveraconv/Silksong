using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silksong.Services
{
    public interface ISharedLinkService
    {
        void ReceiveSharedLink(string url); //this method will be the method called.
        event Action<string>? LinkReceived; //this event allows to views to react when receiving a link.
    }
}
