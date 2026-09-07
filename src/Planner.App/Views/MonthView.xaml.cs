using System.Windows;
using System.Windows.Controls;
using Planner.App.ViewModels;
using Planner.Core.Models;

namespace Planner.App.Views;

public partial class MonthView:UserControl
{
    public MonthView()=>InitializeComponent();
    private async void Task_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is MainViewModel vm&&sender is Button b&&b.Tag is TaskItem t){vm.SelectTask(t);await vm.EditSpecificTaskAsync(t);}
    }

    private void Task_ContextMenuOpening(object sender,ContextMenuEventArgs e)
    {
        if(DataContext is not MainViewModel vm||sender is not Button b||b.Tag is not TaskItem task)return;
        b.ContextMenu=BuildContextMenu(vm,task);
    }

    private static ContextMenu BuildContextMenu(MainViewModel vm,TaskItem task)
    {
        var menu=new ContextMenu();
        var open=new MenuItem{Header="Открыть задачу"};open.Click+=async (_,_)=>await vm.EditSpecificTaskAsync(task);menu.Items.Add(open);
        var status=new MenuItem{Header="Изменить статус"};
        AddStatus(status,"Ожидает выполнения",TaskStatuses.Pending,vm,task);AddStatus(status,"В работе",TaskStatuses.InProgress,vm,task);AddStatus(status,"Выполнено",TaskStatuses.Completed,vm,task);menu.Items.Add(status);
        if(vm.CanManageTask(task))
        {
            var transfer=new MenuItem{Header="Передать / назначить исполнителя"};var targets=vm.GetTransferTargets(task);if(targets.Count==0)transfer.IsEnabled=false;
            foreach(var user in targets){var mi=new MenuItem{Header=user.DisplayName};mi.Click+=async (_,_)=>await vm.TransferTaskAsync(task,user);transfer.Items.Add(mi);}menu.Items.Add(transfer);
            menu.Items.Add(new Separator());var delete=new MenuItem{Header="Удалить задачу"};delete.Click+=async (_,_)=>await vm.DeleteTaskAsync(task);menu.Items.Add(delete);
        }
        return menu;
    }

    private static void AddStatus(MenuItem parent,string caption,string code,MainViewModel vm,TaskItem task)
    {
        var mi=new MenuItem{Header=caption,IsCheckable=true,IsChecked=task.Status==code};mi.Click+=async (_,_)=>await vm.ChangeStatusAsync(task,code);parent.Items.Add(mi);
    }
}
