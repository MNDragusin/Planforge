using Microsoft.AspNetCore.Identity;

namespace Planforge.Application.Common.Interfaces;

public interface IServiceResult
{
    public bool IsSuccessful { get; }
    public IEnumerable<IdentityError>? Errors { get; }
    public string? Result { get; }
}