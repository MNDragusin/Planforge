using Microsoft.AspNetCore.Identity;
using Planforge.Application.Common.Interfaces;

namespace Planforge.Application.DTOs;

public class ServiceResult : IServiceResult
{
    public bool IsSuccessful { get; private set; }
    public IEnumerable<IdentityError>? Errors { get; private set; }
    public string? Result { get; private set; }

    public ServiceResult(bool isSuccessful, string? result = null, IEnumerable<IdentityError>? errors = null)
    {
        IsSuccessful = isSuccessful;
        Errors = errors;
        Result = result;
    }
}
