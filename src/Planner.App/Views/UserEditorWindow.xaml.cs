using System.Windows;
using Planner.App.ViewModels;

namespace Planner.App.Views;
public partial class UserEditorWindow:Window
{
    public UserEditorWindow()=>InitializeComponent();
    private void Save_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is not UserEditorViewModel vm)return;
        var u=vm.BuildUser();
        if(string.IsNullOrWhiteSpace(u.Username)||string.IsNullOrWhiteSpace(u.DisplayName))
        {
            MessageBox.Show(this,"Заполните учётную запись Windows и ФИО.","Проверка",MessageBoxButton.OK,MessageBoxImage.Warning);
            return;
        }
        DialogResult=true;
    }
}
