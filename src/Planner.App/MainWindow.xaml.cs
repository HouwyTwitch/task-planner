using System.Windows;
using System.Windows.Controls;
using Planner.App.ViewModels;
using Planner.Core.Models;

namespace Planner.App;

public partial class MainWindow:Window
{
    public MainWindow()=>InitializeComponent();
    private async void UsersTree_SelectedItemChanged(object sender,RoutedPropertyChangedEventArgs<object> e)
    {
        if(DataContext is MainViewModel vm && e.NewValue is UserTreeNodeViewModel node) await vm.SelectUserAsync(node.User);
    }

    private async void TaskList_MouseDoubleClick(object sender,System.Windows.Input.MouseButtonEventArgs e)
    {
        if(DataContext is MainViewModel vm && sender is ListBox l && l.SelectedItem is TaskItem t) await vm.EditSpecificTaskAsync(t);
    }

    private void RecurringList_ContextMenuOpening(object sender,ContextMenuEventArgs e)
    {
        if(DataContext is not MainViewModel vm||sender is not ListBox list||list.SelectedItem is not TaskItem task)return;
        list.ContextMenu=BuildContextMenu(vm,task);
    }

    private static ContextMenu BuildContextMenu(MainViewModel vm,TaskItem task)
    {
        var menu=new ContextMenu();
        var open=new MenuItem{Header="Открыть шаблон"};open.Click+=async (_,_)=>await vm.EditSpecificTaskAsync(task);menu.Items.Add(open);
        if(vm.CanManageTask(task))
        {
            var transfer=new MenuItem{Header="Передать задачу"};var targets=vm.GetTransferTargets(task);if(targets.Count==0)transfer.IsEnabled=false;
            foreach(var user in targets){var mi=new MenuItem{Header=user.DisplayName};mi.Click+=async (_,_)=>await vm.TransferTaskAsync(task,user);transfer.Items.Add(mi);}menu.Items.Add(transfer);
            menu.Items.Add(new Separator());var delete=new MenuItem{Header="Удалить шаблон и его повторы"};delete.Click+=async (_,_)=>await vm.DeleteTaskAsync(task);menu.Items.Add(delete);
        }
        return menu;
    }
}
