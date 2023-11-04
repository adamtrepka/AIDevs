using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.FunctionCalling.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFunctionCalling(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var functionCallParametersType = typeof(IFunctionCallParameters);

            var functions = assembly
                .GetTypes()
                .Where(x => x.IsInterface is false && functionCallParametersType.IsAssignableFrom(x))
                .ToDictionary(x => x.Name, x => x);

            services.Scan(s => s.FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IFunctionCallHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

            services.AddSingleton<IFunctionCallDispatcher, FunctionCallDispatcher>(serviceProvider =>
            {
                return new FunctionCallDispatcher(functions, serviceProvider);
            });

            return services;
        }
    }
}
