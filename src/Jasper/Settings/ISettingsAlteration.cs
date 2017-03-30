using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jasper.Settings
{
    public interface ISettingsAlteration
    {
        void Alter(ISettingsProvider value);
    }
}
