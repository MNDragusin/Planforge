using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;

namespace Planforge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberManagementController : BaseCustomController
{
    private readonly IUserAuthService _userAuthService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IOrganizationService _organizationService;

    public MemberManagementController(IUserAuthService userAuthService, ICurrentTenant currentTenant, IOrganizationService organizationService)
    {
        _userAuthService = userAuthService;
        _currentTenant = currentTenant;
        _organizationService = organizationService;
    }

    [HttpPost("newMember")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InviteNewMember(InviteRequest request)
    {
        var userDetailsResponse = await _userAuthService.GetActiveUser(request.newMemberEmail);

        if (!userDetailsResponse.IsSuccessful)
        {
            //TODO this should send an email for login and password setup 
            RegisterRequest regRequest = new RegisterRequest(request.name, request.newMemberEmail, "generatedPassword12#$"); //TODO generate a one time password 
            var regResult = await _userAuthService.Register(regRequest);

            if (regResult.IsSuccessful)
            {
                userDetailsResponse = await _userAuthService.GetActiveUser(request.newMemberEmail);
            }
            else
            {
                return MapToErrorActionResult(regResult);
            }
        }

        await _organizationService.AddMember(userDetailsResponse.Result.Id, _currentTenant.OrganizationId!);
        return Ok();
    }

    //TODO
    [HttpGet("getMembers")]
    [ProducesResponseType(typeof(List<MembershipDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers()
    {
        var allUsersResult = await _organizationService.GetAllUser(_currentTenant.OrganizationId);
        if (!allUsersResult.IsSuccessful)
        {
            return MapToErrorActionResult(allUsersResult);
        }

        return Ok(allUsersResult.Result);
    }


    [HttpDelete("removeMember")]
    public async Task<IActionResult> RemoveMember(Guid userId)
    {
        var result = await _organizationService.RemoveUser(userId, _currentTenant.OrganizationId);
        return MapToErrorActionResult<bool>(result);
    }

    //
    // [HttpGet("getMemberDetails")]
    // public void GetMemberDetails()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // [HttpGet("getRoles")]
    // public void GetRoles()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // [HttpPost("addRole")]
    // public void AddRole()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // [HttpDelete("removeRole")]
    // public void RemoveRole()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // [HttpPost("editMember")]
    // public void UpdateMember()
    // {
    //     throw new NotImplementedException();
    // }
}