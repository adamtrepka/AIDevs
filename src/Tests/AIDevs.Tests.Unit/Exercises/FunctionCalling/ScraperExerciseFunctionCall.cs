using AIDevs.Shared.Infrastructure.FunctionCalling;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Exercises.FunctionCalling
{
    [Description("Return information required to answer the question")]
    public class ScraperExerciseFunctionCall : IFunctionCallParameters
    {
        [Description("Service URL containing information")]
        [Required]
        public string Url { get; set; }
    }

    internal class ScraperExerciseFunctionCallHandler : IFunctionCallHandler<ScraperExerciseFunctionCall>
    {
        public async Task<FunctionCallResult> HandleAsync(ScraperExerciseFunctionCall parameters)
        {
            var policy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var httpMessage = new HttpRequestMessage(HttpMethod.Get, parameters.Url);

            var httpResult = await policy.ExecuteAsync(() => httpClient.SendAsync(httpMessage));

            var content = await httpResult.Content.ReadAsStringAsync();

            return new FunctionCallResult(content);
        }
    }
}
