using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.Plugins.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        public const int VeryHigh = -200;
        public const int High = -100;
        public const int Normal = 0;
        public const int Low = 100;
        public const int VeryLow = 200;

        public int Priority { get; protected set; }

        public PriorityAttribute(int priority)
        {
            this.Priority = priority;
        }
    }
}
