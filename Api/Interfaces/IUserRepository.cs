using Api.DTOs;
using Api.Entities;
using Api.Helpers;

namespace Api.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberByUsernameAsync(string username, bool ignoreFilter);
    Task<AppUser?> GetUserByPhotoIdAsync(int id);
}
