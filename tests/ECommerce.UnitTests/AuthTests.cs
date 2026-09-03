using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services.Authentication;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class AuthTests
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public AuthTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:Secret", "SuperSecretKeyForGlobalECommercePlatform2026MustBeAtLeast32BytesLong!"},
            {"JwtSettings:Issuer", "ECommerceAPI"},
            {"JwtSettings:Audience", "ECommerceClients"},
            {"JwtSettings:ExpiryMinutes", "60"},
            {"JwtSettings:RefreshTokenExpiryDays", "7"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var passwordHasher = new PasswordHasher();
        var jwtTokenGenerator = new JwtTokenGenerator(configuration);
        var userRepository = new UserRepository(_context);

        _authService = new AuthService(
            userRepository,
            passwordHasher,
            jwtTokenGenerator,
            _context,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldCreateUserAndReturnTokens()
    {
        var request = new RegisterRequestDto
        {
            Email = "newuser@ecommerce.global",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Alice",
            LastName = "Smith"
        };

        var response = await _authService.RegisterAsync(request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.User.Email.Should().Be("newuser@ecommerce.global");

        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@ecommerce.global");
        dbUser.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnNewJwtAndRefreshToken()
    {
        // First register
        var registerRequest = new RegisterRequestDto
        {
            Email = "loginuser@ecommerce.global",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Bob",
            LastName = "Marley"
        };
        await _authService.RegisterAsync(registerRequest);

        // Then login
        var loginRequest = new LoginRequestDto
        {
            Email = "loginuser@ecommerce.global",
            Password = "Password123!"
        };

        var response = await _authService.LoginAsync(loginRequest);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ShouldRotateTokenSuccessfully()
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = "tokenuser@ecommerce.global",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Charlie",
            LastName = "Brown"
        };
        var registerResponse = await _authService.RegisterAsync(registerRequest);

        var initialRefreshToken = registerResponse.Data!.RefreshToken;

        var refreshResponse = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            RefreshToken = initialRefreshToken
        });

        refreshResponse.Success.Should().BeTrue();
        refreshResponse.Data.Should().NotBeNull();
        refreshResponse.Data!.RefreshToken.Should().NotBe(initialRefreshToken); // Rotated!
    }
}
