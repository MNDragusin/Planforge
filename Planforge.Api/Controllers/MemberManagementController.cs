using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;

namespace Planforge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberManagementController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly ICurrentTenant _currentTenant;

    public MemberManagementController(IUserAuthService userAuthService, ICurrentTenant currentTenant)
    {
        _userAuthService = userAuthService;
        _currentTenant = currentTenant;
    }

    [HttpPost("newMember")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> InviteNewMember(InviteRequest request)
    {
        //_currentTenant.organizationId
        //due to the tenantMiddleware this is redundant, but we need to have access to the organizationId anyway
        if (!Request.Headers.TryGetValue("X-Organization-Id", out var organizationId))
        {
            return Task.FromResult<IActionResult>(BadRequest());
        }

        /*
            1. Check if the user exists or not
            2. Create user if it doesnt exist
            3. Add the current organization to the new user
        */
        throw new NotImplementedException();
    }

    //TODO InviteRequest should work here as well as a parameter
    [HttpDelete("removeMember")]
    public void RemoveMember()
    {
        /*
            1. Remove role from provided user
        */
        throw new NotImplementedException();
    }

    //TODO
    [HttpGet("getMembers")]
    public void GetMembers()
    {
        throw new NotImplementedException();
    }

    [HttpGet("getMember")]
    public void GetMember()
    {
        throw new NotImplementedException();
    }

    [HttpGet("getRoles")]
    public void GetRoles()
    {
        throw new NotImplementedException();
    }

    [HttpPost("addRole")]
    public void AddRole()
    {
        throw new NotImplementedException();
    }

    [HttpDelete("removeRole")]
    public void RemoveRole()
    {
        throw new NotImplementedException();
    }

    [HttpPost("editMember")]
    public void UpdateMember()
    {
        throw new NotImplementedException();
    }
}