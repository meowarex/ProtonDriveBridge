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

    public FileSyncApp()
    {
        Application.Init();

        // Create main window with modern styling
        window = new Window("ProtonDriveBridge");
        window.SetDefaultSize(700, 300);
        window.DeleteEvent += (s, e) => Application.Quit();
        window.WindowPosition = WindowPosition.Center;

        // Add CSS styling
        var cssProvider = new CssProvider();
        cssProvider.LoadFromData(@"
            window {
                background-color: #ffffff;
            }
            button {
                padding: 8px 16px;
                border-radius: 8px;
                background: linear-gradient(to bottom, #3584e4, #1c71d8);
                color: white;
                border: none;
                margin: 5px;
            }
            button:hover {
                background: linear-gradient(to bottom, #3584e4, #1a5fb4);
            }
            entry {
                padding: 8px;
                border-radius: 6px;
                border: 1px solid #deddda;
                margin: 5px;
            }
            label {
                margin: 5px;
            }
        ");
        
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

        // Header with title and subtitle
        var headerBox = new Box(Orientation.Vertical, 5);
        var titleLabel = new Label();
        titleLabel.Markup = "<span size='x-large' weight='bold'>ProtonDriveBridge</span>";
        var subtitleLabel = new Label("Bridge your files between local folders with detailed progress tracking");
        subtitleLabel.StyleContext.AddClass("dim-label");
        headerBox.PackStart(titleLabel, false, false, 0);
        headerBox.PackStart(subtitleLabel, false, false, 0);
        mainBox.PackStart(headerBox, false, false, 10);

        // Source folder selection
        var sourceBox = new Box(Orientation.Horizontal, 10);
        var sourceLabel = new Label("Source Folder:");
        sourceLabel.Halign = Align.Start;
        sourceEntry = new Entry();
        sourceEntry.Hexpand = true;
        var sourceButton = new Button("Browse");
        sourceButton.Clicked += (s, e) => BrowseFolder(sourceEntry);
        sourceBox.PackStart(sourceLabel, false, false, 0);
        sourceBox.PackStart(sourceEntry, true, true, 0);
        sourceBox.PackStart(sourceButton, false, false, 0);
        mainBox.PackStart(sourceBox, false, false, 10);

        // Target folder selection
        var targetBox = new Box(Orientation.Horizontal, 10);
        var targetLabel = new Label("Target Folder:");
        targetLabel.Halign = Align.Start;
        targetEntry = new Entry();
        targetEntry.Hexpand = true;
        var targetButton = new Button("Browse");
        targetButton.Clicked += (s, e) => BrowseFolder(targetEntry);
        targetBox.PackStart(targetLabel, false, false, 0);
        targetBox.PackStart(targetEntry, true, true, 0);
        targetBox.PackStart(targetButton, false, false, 0);
        mainBox.PackStart(targetBox, false, false, 10);

        // Status label with modern styling
        statusLabel = new Label("Ready to sync");
        statusLabel.StyleContext.AddClass("status-label");
        statusLabel.MarginTop = 20;
        statusLabel.MarginBottom = 20;
        mainBox.PackStart(statusLabel, false, false, 0);

        // Start button with modern styling
        startButton = new Button("Start Synchronization");
        startButton.Halign = Align.Center;
        startButton.MarginTop = 10;
        startButton.Clicked += StartSync;
        mainBox.PackStart(startButton, false, false, 0);

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

    public static void Main()
    {
        new FileSyncApp();
        Application.Run();
    }
} 