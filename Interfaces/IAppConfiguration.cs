using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IAppConfiguration
    {
        T GetAppSettingValue<T>(string key, T defaultValue) where T : notnull;
    }
}
