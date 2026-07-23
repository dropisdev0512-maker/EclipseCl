using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using EclipseClient.Helpers;
using EclipseClient.Models;
using EclipseClient.Services;
using EclipseClient.Themes;

namespace EclipseClient.Views;

public partial class MainWindow : Window
{
    private readonly SystemMonitorService _monitor = new();
    private readonly DispatcherTimer _uiTimer;
    private string _currentTab = "SPVP";
    private string _searchFilter = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        WindowHelper.EnableGlass(this);
        WindowHelper.EnableDrag(this, TitleBar);

        if (AuthService.IsAdmin)
            TabAdmin.Visibility = Visibility.Visible;

        SearchBox.Text = "Search modules...";
        SearchBox.GotFocus += (_, _) => { if (SearchBox.Text == "Search modules...") SearchBox.Text = ""; };
        SearchBox.LostFocus += (_, _) => { if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = "Search modules..."; };

        _monitor.StatsUpdated += OnStatsUpdated;
        InjectionService.InjectionCompleted += OnInjectionCompleted;

        var refreshHz = Math.Clamp(SettingsService.Current.UiRefreshHz, 60, 360);
        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / refreshHz) };
        _uiTimer.Tick += (_, _) => _monitor.Refresh();
        _uiTimer.Start();

        LoadTab("SPVP");
    }

    private void OnStatsUpdated(SystemStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            var connected = stats.MinecraftConnected;
            var connText = connected ? "MC Connected" : "MC Disconnected";
            var connColor = connected
                ? (Brush)FindResource("SuccessBrush")
                : (Brush)FindResource("DangerBrush");

            ConnText.Text = connText;
            ConnDot.Fill = connColor;
            FooterConn.Text = connText;
            FooterFps.Text = connected ? $"FPS: {stats.Fps}" : "FPS: --";
            FooterRam.Text = $"RAM: {stats.RamMb} MB";
            FooterCpu.Text = $"CPU: {stats.CpuPercent}%";
        });
    }

    private void OnInjectionCompleted(string message, bool success)
    {
        Dispatcher.Invoke(() =>
        {
            InjectStatus.Text = message;
            InjectStatus.Foreground = success
                ? (Brush)FindResource("SuccessBrush")
                : (Brush)FindResource("DangerBrush");
        });
    }

    private void Tab_Changed(object sender, RoutedEventArgs e)
    {
        if (TabSpvp.IsChecked == true) LoadTab("SPVP");
        else if (TabMace.IsChecked == true) LoadTab("Mace");
        else if (TabMisc.IsChecked == true) LoadTab("Misc");
        else if (TabCustomize.IsChecked == true) LoadTab("Customize");
        else if (TabAdmin.IsChecked == true) LoadTab("Admin");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchFilter = SearchBox.Text == "Search modules..." ? "" : SearchBox.Text.Trim();
        LoadTab(_currentTab);
    }

    private void LoadTab(string tab)
    {
        _currentTab = tab;
        ContentPanel.Children.Clear();

        switch (tab)
        {
            case "SPVP":
                BuildModuleTab(ModuleCategory.Spvp, "SPVP");
                break;
            case "Mace":
                BuildModuleTab(ModuleCategory.Mace, "Mace");
                break;
            case "Misc":
                BuildModuleTab(ModuleCategory.Misc, "Misc");
                BuildModuleTab(ModuleCategory.Prevent, "Prevent", append: true);
                break;
            case "Customize":
                BuildCustomizeTab();
                break;
            case "Admin":
                BuildAdminTab();
                break;
        }
    }

    private void BuildModuleTab(ModuleCategory category, string title, bool append = false)
    {
        if (!append)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = (FontFamily)FindResource("MainFont"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextBrush"),
                Margin = new Thickness(0, 0, 0, 16)
            });
        }
        else
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = (FontFamily)FindResource("MainFont"),
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextBrush"),
                Margin = new Thickness(0, 24, 0, 12)
            });
        }

        var modules = ModuleRegistry.GetByCategory(category)
            .Where(m => string.IsNullOrEmpty(_searchFilter) ||
                        m.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase));

        foreach (var module in modules)
            ContentPanel.Children.Add(CreateModuleCard(module));
    }

    private Border CreateModuleCard(ModuleDefinition module)
    {
        var card = new Border { Style = (Style)FindResource("ModuleCard") };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Icon
        var iconBorder = new Border
        {
            Width = 36, Height = 36,
            CornerRadius = new CornerRadius(8),
            Background = (Brush)FindResource("AccentDimBrush"),
            Margin = new Thickness(0, 0, 14, 0),
            VerticalAlignment = VerticalAlignment.Top
        };

        try
        {
            iconBorder.Child = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(module.IconPath)),
                Width = 24, Height = 24,
                Stretch = Stretch.Uniform
            };
        }
        catch
        {
            iconBorder.Child = new TextBlock
            {
                Text = "⚡", FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        Grid.SetColumn(iconBorder, 0);
        grid.Children.Add(iconBorder);

        // Info
        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        info.Children.Add(new TextBlock
        {
            Text = module.Name,
            FontFamily = (FontFamily)FindResource("MainFont"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("TextBrush")
        });
        info.Children.Add(new TextBlock
        {
            Text = module.Description,
            FontFamily = (FontFamily)FindResource("MainFont"),
            FontSize = 11,
            Foreground = (Brush)FindResource("TextDimBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 12, 0),
            MaxWidth = 480
        });

        if (module.Settings.Count > 0)
        {
            var settingsPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            foreach (var setting in module.Settings)
                settingsPanel.Children.Add(CreateSettingControl(module, setting));
            info.Children.Add(settingsPanel);
        }

        Grid.SetColumn(info, 1);
        grid.Children.Add(info);

        // Toggle
        var toggle = new ToggleButton
        {
            Style = (Style)FindResource("ToggleSwitch"),
            IsChecked = SettingsService.GetModuleEnabled(module.Id),
            VerticalAlignment = VerticalAlignment.Top,
            Tag = module.Id
        };
        toggle.Checked += ModuleToggle_Changed;
        toggle.Unchecked += ModuleToggle_Changed;

        Grid.SetColumn(toggle, 2);
        grid.Children.Add(toggle);

        card.Child = grid;
        return card;
    }

    private UIElement CreateSettingControl(ModuleDefinition module, ModuleSetting setting)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };

        panel.Children.Add(new TextBlock
        {
            Text = setting.Label + ":",
            Foreground = (Brush)FindResource("TextDimBrush"),
            FontSize = 11,
            Width = 100,
            VerticalAlignment = VerticalAlignment.Center
        });

        switch (setting.Type)
        {
            case SettingType.Slider:
                var slider = new Slider
                {
                    Style = (Style)FindResource("GlassSlider"),
                    Minimum = setting.Min,
                    Maximum = setting.Max,
                    Value = Convert.ToDouble(SettingsService.GetModuleSetting(module.Id, setting.Key,
                        Convert.ToDouble(setting.Value))),
                    Width = 160,
                    Tag = (module.Id, setting.Key)
                };
                var valLabel = new TextBlock
                {
                    Foreground = (Brush)FindResource("AccentBrush"),
                    FontSize = 11,
                    Width = 40,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = slider.Value.ToString("0.#")
                };
                slider.ValueChanged += (_, e) =>
                {
                    valLabel.Text = e.NewValue.ToString("0.#");
                    SettingsService.SetModuleSetting(module.Id, setting.Key, e.NewValue);
                    ModuleBridge.WriteState();
                };
                panel.Children.Add(slider);
                panel.Children.Add(valLabel);
                break;

            case SettingType.Toggle:
                var cb = new CheckBox
                {
                    IsChecked = SettingsService.GetModuleSetting(module.Id, setting.Key,
                        setting.Value is bool b && b),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = (module.Id, setting.Key)
                };
                cb.Checked += SettingToggle_Changed;
                cb.Unchecked += SettingToggle_Changed;
                panel.Children.Add(cb);
                break;

            case SettingType.Dropdown:
                var combo = new ComboBox
                {
                    Width = 140,
                    ItemsSource = setting.Options,
                    SelectedItem = SettingsService.GetModuleSetting(module.Id, setting.Key,
                        setting.Value?.ToString() ?? ""),
                    Tag = (module.Id, setting.Key)
                };
                combo.SelectionChanged += (_, _) =>
                {
                    if (combo.SelectedItem != null)
                    {
                        SettingsService.SetModuleSetting(module.Id, setting.Key, combo.SelectedItem.ToString()!);
                        ModuleBridge.WriteState();
                    }
                };
                panel.Children.Add(combo);
                break;

            case SettingType.Text:
                var tb = new TextBox
                {
                    Style = (Style)FindResource("GlassTextBox"),
                    Width = 140,
                    Padding = new Thickness(8, 4, 8, 4),
                    Text = SettingsService.GetModuleSetting(module.Id, setting.Key,
                        setting.Value?.ToString() ?? ""),
                    Tag = (module.Id, setting.Key)
                };
                tb.LostFocus += (_, _) =>
                {
                    SettingsService.SetModuleSetting(module.Id, setting.Key, tb.Text);
                    ModuleBridge.WriteState();
                };
                panel.Children.Add(tb);
                break;
        }

        return panel;
    }

    private void ModuleToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string id)
        {
            var enabled = tb.IsChecked == true;
            SettingsService.SetModuleEnabled(id, enabled);
            ModuleBridge.NotifyModuleChanged(id, enabled);
        }
    }

    private void SettingToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && cb.Tag is ValueTuple<string, string> tag)
        {
            SettingsService.SetModuleSetting(tag.Item1, tag.Item2, cb.IsChecked == true);
            ModuleBridge.WriteState();
        }
    }

    private void BuildCustomizeTab()
    {
        ContentPanel.Children.Add(Header("Customize"));

        // Stream Proof
        ContentPanel.Children.Add(CreateCustomizeToggle(
            "Stream Proof",
            "Hides the Eclipse GUI from OBS, Discord, and recording software.",
            SettingsService.Current.StreamProof,
            v => { SettingsService.Current.StreamProof = v; SettingsService.Save(); ModuleBridge.WriteState(); }));

        ContentPanel.Children.Add(SectionHeader("Performance"));

        ContentPanel.Children.Add(CreateSliderSetting("UI Refresh Rate (Hz)", 60, 360,
            SettingsService.Current.UiRefreshHz, v =>
            {
                SettingsService.Current.UiRefreshHz = (int)v;
                SettingsService.Save();
                _uiTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / v);
            }));

        ContentPanel.Children.Add(CreateSliderSetting("Blur Refresh Rate (Hz)", 5, 60,
            SettingsService.Current.BlurRefreshHz, v =>
            {
                SettingsService.Current.BlurRefreshHz = (int)v;
                SettingsService.Save();
            }));

        ContentPanel.Children.Add(SectionHeader("Appearance"));

        ContentPanel.Children.Add(CreateCustomizeToggle(
            "Light Theme",
            "Switch between dark glass and light glass appearance.",
            SettingsService.Current.LightTheme,
            v =>
            {
                SettingsService.Current.LightTheme = v;
                SettingsService.Save();
                ThemeManager.ApplyTheme(v);
                WindowHelper.EnableGlass(this);
            }));

        ContentPanel.Children.Add(SectionHeader("Accent Color"));

        var colorPanel = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        foreach (var color in ThemeManager.AccentColors.Keys)
        {
            var btn = new Button
            {
                Content = color,
                Tag = color,
                Margin = new Thickness(0, 0, 8, 8),
                Padding = new Thickness(14, 8, 14, 8),
                Style = (Style)FindResource("GlassButton"),
                FontSize = 11
            };
            if (color == SettingsService.Current.AccentColor)
                btn.BorderBrush = (Brush)FindResource("AccentBrush");

            btn.Click += (_, _) =>
            {
                SettingsService.Current.AccentColor = color;
                SettingsService.Save();
                ThemeManager.ApplyAccent(color);
                LoadTab("Customize");
            };
            colorPanel.Children.Add(btn);
        }
        ContentPanel.Children.Add(colorPanel);
    }

    private void BuildAdminTab()
    {
        ContentPanel.Children.Add(Header("Admin Panel"));

        // Add user form
        var form = new Border
        {
            Style = (Style)FindResource("ModuleCard"),
            Margin = new Thickness(0, 0, 0, 16)
        };
        var formStack = new StackPanel();

        formStack.Children.Add(SectionHeader("Add User"));
        var emailBox = new TextBox { Style = (Style)FindResource("GlassTextBox"), Margin = new Thickness(0, 0, 0, 8) };
        emailBox.SetValue(TextBox.TagProperty, "placeholder");
        var passBox = new PasswordBox { Style = (Style)FindResource("GlassPasswordBox"), Margin = new Thickness(0, 0, 0, 8) };
        var permCheck = new CheckBox { Content = "Permanent Access", Foreground = (Brush)FindResource("TextDimBrush"), Margin = new Thickness(0, 0, 0, 8) };
        var expiryPicker = new DatePicker { Margin = new Thickness(0, 0, 0, 8) };
        var addError = new TextBlock { Foreground = (Brush)FindResource("DangerBrush"), FontSize = 11, Visibility = Visibility.Collapsed };

        permCheck.Checked += (_, _) => expiryPicker.IsEnabled = false;
        permCheck.Unchecked += (_, _) => expiryPicker.IsEnabled = true;

        var addBtn = new Button { Content = "Add User", Style = (Style)FindResource("AccentButton"), HorizontalAlignment = HorizontalAlignment.Left };
        addBtn.Click += (_, _) =>
        {
            addError.Visibility = Visibility.Collapsed;
            if (!AuthService.AddUser(emailBox.Text, passBox.Password, permCheck.IsChecked == true,
                    expiryPicker.SelectedDate, out var err))
            {
                addError.Text = err;
                addError.Visibility = Visibility.Visible;
                return;
            }
            emailBox.Clear();
            passBox.Clear();
            LoadTab("Admin");
        };

        formStack.Children.Add(new TextBlock { Text = "Email", Foreground = (Brush)FindResource("TextDimBrush"), FontSize = 11, Margin = new Thickness(0, 0, 0, 4) });
        formStack.Children.Add(emailBox);
        formStack.Children.Add(new TextBlock { Text = "Password", Foreground = (Brush)FindResource("TextDimBrush"), FontSize = 11, Margin = new Thickness(0, 0, 0, 4) });
        formStack.Children.Add(passBox);
        formStack.Children.Add(permCheck);
        formStack.Children.Add(expiryPicker);
        formStack.Children.Add(addError);
        formStack.Children.Add(addBtn);
        form.Children = formStack;
        ContentPanel.Children.Add(form);

        // User list
        ContentPanel.Children.Add(SectionHeader("Users"));
        foreach (var user in AuthService.GetAllUsers())
        {
            var row = new Border { Style = (Style)FindResource("ModuleCard") };
            var rowGrid = new Grid();
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = user.Email + (user.IsAdmin ? " (Admin)" : ""),
                Foreground = (Brush)FindResource("TextBrush"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 13
            });

            var access = user.IsPermanent ? "Permanent" :
                user.ExpiryDate.HasValue ? $"Expires: {user.ExpiryDate.Value:yyyy-MM-dd}" :
                "No expiry set";
            if (user.IsExpired) access += " [EXPIRED]";

            info.Children.Add(new TextBlock
            {
                Text = access,
                Foreground = (Brush)FindResource("TextDimBrush"),
                FontSize = 11
            });

            Grid.SetColumn(info, 0);
            rowGrid.Children.Add(info);

            if (!user.IsAdmin)
            {
                var removeBtn = new Button
                {
                    Content = "Remove",
                    Style = (Style)FindResource("GlassButton"),
                    Tag = user.Email,
                    Padding = new Thickness(12, 6, 12, 6),
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                };
                removeBtn.Click += (_, _) =>
                {
                    AuthService.RemoveUser(user.Email);
                    LoadTab("Admin");
                };
                Grid.SetColumn(removeBtn, 1);
                rowGrid.Children.Add(removeBtn);
            }

            row.Child = rowGrid;
            ContentPanel.Children.Add(row);
        }
    }

    private static TextBlock Header(string text) => new()
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 16)
    };

    private static TextBlock SectionHeader(string text) => new()
    {
        Text = text,
        FontSize = 14,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 16, 0, 8)
    };

    private Border CreateCustomizeToggle(string title, string desc, bool value, Action<bool> onChanged)
    {
        var card = new Border { Style = (Style)FindResource("ModuleCard") };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var info = new StackPanel();
        info.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 14 });
        info.Children.Add(new TextBlock { Text = desc, FontSize = 11, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 12, 0) });
        Grid.SetColumn(info, 0);
        grid.Children.Add(info);

        var toggle = new ToggleButton { Style = (Style)FindResource("ToggleSwitch"), IsChecked = value, VerticalAlignment = VerticalAlignment.Center };
        toggle.Checked += (_, _) => onChanged(true);
        toggle.Unchecked += (_, _) => onChanged(false);
        Grid.SetColumn(toggle, 1);
        grid.Children.Add(toggle);

        card.Child = grid;
        return card;
    }

    private Border CreateSliderSetting(string label, double min, double max, double value, Action<double> onChanged)
    {
        var card = new Border { Style = (Style)FindResource("ModuleCard") };
        var panel = new StackPanel();

        var header = new Grid();
        header.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, FontSize = 13 });
        var valText = new TextBlock { Text = value.ToString("0"), HorizontalAlignment = HorizontalAlignment.Right, Foreground = (Brush)FindResource("AccentBrush") };
        header.Children.Add(valText);
        panel.Children.Add(header);

        var slider = new Slider { Minimum = min, Maximum = max, Value = value, Style = (Style)FindResource("GlassSlider"), Margin = new Thickness(0, 8, 0, 0) };
        slider.ValueChanged += (_, e) => { valText.Text = e.NewValue.ToString("0"); onChanged(e.NewValue); };
        panel.Children.Add(slider);

        card.Child = panel;
        return card;
    }

    private void Inject_Click(object sender, RoutedEventArgs e) => InjectionService.Inject();

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AuthService.Logout();
        SessionService.ClearSession();
        new LoginWindow().Show();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _uiTimer.Stop();
        _monitor.Dispose();
        base.OnClosed(e);
    }
}
