using System.Windows;
using Planner.App.ViewModels;

namespace Planner.App.Views;
public partial class TaskEditorWindow:Window
{
    public TaskEditorWindow()=>InitializeComponent();
    private void Save_Click(object sender,RoutedEventArgs e){try{if(DataContext is TaskEditorViewModel vm)_=vm.BuildDraft();DialogResult=true;}catch(Exception ex){MessageBox.Show(this,ex.Message,"Проверка",MessageBoxButton.OK,MessageBoxImage.Warning);}}
}
