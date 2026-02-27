using Microsoft.AspNetCore.Identity;
using Planforge.Application.Common.Enums;

namespace Planforge.Application.Common.Interfaces;

public interface IServiceResult<T>
{
    public bool IsSuccessful { get; }
    public string Message { get; }
    public IEnumerable<IdentityError>? Errors { get; }
    public T? Result { get; }
    public ServiceErrorType ErrorType { get; }
}