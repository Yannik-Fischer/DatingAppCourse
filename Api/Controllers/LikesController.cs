using System;
using Api.DTOs;
using Api.Entities;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId)
        {
            return BadRequest("You cannot like yourself");
        }

        var like = await likesRepository.GetUserLike(sourceUserId, targetUserId);
        if (like == null)
        {
            like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            likesRepository.AddLike(like);
        }
        else
        {
            likesRepository.DeleteLike(like);
        }

        if (await likesRepository.SaveChanges())
        {
            return Ok();
        }

        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var users = await likesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(users);
        return Ok(users);
    }
}
