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

        // Source folder selection
        var sourceBox = new Box(Orientation.Horizontal, 10);
        sourceBox.PackStart(new Label("Source Folder:"), false, false, 5);
        sourceEntry = new Entry();
        sourceBox.PackStart(sourceEntry, true, true, 5);
        var sourceButton = new Button("Browse");
        sourceButton.Clicked += (s, e) => BrowseFolder(sourceEntry);
        sourceBox.PackStart(sourceButton, false, false, 5);
        mainBox.PackStart(sourceBox, false, false, 5);

        // Target folder selection
        var targetBox = new Box(Orientation.Horizontal, 10);
        targetBox.PackStart(new Label("Target Folder:"), false, false, 5);
        targetEntry = new Entry();
        targetBox.PackStart(targetEntry, true, true, 5);
        var targetButton = new Button("Browse");
        targetButton.Clicked += (s, e) => BrowseFolder(targetEntry);
        targetBox.PackStart(targetButton, false, false, 5);
        mainBox.PackStart(targetBox, false, false, 5);

        // Status label
        statusLabel = new Label("Ready to sync");
        mainBox.PackStart(statusLabel, false, false, 5);

        // Start button
        startButton = new Button("Start Synchronization");
        startButton.Clicked += StartSync;
        mainBox.PackStart(startButton, false, false, 5);

        window.ShowAll();
    }

    private void BrowseFolder(Entry entry)
    {
        var dialog = new FileChooserDialog(
            "Select folder",
            window,
            FileChooserAction.SelectFolder,
            "Cancel", ResponseType.Cancel,
            "Select", ResponseType.Accept);

        if (dialog.Run() == (int)ResponseType.Accept)
        {
            entry.Text = dialog.Filename;
        }
        dialog.Destroy();
    }

    private void StartSync(object sender, EventArgs e)
    {
        string sourceDir = sourceEntry.Text;
        string targetDir = targetEntry.Text;

        if (string.IsNullOrEmpty(sourceDir) || string.IsNullOrEmpty(targetDir))
        {
            statusLabel.Text = "Please select both source and target folders!";
            return;
        }

        if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir))
        {
            statusLabel.Text = "Invalid directory path!";
            return;
        }

        startButton.Sensitive = false;
        statusLabel.Text = "Synchronizing...";

        // Start sync in a new thread to keep UI responsive
        new System.Threading.Thread(() =>
        {
            try
            {
                SyncFolders(sourceDir, targetDir);
                Application.Invoke((s, e) =>
                {
                    statusLabel.Text = "Synchronization completed!";
                    startButton.Sensitive = true;
                });
            }
            catch (Exception ex)
            {
                Application.Invoke((s, e) =>
                {
                    statusLabel.Text = $"Error: {ex.Message}";
                    startButton.Sensitive = true;
                });
            }
        }).Start();
    }

    private void SyncFolders(string sourceDir, string targetDir)
    {
        Console.WriteLine("\n=== File Synchronization Started ===");
        Console.WriteLine($"Source: {sourceDir}");
        Console.WriteLine($"Target: {targetDir}\n");

        var sourceFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
        Console.WriteLine($"Found {sourceFiles.Length} files in source directory\n");

        foreach (var sourceFile in sourceFiles)
        {
            string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            string targetFile = Path.Combine(targetDir, relativePath);
            
            Console.WriteLine($"Processing: {relativePath}");
            Console.WriteLine($"Source: {sourceFile}");
            Console.WriteLine($"Target: {targetFile}");

            bool shouldCopy = false;
            string reason = string.Empty;

            if (!File.Exists(targetFile))
            {
                shouldCopy = true;
                reason = "New file";
            }
            else
            {
                var sourceHash = CalculateFileHash(sourceFile);
                var targetHash = CalculateFileHash(targetFile);
                
                Console.WriteLine($"Source Hash: {sourceHash}");
                Console.WriteLine($"Target Hash: {targetHash}");

                if (sourceHash != targetHash)
                {
                    shouldCopy = true;
                    reason = "Modified file";
                }
            }

            if (shouldCopy)
            {
                Console.WriteLine($"Action Required: {reason}");
                Console.WriteLine("Creating directory structure if needed...");
                
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                
                Console.WriteLine("Copying file...");
                File.Copy(sourceFile, targetFile, true);
                Console.WriteLine("Copy completed successfully!");
            }
            else
            {
                Console.WriteLine("Action Required: None (files are identical)");
            }
            
            Console.WriteLine("\n" + new string('-', 80) + "\n");
        }

        Console.WriteLine("=== File Synchronization Completed ===");
    }

    private string CalculateFileHash(string filename)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filename))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
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

    public static void Main()
    {
        new FileSyncApp();
        Application.Run();
    }
} 