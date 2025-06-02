using Api.DTOs;
using Api.Entities;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();

        return Ok(users);
    }

    [HttpGet("{username}")] // /api/users/1
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }
}
