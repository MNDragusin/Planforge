using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Planforge.Application.DTOs;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Planforge.Tests;

[TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
public class AuthTests: IClassFixture<PlanForgeWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    
    private readonly PlanForgeWebAppFactory _fixture;
    public AuthTests(PlanForgeWebAppFactory fixture, ITestOutputHelper output)
    {
        _client = fixture.CreateClient();
        _output = output;
        _fixture = fixture;
    }
    
    [Fact, Order(1)]
    public async Task RegisterWithValidData()
    {
        var requestDto = new RegisterRequest(_fixture.TestUser, _fixture.TestEmail, _fixture.TestPassword);
        var response = await _client.PostAsJsonAsync("/api/Auth/register", requestDto);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _output.WriteLine(body);
    }

    [Fact, Order(2)]
    public async Task LoginWithValidData()
    {
        LoginRequest requestDto = new LoginRequest(_fixture.TestEmail, _fixture.TestPassword);
        var response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (data is not null)
        {
            _fixture.LoggedInUser = data;
        }
    }

    [Fact, Order(3)]
    public async Task LoginWithInvalidData()
    {
        LoginRequest requestDto = new LoginRequest(_fixture.TestEmail, "wrongPassword");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        LoginRequest requestDto2 = new LoginRequest("wrongEmail", _fixture.TestPassword);
        response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, Order(999)]
    public async Task DeactivateUser()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _fixture.LoggedInUser.Token);
        //TODO redo this awful shit
        _client.DefaultRequestHeaders.Add("X-Organization-Id", _fixture.LoggedInUser.memberships[0].OrgId.ToString());
        
        var result = await _client.PostAsync("/api/Auth/deactivate",  null);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}