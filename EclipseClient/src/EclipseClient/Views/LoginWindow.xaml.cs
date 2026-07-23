using System.Windows;
using EclipseClient.Helpers;
using EclipseClient.Services;

namespace EclipseClient.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        WindowHelper.EnableGlass(this);
        WindowHelper.EnableDrag(this, TitleBar);
        TryAutoLogin();
    }

    private void TryAutoLogin()
    {
        var session = SessionService.LoadSession();
        if (session == null) return;

        EmailBox.Text = session.Email;
        RememberMeBox.IsChecked = true;

        // Attempt silent login with stored email (user re-enters password or we skip)
        EmailBox.Text = session.Email;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter email and password.");
            return;
        }

        if (!AuthService.Login(email, password, out var error))
        {
            ShowError(error);
            return;
        }

        SessionService.SaveSession(email, RememberMeBox.IsChecked == true);

        var main = new MainWindow();
        main.Show();
        Close();
    }

    private void ShowError(string msg)
    {
        ErrorText.Text = msg;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
