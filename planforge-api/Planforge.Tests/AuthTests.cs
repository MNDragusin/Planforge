using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Planforge.Application.DTOs;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Planforge.Tests;

[TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
public class AuthTests : IClassFixture<PlanForgeWebAppFactory>
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
        data.Should().NotBeNull();
        data!.Token.Should().NotBeNullOrWhiteSpace();
        data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        _fixture.LoggedInUser = data;
    }

    [Fact, Order(3)]
    public async Task RefreshWithValidRefreshToken()
    {
        var requestDto = new RefreshRequest(_fixture.LoggedInUser.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await response.Content.ReadFromJsonAsync<RefreshResponse>();
        data.Should().NotBeNull();
        data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        data.RefreshToken.Should().NotBe(_fixture.LoggedInUser.RefreshToken);
        _fixture.RefreshedTokens = data;
    }

    [Fact, Order(4)]
    public async Task LoginWithInvalidData()
    {
        LoginRequest requestDto = new LoginRequest(_fixture.TestEmail, "wrongPassword");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        LoginRequest requestDto2 = new LoginRequest("wrongEmail", _fixture.TestPassword);
        response = await _client.PostAsJsonAsync("/api/Auth/login", requestDto2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, Order(5)]
    public async Task LogoutWithValidRefreshToken()
    {
        _fixture.SetHeaders(_client);

        var requestDto = new RefreshRequest(_fixture.LoggedInUser.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/Auth/logout", requestDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, Order(999)]
    public async Task DeactivateUser()
    {
        _fixture.SetHeaders(_client);

        var result = await _client.PostAsync("/api/Auth/deactivate", null);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}