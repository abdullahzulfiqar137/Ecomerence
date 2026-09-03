using Xunit;
using FluentAssertions;
using ECommerce.Application.Models;

namespace ECommerce.UnitTests;

public class ArchitectureTests
{
    [Fact]
    public void ApiResponse_SuccessResponse_ShouldSetProperProperties()
    {
        var response = ApiResponse<string>.SuccessResponse("test data", "Operation succeeded");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("test data");
        response.Message.Should().Be("Operation succeeded");
        response.Errors.Should().BeEmpty();
    }
}
