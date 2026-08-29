using System;
using System.Windows;
using KylaPcRental.Client.Services;

namespace KylaPcRental.Client;

public partial class LockWindow : Window
{
    private readonly RentalSession _session;
    private readonly Action _onUnlocked;

    public LockWindow(RentalSession session, Action onUnlocked)
    {
        InitializeComponent();
        _session = session;
        _onUnlocked = onUnlocked;
    }

    private void Extend(int hours)
    {
        _session.AddHours(hours);
        Close();
        _onUnlocked();
    }

    private void OneHour_Click(object sender, RoutedEventArgs e) => Extend(1);
    private void TwoHours_Click(object sender, RoutedEventArgs e) => Extend(2);
    private void ThreeHours_Click(object sender, RoutedEventArgs e) => Extend(3);
}
