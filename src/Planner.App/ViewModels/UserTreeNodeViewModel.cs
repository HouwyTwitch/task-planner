using System.Collections.ObjectModel;
using Planner.Core.Models;

namespace Planner.App.ViewModels;

public sealed class UserTreeNodeViewModel
{
    public User User { get; }
    public string DisplayName => User.DisplayName;
    public ObservableCollection<UserTreeNodeViewModel> Children { get; } = new();
    public UserTreeNodeViewModel(User user)=>User=user;
}
