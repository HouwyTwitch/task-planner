using System.Collections.ObjectModel;
using Planner.App.Infrastructure;
using Planner.Core.Models;

namespace Planner.App.ViewModels;

public sealed record RoleOption(string Code,string Name);

public sealed class UserEditorViewModel:ObservableObject
{
    private readonly long _id;
    public ObservableCollection<User> PossibleParents{get;}
    public RoleOption[] Roles{get;}=[new(UserRoles.User,"Пользователь"),new(UserRoles.Admin,"Администратор")];
    private string _username="";public string Username{get=>_username;set=>Set(ref _username,value);}
    private string _display="";public string DisplayName{get=>_display;set=>Set(ref _display,value);}
    private User? _parent;public User? Parent{get=>_parent;set=>Set(ref _parent,value);}
    private RoleOption _role;public RoleOption Role{get=>_role;set=>Set(ref _role,value);}
    public UserEditorViewModel(User? existing,IReadOnlyList<User> users)
    {
        _id=existing?.Id??0;PossibleParents=new(users.Where(x=>x.Id!=_id));_role=Roles[0];
        if(existing is not null){Username=existing.Username;DisplayName=existing.DisplayName;Role=Roles.FirstOrDefault(x=>x.Code==existing.Role)??Roles[0];Parent=PossibleParents.FirstOrDefault(x=>x.Id==existing.ParentId);}
    }
    public User BuildUser()=>new(){Id=_id,Username=Username.Trim(),DisplayName=DisplayName.Trim(),ParentId=Parent?.Id,Role=Role.Code};
}
