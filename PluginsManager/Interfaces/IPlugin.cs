using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.Plugins.Interfaces
{
    public interface IPlugin
    {
        public string Name { get; }
        public string Description { get; }
    }
}
