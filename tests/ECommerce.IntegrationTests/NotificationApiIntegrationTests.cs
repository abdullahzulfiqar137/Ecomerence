using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class NotificationApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotificationApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Send_Custom_Email_And_Query_Outbox_ApiEndpoint_ShouldSucceed()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 2. Send Custom Email
        var sendDto = new SendEmailRequestDto
        {
            RecipientEmail = "customer.service@test.com",
            RecipientName = "VIP Customer",
            Subject = "Exclusive Invitation",
            BodyHtml = "<p>Welcome to our early access program.</p>",
            Type = NotificationType.WelcomeCustomer
        };

        var sendResponse = await _client.PostAsJsonAsync("/api/v1/notifications/send-custom", sendDto);
        sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sendResult = await sendResponse.Content.ReadFromJsonAsync<ApiResponse<NotificationDto>>();
        sendResult.Should().NotBeNull();
        sendResult!.Success.Should().BeTrue();
        sendResult.Data!.Status.Should().Be(NotificationStatus.Sent);

        // 3. Query Outbox
        var outboxResponse = await _client.GetAsync("/api/v1/notifications/outbox");
        outboxResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var outboxResult = await outboxResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<NotificationDto>>>();
        outboxResult.Should().NotBeNull();
        outboxResult!.Success.Should().BeTrue();
        outboxResult.Data!.Items.Should().Contain(n => n.Id == sendResult.Data.Id);
    }
}
