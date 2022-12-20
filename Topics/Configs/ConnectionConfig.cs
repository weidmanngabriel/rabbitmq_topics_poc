using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topics.Configs
{
    internal class ConnectionConfig
    {
        public string HostName { get; set; } = null!;
        public string VirtualHost { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
