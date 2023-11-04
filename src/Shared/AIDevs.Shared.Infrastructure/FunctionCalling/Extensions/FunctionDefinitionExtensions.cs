using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.FunctionCalling.Extensions
{
    public static class FunctionDefinitionExtensions
    {
        public static FunctionDefinition Create<TParameters>()
            where TParameters : IFunctionCallParameters
        {
            var parametersType = typeof(TParameters);

            var parametersDescriptionAttribute = parametersType.GetCustomAttribute<DescriptionAttribute>();
            var required = new List<string>();

            var properties = parametersType.GetProperties().ToDictionary(x => x.Name, x =>
            {
                var type = x.PropertyType.Name;
                var descriptionAttribute = x.GetCustomAttribute<DescriptionAttribute>();
                var requiredAttribute = x.GetCustomAttribute<RequiredAttribute>();

                if (requiredAttribute is not null)
                {
                    required.Add(x.Name);
                }

                return new PropertyDescriptor(type, descriptionAttribute is not null ? descriptionAttribute.Description : null);
            });

            var parametersDescriptor = properties.Any() ? new ParametersDescriptor("object", properties, required) : ParametersDescriptor.Empty;

            var definition = new FunctionDefinition();

            definition.Name = parametersType.Name;

            if (parametersDescriptionAttribute is not null)
            {
                definition.Description = parametersDescriptionAttribute.Description;
            }

            definition.Parameters = BinaryData.FromObjectAsJson(parametersDescriptor, new() 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            return definition;
        }
    }

    internal record PropertyDescriptor(string Type, string? Description);

    internal record ParametersDescriptor(string Type, Dictionary<string, PropertyDescriptor>? Properties, ICollection<string>? Required)
    {
        public static ParametersDescriptor Empty = new("object", new(), null);
    };
}
