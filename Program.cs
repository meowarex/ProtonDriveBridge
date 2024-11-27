using Gtk;
using System;
using System.IO;
using System.Security.Cryptography;

class FileSyncApp
{
    private Window window;
    private Entry sourceEntry;
    private Entry targetEntry;
    private Button startButton;
    private Label statusLabel;
    private bool isDarkMode = true;
    private CssProvider cssProvider;

    public FileSyncApp()
    {
        Application.Init();

        // Create main window with modern styling
        window = new Window("ProtonDriveBridge");
        window.SetDefaultSize(700, 300);
        window.DeleteEvent += (s, e) => Application.Quit();
        window.WindowPosition = WindowPosition.Center;

        // Try to set the window icon
        try
        {
            window.Icon = new Gdk.Pixbuf("Assets/pdb.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load icon: {ex.Message}");
        }

        // Add CSS styling
        cssProvider = new CssProvider();
        ApplyTheme(isDarkMode);
        
        StyleContext.AddProviderForScreen(
            Gdk.Screen.Default,
            cssProvider,
            StyleProviderPriority.Application
        );

        // Main container with padding
        var mainBox = new Box(Orientation.Vertical, 0);
        mainBox.MarginStart = 24;
        mainBox.MarginEnd = 24;
        mainBox.MarginTop = 24;
        mainBox.MarginBottom = 24;
        window.Add(mainBox);

        // Header with title, subtitle, and theme toggle
        var headerBox = new Box(Orientation.Horizontal, 5);
        
        var titleBox = new Box(Orientation.Vertical, 5);
        var titleLabel = new Label();
        titleLabel.Markup = "<span size='x-large' weight='bold'>ProtonDriveBridge</span>";
        var subtitleLabel = new Label("Bridge your files between local folders with detailed progress tracking");
        subtitleLabel.StyleContext.AddClass("dim-label");
        titleBox.PackStart(titleLabel, false, false, 0);
        titleBox.PackStart(subtitleLabel, false, false, 0);
        
        // Theme toggle button
        var themeButton = new Button();
        themeButton.TooltipText = "Toggle Theme";
        themeButton.StyleContext.AddClass("theme-toggle");
        themeButton.Label = isDarkMode ? "🌙" : "☀️";
        themeButton.Clicked += (s, e) => {
            isDarkMode = !isDarkMode;
            themeButton.Label = isDarkMode ? "🌙" : "☀️";
            ApplyTheme(isDarkMode);
        };

        headerBox.PackStart(titleBox, true, true, 0);
        headerBox.PackEnd(themeButton, false, false, 0);
        mainBox.PackStart(headerBox, false, false, 10);

        // Rest of the UI setup...
        // [Previous UI code remains the same until the styling changes]

        window.ShowAll();
    }

    private void ApplyTheme(bool isDark)
    {
        var themeCSS = @"
            window {
                background-color: " + (isDark ? "#1e1e1e" : "#ffffff") + @";
                color: " + (isDark ? "#ffffff" : "#000000") + @";
            }
            button {
                padding: 8px 16px;
                border-radius: 8px;
                background: " + (isDark ? "linear-gradient(to bottom, #ff69b4, #da1884)" : "linear-gradient(to bottom, #3584e4, #1c71d8)") + @";
                color: white;
                border: none;
                margin: 5px;
            }
            button:hover {
                background: " + (isDark ? "linear-gradient(to bottom, #ff69b4, #c71585)" : "linear-gradient(to bottom, #3584e4, #1a5fb4)") + @";
            }
            button.theme-toggle {
                padding: 4px 8px;
                background: transparent;
                border: 1px solid " + (isDark ? "#ffffff" : "#000000") + @";
                color: " + (isDark ? "#ffffff" : "#000000") + @";
            }
            button.theme-toggle:hover {
                background: " + (isDark ? "rgba(255,255,255,0.1)" : "rgba(0,0,0,0.1)") + @";
            }
            entry {
                padding: 8px;
                border-radius: 6px;
                border: 1px solid " + (isDark ? "#404040" : "#deddda") + @";
                background-color: " + (isDark ? "#2d2d2d" : "#ffffff") + @";
                color: " + (isDark ? "#ffffff" : "#000000") + @";
                margin: 5px;
            }
            label {
                margin: 5px;
                color: " + (isDark ? "#ffffff" : "#000000") + @";
            }
            .dim-label {
                opacity: 0.8;
            }
        ";

        cssProvider.LoadFromData(themeCSS);
    }

    // Rest of the code remains the same...
} 