using System.Diagnostics.CodeAnalysis;

namespace LWS_Auth.Models.Inner;

public enum ResultType
{
    Success,
    DataNotFound,
    DataConflicts,
    InvalidRequest,
    UnknownFailure
}

[ExcludeFromCodeCoverage]
public class InternalCommunication<T>
{
    public ResultType ResultType { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
}