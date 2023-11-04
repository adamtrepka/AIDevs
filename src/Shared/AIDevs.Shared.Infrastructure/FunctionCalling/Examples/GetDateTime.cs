using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.FunctionCalling.Examples
{
    [Description("Return information about current date and time")]
    public record GetDateTime() : IFunctionCallParameters;

    public class GetDateTimeHandler : IFunctionCallHandler<GetDateTime>
    {
        public Task<FunctionCallResult> HandleAsync(GetDateTime parameters)
        {
            var dateTime = DateTime.Now;
            var dateTimeResult = new
            {
                dateTime.TimeOfDay,
                dateTime.Date,
                dateTime.DayOfWeek,
                dateTime.DayOfYear,
            };

            var result = new FunctionCallResult(JsonSerializer.Serialize(dateTimeResult));

            return Task.FromResult(result);
        }
    }
}
