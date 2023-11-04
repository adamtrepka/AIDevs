using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.FunctionCalling
{
    public interface IFunctionCallDispatcher
    {
        Task<FunctionCallResult> DispatchAsync(string functionName, string parameters);
    }

    internal class FunctionCallDispatcher : IFunctionCallDispatcher
    {
        private readonly IDictionary<string, Type> _functions;
        private readonly IServiceProvider _serviceProvider;

        public FunctionCallDispatcher(IDictionary<string, Type> functions, IServiceProvider serviceProvider)
        {
            _functions = functions;
            _serviceProvider = serviceProvider;
        }
        public async Task<FunctionCallResult> DispatchAsync(string functionName, string parameters)
        {
            var functionCallType = _functions[functionName];
            var functionCallHandlerType = typeof(IFunctionCallHandler<>).MakeGenericType(functionCallType);
            var handler = _serviceProvider.GetService(functionCallHandlerType);

            var functionCall = JsonSerializer.Deserialize(parameters, functionCallType);

            var method = functionCallHandlerType.GetMethods().FirstOrDefault(x => x.Name.EndsWith("HandleAsync"));

            return await (Task<FunctionCallResult>)method.Invoke(handler, new object[] { functionCall });
        }
    }
}
