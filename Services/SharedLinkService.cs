using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silksong.Services
{
    public class SharedLinkService :  ISharedLinkService
    {
        public event Action<string>? LinkReceived;

        public void ReceivedSharedLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            LinkReceived?.Invoke(url);
        }
    }
}
