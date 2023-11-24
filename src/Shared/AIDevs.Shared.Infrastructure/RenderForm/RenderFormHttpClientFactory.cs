using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.RenderForm
{
    public static class RenderFormHttpClientFactory
    {
        private static HttpClient? _httpClient;

        public static HttpClient Create()
        {
            if (_httpClient is null)
            {
                var apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableNames.RENDER_FORM_APIKEY_ENVIRONMENT_VARIABLE_NAME);


                _httpClient = new HttpClient()
                {
                    BaseAddress = new Uri("https://get.renderform.io/api/v2/"),
                };

                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            }

            return _httpClient;
        }
    }
}
