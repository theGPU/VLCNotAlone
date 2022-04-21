using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneShared
{
    public static class SharedApi
    {
        public const string apiVersion = "2";
        public const string commandArgsSeparator = "<ENQ>";
        public const string commandEndSeparator = "<ETX>";
        public static string CreateCommand(string command, params string[] parameters) => parameters.Length == 0 ? command : command + commandArgsSeparator + string.Join(commandArgsSeparator, parameters);
        public static string[] DecodeCommand(string command) => command.Split(commandArgsSeparator);
    }
}
