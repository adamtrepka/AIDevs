using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Abstraction.Api.Answer
{
    public class AnswerRequest<TAnswer>
    {
        public AnswerRequest(TAnswer answer)
        {
            Answer = answer;
        }

        public TAnswer Answer { get; set; }
    }
}
