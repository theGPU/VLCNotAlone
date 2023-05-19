using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VLCNotAlone.Shared.Models.Configs
{
    public interface IRemoteConfigModel
    {
        bool IsValid(out List<ValidationResult> results);
    }
}
