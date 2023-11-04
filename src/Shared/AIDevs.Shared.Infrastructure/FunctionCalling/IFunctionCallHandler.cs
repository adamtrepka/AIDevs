namespace AIDevs.Shared.Infrastructure.FunctionCalling
{
    public interface IFunctionCallHandler<TParameters> where TParameters : IFunctionCallParameters
    {
        Task<FunctionCallResult> HandleAsync(TParameters parameters);
    }
}
