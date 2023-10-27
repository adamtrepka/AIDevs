using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace AIDevs.Shared.Abstraction.Api.Task
{
    public class TaskResponse
    {
        public int Code { get; set; }
        public string Msg { get; set; }

        [System.Text.Json.Serialization.JsonExtensionData]
        public IDictionary<string, JsonElement> AdditionalData { get; set; }
    }
}
