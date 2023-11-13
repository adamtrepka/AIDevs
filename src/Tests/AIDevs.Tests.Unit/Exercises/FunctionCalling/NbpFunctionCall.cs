using AIDevs.Shared.Infrastructure.FunctionCalling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Exercises.FunctionCalling
{
    [Description("Return current table of exchange rates")]
    public record NbpFunctionCall() : IFunctionCallParameters;

    public class NbpFunctionCallHandler : IFunctionCallHandler<NbpFunctionCall>
    {
        public async Task<FunctionCallResult> HandleAsync(NbpFunctionCall parameters)
        {
            var httpClient = new HttpClient();
            var httpResult = await httpClient.GetStringAsync("http://api.nbp.pl/api/exchangerates/tables/A/");

            return new FunctionCallResult(httpResult);
        }
    }
}
