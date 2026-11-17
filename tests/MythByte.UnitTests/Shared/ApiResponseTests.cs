using MythByte.Shared.Responses;

namespace MythByte.UnitTests.Shared;

public class ApiResponseTests
{
    [Fact]
    public void Ok_WithValue_ReturnsSucceededResponseWithValue()
    {
        var value = "MythByte";

        var response = ApiResponse<string>.Ok(value, "Done");

        Assert.True(response.Succeeded);
        Assert.Equal(value, response.Value);
        Assert.Equal("Done", response.Message);
    }

    [Fact]
    public void Fail_WithMessage_ReturnsFailedResponseWithoutValue()
    {
        var message = "Something went wrong.";

        var response = ApiResponse<string>.Fail(message);

        Assert.False(response.Succeeded);
        Assert.Null(response.Value);
        Assert.Equal(message, response.Message);
    }
}
