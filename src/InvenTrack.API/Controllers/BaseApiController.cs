using InvenTrack.Shared.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiResponseOk<T>(ApiResponse<T> response)
    {
        if (response.Success)
            return Ok(response);

        return response.Message switch
        {
            var m when m.Contains("not found") => NotFound(response),
            var m when m.Contains("Unauthorized") => Unauthorized(response),
            _ => BadRequest(response)
        };
    }

    protected IActionResult ApiResponseOk<T>(T data, string message = "Operation successful")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        });
    }
}
