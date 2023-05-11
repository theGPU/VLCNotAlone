using System;
using System.Collections.Generic;
using System.Text;

namespace VLCNotAlone.Shared
{
    public enum MasterServerError
    {
        NoError,
        CannnotGetIPAddress,
        ServerBanned,
        InvalidPassport
    }
}
