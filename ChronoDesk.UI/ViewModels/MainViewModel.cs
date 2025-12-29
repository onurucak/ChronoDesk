using System;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoDesk.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetField(ref _currentViewModel, value);
    }

    public ICommand NavigateToProjectsCommand { get; }
    public ICommand NavigateToTimerCommand { get; }
    public ICommand NavigateToSummaryCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        NavigateToProjectsCommand = new RelayCommand(_ => NavigateTo<ProjectListViewModel>());
        NavigateToTimerCommand = new RelayCommand(_ => NavigateTo<TimerViewModel>());
        NavigateToSummaryCommand = new RelayCommand(_ => NavigateTo<SummaryViewModel>());
        NavigateToSettingsCommand = new RelayCommand(_ => NavigateTo<SettingsViewModel>());

        // Default view
        NavigateTo<TimerViewModel>();
    }

    private void NavigateTo<T>() where T : ViewModelBase
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<T>();
    }
}
