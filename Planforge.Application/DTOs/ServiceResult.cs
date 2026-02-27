using Microsoft.AspNetCore.Identity;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;

namespace Planforge.Application.DTOs;

public class ServiceResult<T> : IServiceResult<T>
{
    public bool IsSuccessful { get; private set; }
    public string Message { get; private set; }
    public IEnumerable<IdentityError>? Errors { get; private set; }
    public T? Result { get; private set; }
    public ServiceErrorType ErrorType { get; private set; }

    public static ServiceResult<T> Success(T result)
    {
        return new ServiceResult<T>()
        {
            IsSuccessful = true,
            Result =  result,
            Message = string.Empty,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult<T> Failure(string message, ServiceErrorType errorType, IEnumerable<IdentityError>? errors = null)
    {
        return new ServiceResult<T>()
        {
            IsSuccessful = false,
            Errors = errors,
            ErrorType = errorType,
            Message = message
        };
    }
}
