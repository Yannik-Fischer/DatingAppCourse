using Api.Extensions;
using Api.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var username = resultContext.HttpContext.User.GetUserId();

        var unitOfWork = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var user = await unitOfWork.UserRepository.GetUserByIdAsync(username);
        if (user == null)
        {
            return;
        }

        user.LastActive = DateTime.UtcNow;
        await unitOfWork.Complete();
    }
}
