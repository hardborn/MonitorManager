using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.Monitoring.Common
{
    public class CommandResult
    {
        public string mac { get; set; }
        public CommandCode commandType { get; set; }
        public string commandResult { get; set; }
    }
}
