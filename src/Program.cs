using Gtk;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ProtonDriveBridge
{
    class Program
    {
        public static void Main()
        {
            new MainWindow();
            Application.Run();
        }
    }

    class MainWindow
    {
        private Window window;
        private Entry sourceEntry;
        private Entry targetEntry;
        private Button startButton;
        private Label statusLabel;
        private bool isDarkMode = true;
        private CssProvider cssProvider;
        private TextView debugTextView;
        private ScrolledWindow debugScrolled;
        private bool isDebugVisible = false;
        private Box mainContentBox;  // To hold everything except debug
        private Box rootBox;         // To hold both main content and debug

        public MainWindow()
        {
            Application.Init();

            // Create main window with modern styling
            window = new Window("ProtonDriveBridge");
            window.SetDefaultSize(700, 300);
            window.DeleteEvent += (s, e) => Application.Quit();
            window.WindowPosition = WindowPosition.Center;

            // Add CSS styling
            cssProvider = new CssProvider();
            ApplyTheme(isDarkMode);
            
            // Add this line to make the CSS take effect
            StyleContext.AddProviderForScreen(
                Gdk.Screen.Default,
                cssProvider,
                StyleProviderPriority.Application
            );

            // Create root box to hold everything
            rootBox = new Box(Orientation.Vertical, 0);
            window.Add(rootBox);

            // Main content box (everything except debug)
            mainContentBox = new Box(Orientation.Vertical, 0);
            mainContentBox.MarginStart = 24;
            mainContentBox.MarginEnd = 24;
            mainContentBox.MarginTop = 24;
            mainContentBox.MarginBottom = 24;
            rootBox.PackStart(mainContentBox, true, true, 0);

            // Debug area (initially hidden)
            debugScrolled = new ScrolledWindow();
            debugScrolled.SetSizeRequest(-1, 200);  // Height of 200px
            debugScrolled.Visible = false;
            
            debugTextView = new TextView();
            debugTextView.Editable = false;
            debugTextView.WrapMode = WrapMode.Word;
            debugTextView.Buffer.Text = "Debug Output:\n";
            
            // Style the debug area
            debugTextView.StyleContext.AddClass("debug-view");
            
            debugScrolled.Add(debugTextView);
            rootBox.PackEnd(debugScrolled, false, true, 0);

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
            mainContentBox.PackStart(headerBox, false, false, 10);

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
            mainContentBox.PackStart(sourceBox, false, false, 10);

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
            mainContentBox.PackStart(targetBox, false, false, 10);

            // Status label with modern styling
            statusLabel = new Label("Ready to sync");
            statusLabel.StyleContext.AddClass("status-label");
            statusLabel.MarginTop = 20;
            statusLabel.MarginBottom = 20;
            mainContentBox.PackStart(statusLabel, false, false, 0);

            // Start button with modern styling
            startButton = new Button("Start Synchronization");
            startButton.Halign = Align.Center;
            startButton.MarginTop = 10;
            startButton.Clicked += StartSync;
            mainContentBox.PackStart(startButton, false, false, 0);

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
            LogDebug("\n=== File Synchronization Started ===");
            LogDebug($"Source: {sourceDir}");
            LogDebug($"Target: {targetDir}\n");

            var sourceFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            LogDebug($"Found {sourceFiles.Length} files in source directory\n");

            foreach (var sourceFile in sourceFiles)
            {
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                string targetFile = Path.Combine(targetDir, relativePath);
                
                LogDebug($"Processing: {relativePath}");
                LogDebug($"Source: {sourceFile}");
                LogDebug($"Target: {targetFile}");

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
                    
                    LogDebug($"Source Hash: {sourceHash}");
                    LogDebug($"Target Hash: {targetHash}");

                    if (sourceHash != targetHash)
                    {
                        shouldCopy = true;
                        reason = "Modified file";
                    }
                }

                if (shouldCopy)
                {
                    LogDebug($"Action Required: {reason}");
                    LogDebug("Creating directory structure if needed...");
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                    
                    LogDebug("Copying file...");
                    File.Copy(sourceFile, targetFile, true);
                    LogDebug("Copy completed successfully!");
                }
                else
                {
                    LogDebug("Action Required: None (files are identical)");
                }
                
                LogDebug("\n" + new string('-', 80) + "\n");
            }

            LogDebug("=== File Synchronization Completed ===");
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
                .debug-view {
                    font-family: monospace;
                    background-color: " + (isDark ? "#2d2d2d" : "#f6f6f6") + @";
                    color: " + (isDark ? "#ffffff" : "#000000") + @";
                    padding: 8px;
                }
                .debug-view text {
                    background-color: " + (isDark ? "#2d2d2d" : "#f6f6f6") + @";
                    color: " + (isDark ? "#ffffff" : "#000000") + @";
                }
                scrolledwindow {
                    border-top: 1px solid " + (isDark ? "#404040" : "#deddda") + @";
                }
            ";

            cssProvider.LoadFromData(themeCSS);
        }

        private void LogDebug(string message)
        {
            if (!debugScrolled.Visible)
            {
                debugScrolled.Visible = true;
            }

            Application.Invoke((s, e) =>
            {
                var buffer = debugTextView.Buffer;
                buffer.Insert(buffer.EndIter, message + "\n");
                
                // Auto-scroll to bottom
                var mark = buffer.CreateMark(null, buffer.EndIter, false);
                debugTextView.ScrollToMark(mark, 0, false, 0, 0);
            });
        }
    }
} 