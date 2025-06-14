using Api.DTOs;
using Api.Entities;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);
        return Ok(users);
    }

    [HttpGet("{username}")] // /api/users/1
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var currentUsername = User.GetUsername();
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(username, currentUsername == username);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("User update failed");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            IsMain = false,
            IsApproved = false
        };

        user.Photos.Add(photo);

        if (await unitOfWork.Complete())
        {
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Problem while adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain)
        {
            return BadRequest("Cannot use this as main photo");
        }

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null)
        {
            currentMain.IsMain = false;
        }

        photo.IsMain = true;

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null || photo.IsMain)
        {
            return BadRequest("This photo cannot be deleted");
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

        return BadRequest("Problem deleting photo");
    }
}
