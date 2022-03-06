using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.Plugins.Interfaces
{
    public interface IConfigurablePlugin : IPlugin
    {
        public string ConfigName { get; }
    }
}
