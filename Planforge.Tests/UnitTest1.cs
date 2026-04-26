using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Planforge.Application.DTOs;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Planforge.Tests;

[TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
public class UnitTest1: IClassFixture<PlanForgeWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    
    private readonly string _testUser = "test";
    private readonly string _testEmail = "test@test.com";
    private readonly string _testPassword = "iop890IOP*()";

    private LoginResponse _loggedInUser;
    
    public UnitTest1(PlanForgeWebAppFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }
    
    [Fact, Order(1)]
    public async Task RegisterWithValidData()
    {
        var requestDto = new RegisterRequest(_testUser, _testEmail, _testPassword);
        var response = await _client.PostAsJsonAsync("/api/Auth/register", requestDto);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _output.WriteLine(body);
    }

    [Fact, Order(2)]
    public async Task LoginWithValidData()
    {
        LoginRequest requestDto = new LoginRequest(_testEmail, _testPassword);
        var response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (data is not null)
        {
            _loggedInUser = data;
        }
    }

    [Fact, Order(3)]
    public async Task LoginWithInvalidData()
    {
        LoginRequest requestDto = new LoginRequest(_testEmail, "wrongPassword");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        LoginRequest requestDto2 = new LoginRequest("wrongEmail", _testPassword);
        response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, Order(999)]
    public async Task DeactivateUser()
    {
        Assert.Fail("Not implemented yet.");
    }
}