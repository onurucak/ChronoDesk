using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.UI.ViewModels;

namespace ChronoDesk.UI;

public partial class MainWindow : Window
{
    private readonly ITimerService _timerService;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private bool _isRealClose = false;

    public MainWindow(MainViewModel viewModel, ITimerService timerService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _timerService = timerService;
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        System.Drawing.Icon trayIcon;
        try
        {
            var iconUri = new Uri("pack://application:,,,/icon.png");
            var resourceStream = System.Windows.Application.GetResourceStream(iconUri);
            if (resourceStream != null)
            {
                using var bitmap = new System.Drawing.Bitmap(resourceStream.Stream);
                trayIcon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            }
            else
            {
                trayIcon = SystemIcons.Application;
            }
        }
        catch
        {
            trayIcon = SystemIcons.Application;
        }

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = trayIcon,
            Visible = true,
            Text = "ChronoDesk"
        };

        _notifyIcon.DoubleClick += (s, e) => ShowWindow();

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("Exit", null, (s, e) => CloseApp());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
        base.OnStateChanged(e);
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void CloseApp()
    {
        // Explicit exit from tray
        _isRealClose = true;
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        System.Windows.Application.Current.Shutdown();
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (_isRealClose)
        {
            base.OnClosing(e);
            return;
        }

        // Prevent default close immediately to check timer
        e.Cancel = true;

        var activeTimer = await _timerService.GetCurrentTimerAsync();
        if (activeTimer != null)
        {
            var result = System.Windows.MessageBox.Show(
                "A timer is currently running.\n\nDo you want to stop the timer and close the application?",
                "Timer Running",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _timerService.StopTimerAsync();
                ProceedWithClosing();
            }
            // If No, do nothing (keep app open)
        }
        else
        {
            // No timer, just close
            ProceedWithClosing();
        }
    }

    private void ProceedWithClosing()
    {
        _isRealClose = true;
        // Verify access on UI thread (OnClosing is UI thread, async continuation implies UI context usually in WPF)
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        System.Windows.Application.Current.Shutdown();
    }
}