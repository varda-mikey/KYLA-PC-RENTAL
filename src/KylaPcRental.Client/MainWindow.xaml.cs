using System;
using System.Windows;
using System.Windows.Threading;
using KylaPcRental.Client.Services;

namespace KylaPcRental.Client;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly RentalSession _session = new();
    private readonly WindowsInputService _input = new();
    private LockWindow? _lockWindow;
    private bool _sessionActive;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += Timer_Tick;
    }

    private void StartSession(int hours)
    {
        _session.Start(hours);
        _sessionActive = true;
        _timer.Start();
        MessageBox.Show($"Session started: {hours} hour(s) — ₱{RentalSession.PriceForHours(hours):0}.", "KYLA PC RENTAL");
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_sessionActive) return;

        _session.Tick(TimeSpan.FromSeconds(1));
        if (_session.IsExpired)
        {
            _timer.Stop();
            _sessionActive = false;
            ExpireSession();
        }
    }

    private void ExpireSession()
    {
        // Best-effort pause request. The rental client never closes/kills the game.
        _input.SendEscape();

        _lockWindow = new LockWindow(_session, OnUnlocked)
        {
            Owner = this
        };
        _lockWindow.Show();
        Hide();
    }

    private void OnUnlocked()
    {
        Show();
        _sessionActive = true;
        _timer.Start();
        Activate();
    }

    private void OneHour_Click(object sender, RoutedEventArgs e) => StartSession(1);
    private void TwoHours_Click(object sender, RoutedEventArgs e) => StartSession(2);
    private void ThreeHours_Click(object sender, RoutedEventArgs e) => StartSession(3);
}
