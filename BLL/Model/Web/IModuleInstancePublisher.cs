using System.Collections.Generic;

namespace One.Net.BLL
{
    interface IModuleInstancePublisher
    {
        void PublishModuleInstance(int moduleInstanceId, Dictionary<string, BOSetting> instanceSettings);
    }
}
