using Api.DTOs;
using Api.Entities;
using Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles))
        {
            return BadRequest("You must select at least one role");
        }

        var selectedRoles = roles.Split(",").ToArray();
        var user = await userManager.FindByNameAsync(username.ToLower());

        if (user == null)
        {
            return BadRequest("User not found");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to add to roles");
        }

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to remove user from roles");
        }

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
    {
        return Ok(await unitOfWork.PhotoRepository.GetUnapprovedPhotos());
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("approve-photo/{id:int}")]
    public async Task<ActionResult> ApprovePhoto(int id)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(id);
        if (photo == null)
        {
            return BadRequest("Unable to get photo");
        }

        photo.IsApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByPhotoIdAsync(id);
        if (user == null)
        {
            return BadRequest("No user associated with photo");
        }

        if (user.Photos.All(x => !x.IsMain))
        {
            photo.IsMain = true;
        }

        if (await unitOfWork.Complete())
        {
            return Ok();
        }

        return BadRequest("Failed to approve photo");
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("reject-photo/{id:int}")]
    public async Task<ActionResult> RejectPhoto(int id)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(id);

        if (photo == null)
        {
            return BadRequest("Photo not found");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        unitOfWork.PhotoRepository.RemovePhoto(photo);

        if (await unitOfWork.Complete())
        {
            return Ok();
        }

        return BadRequest("Failed to reject photo");
    }
}
