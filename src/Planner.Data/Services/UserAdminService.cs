using Planner.Core.Models;
using Planner.Data.Repositories;

namespace Planner.Data.Services;

public sealed class UserAdminService
{
    private readonly UserRepository _users;
    public UserAdminService(UserRepository users)=>_users=users;

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct=default)=>_users.GetAllAsync(ct);
    public async Task<long> SaveAsync(User loggedAdmin, User user, CancellationToken ct=default)
    {
        if(loggedAdmin.Role!=UserRoles.Admin) throw new UnauthorizedAccessException("Требуются права администратора.");
        if(string.IsNullOrWhiteSpace(user.Username)||string.IsNullOrWhiteSpace(user.DisplayName)) throw new ArgumentException("Логин и ФИО обязательны.");
        if(user.ParentId==user.Id && user.Id!=0) throw new ArgumentException("Пользователь не может быть начальником самому себе.");
        if(user.Id!=0 && user.ParentId is long parentId && await _users.IsSubordinateAsync(user.Id,parentId,ct))
            throw new ArgumentException("Нельзя назначить подчиненного начальником его собственного руководителя: возникнет цикл иерархии.");
        if(user.Id==0) return await _users.CreateAsync(user,ct);
        await _users.UpdateAsync(user,ct); return user.Id;
    }
}
