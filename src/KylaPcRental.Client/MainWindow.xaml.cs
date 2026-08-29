using System;
using System.Windows;
using System.Windows.Threading;

namespace KylaPcRental.Client;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private TimeSpan _remaining;
    private bool _sessionActive;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += Timer_Tick;
    }

    private void StartSession(int hours)
    {
        _remaining = TimeSpan.FromHours(hours);
        _sessionActive = true;
        _timer.Start();
        MessageBox.Show($"Session started: {hours} hour(s).\n\nV1 timer engine is now running.", "KYLA PC RENTAL");
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_sessionActive) return;

        _remaining -= TimeSpan.FromSeconds(1);
        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = TimeSpan.Zero;
            _timer.Stop();
            _sessionActive = false;
            ShowExpiryNotice();
        }
    }

    private void ShowExpiryNotice()
    {
        MessageBox.Show(
            "TIME EXPIRED\n\nThe next V1 step will replace this with the full-screen rental lock and extension screen.\n\nThe game process will not be closed by the rental client.",
            "KYLA PC RENTAL");
    }

    private void OneHour_Click(object sender, RoutedEventArgs e) => StartSession(1);
    private void TwoHours_Click(object sender, RoutedEventArgs e) => StartSession(2);
    private void ThreeHours_Click(object sender, RoutedEventArgs e) => StartSession(3);
}
