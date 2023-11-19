using AIDevs.Shared.Infrastructure.FunctionCalling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AIDevs.Tests.Unit.Exercises.FunctionCalling
{
    [Description("Saves a reminder")]
    public class ToolFunctionCall : IFunctionCallParameters
    {
        [Required]
        [Description("Tool type. Accepted options is: \"ToDo\", \"Calendar\"")]
        public string tool { get; set; }

        [Required]
        [Description("Reminder description")]
        public string desc { get; set; }

        [Description("Date of event")]
        public string date { get; set; }
    }
}
