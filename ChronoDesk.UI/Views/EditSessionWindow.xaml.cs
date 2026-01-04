using System.Windows;
using ChronoDesk.UI.ViewModels;

namespace ChronoDesk.UI.Views;

public partial class EditSessionWindow : Window
{
    public EditSessionWindow()
    {
        InitializeComponent();
        this.DataContextChanged += EditSessionWindow_DataContextChanged;
    }

    private void EditSessionWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is EditSessionViewModel vm)
        {
            vm.RequestClose += (result) =>
            {
                if (result)
                {
                    this.DialogResult = true;
                }
                this.Close();
            };
        }
    }
}
