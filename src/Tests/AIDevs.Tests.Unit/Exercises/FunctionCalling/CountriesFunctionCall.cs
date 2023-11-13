using AIDevs.Shared.Infrastructure.FunctionCalling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Exercises.FunctionCalling
{
    [Description("Return basic information about country")]
    public class CountriesFunctionCall : IFunctionCallParameters
    {
        [Description("Country name")]
        [Required]
        public string Name { get; set; }
    }

    public class CountriesFunctionCallHandler : IFunctionCallHandler<CountriesFunctionCall>
    {
        public async Task<FunctionCallResult> HandleAsync(CountriesFunctionCall parameters)
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync($"https://restcountries.com/v3.1/name/{parameters.Name}");

            return new(result);
        }
    }
}
