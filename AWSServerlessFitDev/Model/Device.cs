using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Device
    {
        public int? Id { get; set; }
        public string EndpointArn { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }

    }
}
