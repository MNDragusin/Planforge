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
    
    public MemberManagementController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [HttpPost("newMember")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> InviteNewMember(InviteRequest request)  
    {
        //due to the tenantMiddleware this is redundant, but we need to have access to the organizationId anyway
        if (!Request.Headers.TryGetValue("X-Organization-Id", out var organizationId))
        {
            return Task.FromResult<IActionResult>(BadRequest());
        }
        
        _userAuthService.
    }
    
    //TODO InviteRequest should work here as well as a parameter
    [HttpDelete("removeMember")]
    public void RemoveMember()
    {
        
    }
    
    //TODO
    [HttpGet("getMembers")]
    public void GetMembers()
    {
    }

    [HttpGet("getMember")]
    public void GetMember()
    {
        
    }
    
    [HttpGet("getRoles")]
    public void GetRoles()
    {
    }
    
    [HttpPost("addRole")]
    public void AddRole()
    {
    }
    
    [HttpDelete("removeRole")]
    public void RemoveRole(){}

    [HttpPost("editMember")]
    public void UpdateMember()
    {
        
    }
}