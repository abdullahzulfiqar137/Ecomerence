using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Models;
using ECommerce.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Remove the provider-specific SQL Server options registration as well.
            var openGenericTypeName = "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration" + "`1";
            var openGenericType = typeof(DbContextOptions<ApplicationDbContext>).Assembly.GetType(openGenericTypeName);
            if (openGenericType is not null)
            {
                var contextOptionsType = openGenericType.MakeGenericType(typeof(ApplicationDbContext));
                foreach (var descriptor in services.Where(item => item.ServiceType == contextOptionsType).ToArray())
                {
                    services.Remove(descriptor);
                }
            }

            var databaseName = $"ECommerceIntegrationTests_{Guid.NewGuid():N}";
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}

public class AuthApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_And_Login_ApiWorkflow_ShouldSucceed()
    {
        var email = $"integration_{Guid.NewGuid():N}@ecommerce.global";
        var password = "SecurePassword123!";
        var registerDto = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };

        var regResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var regContent = await regResponse.Content.ReadAsStringAsync();
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: regContent);

        var regResult = await regResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        regResult.Should().NotBeNull();
        regResult!.Success.Should().BeTrue();
        regResult.Data!.AccessToken.Should().NotBeNullOrEmpty();
        regResult.Data.RefreshToken.Should().NotBeNullOrEmpty();

        var loginDto = new LoginRequestDto { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        loginResult.Should().NotBeNull();
        loginResult!.Success.Should().BeTrue();
        loginResult.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }
}