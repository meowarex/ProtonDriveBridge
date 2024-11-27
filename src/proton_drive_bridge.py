import gi
gi.require_version('Gtk', '4.0')  # or '3.0' if you prefer GTK3
from gi.repository import Gtk, Gdk, Gio, GLib
import os
import hashlib
from pathlib import Path
import threading

@Gtk.Template(filename='ui/main_window.ui')
class MainWindow(Gtk.ApplicationWindow):
    __gtype_name__ = 'MainWindow'

    # Template widgets
    source_entry = Gtk.Template.Child()
    target_entry = Gtk.Template.Child()
    source_button = Gtk.Template.Child()
    target_button = Gtk.Template.Child()
    start_button = Gtk.Template.Child()
    status_label = Gtk.Template.Child()
    debug_view = Gtk.Template.Child()
    debug_scroll = Gtk.Template.Child()

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.setup_style()
        self.setup_actions()
        
        # Set default theme
        settings = Gtk.Settings.get_default()
        settings.set_property("gtk-application-prefer-dark-theme", True)

    def setup_style(self):
        css_provider = Gtk.CssProvider()
        css_provider.load_from_data('''
            @define-color accent_color #ff69b4;
            @define-color accent_bg_color #da1884;
            @define-color accent_hover #c71585;

            .heading {
                font-weight: bold;
                font-size: 15px;
            }
            .caption {
                font-size: 13px;
            }
            .monospace {
                font-family: monospace;
            }
            .card {
                background: alpha(currentColor, 0.05);
                border-radius: 12px;
                padding: 12px;
            }
            .boxed-list {
                background-color: alpha(currentColor, 0.05);
                border-radius: 12px;
            }
            .pill {
                border-radius: 9999px;
                padding: 12px 32px;
            }
            button.suggested-action {
                background: linear-gradient(to bottom, @accent_color, @accent_bg_color);
                color: white;
            }
            button.suggested-action:hover {
                background: linear-gradient(to bottom, @accent_color, @accent_hover);
            }
            button.flat:hover {
                background-color: alpha(@accent_color, 0.1);
            }
            textview {
                background: none;
            }
            textview text {
                background: none;
            }
            textview text selection {
                background-color: alpha(@accent_color, 0.3);
                color: inherit;
            }
            entry selection {
                background-color: alpha(@accent_color, 0.3);
                color: inherit;
            }
            entry:focus {
                border-color: @accent_color;
            }
            *:selected {
                background-color: alpha(@accent_color, 0.3);
            }
            .view:selected {
                background-color: alpha(@accent_color, 0.3);
            }
            scrollbar slider {
                background-color: alpha(@accent_color, 0.5);
                border-radius: 9999px;
            }
            scrollbar slider:hover {
                background-color: alpha(@accent_color, 0.7);
            }
        '''.encode())
        
        Gtk.StyleContext.add_provider_for_display(
            Gdk.Display.get_default(),
            css_provider,
            Gtk.STYLE_PROVIDER_PRIORITY_APPLICATION
        )

    def setup_actions(self):
        # Browse button actions
        self.source_button.connect("clicked", self.on_browse_source)
        self.target_button.connect("clicked", self.on_browse_target)
        
        # Start sync action
        self.start_button.connect("clicked", self.on_start_sync)

    def on_browse_source(self, button):
        self.browse_folder(self.source_entry)

    def on_browse_target(self, button):
        self.browse_folder(self.target_entry)

    def browse_folder(self, entry):
        dialog = Gtk.FileChooserDialog(
            title="Select Folder",
            parent=self,
            action=Gtk.FileChooserAction.SELECT_FOLDER,
        )
        dialog.add_buttons(
            "_Cancel",
            Gtk.ResponseType.CANCEL,
            "_Select",
            Gtk.ResponseType.ACCEPT,
        )
        dialog.connect("response", self.on_folder_chosen, entry)
        dialog.show()

    def on_folder_chosen(self, dialog, response, entry):
        if response == Gtk.ResponseType.ACCEPT:
            entry.set_text(dialog.get_file().get_path())
        dialog.destroy()

    def on_start_sync(self, button):
        source_dir = self.source_entry.get_text()
        target_dir = self.target_entry.get_text()

        if not source_dir or not target_dir:
            self.status_label.set_text("Please select both source and target folders!")
            return

        if not os.path.exists(source_dir) or not os.path.exists(target_dir):
            self.status_label.set_text("Invalid directory path!")
            return

        self.start_button.set_sensitive(False)
        self.status_label.set_text("Synchronizing...")
        self.debug_scroll.set_visible(True)

        thread = threading.Thread(target=self.sync_folders, args=(source_dir, target_dir))
        thread.daemon = True
        thread.start()

    def log_debug(self, message):
        GLib.idle_add(self._append_debug_text, message)

    def _append_debug_text(self, message):
        buffer = self.debug_view.get_buffer()
        buffer.insert(buffer.get_end_iter(), message + "\n")
        mark = buffer.create_mark(None, buffer.get_end_iter(), False)
        self.debug_view.scroll_mark_onscreen(mark)

    def sync_folders(self, source_dir, target_dir):
        self.log_debug("\n=== File Synchronization Started ===")
        self.log_debug(f"Source: {source_dir}")
        self.log_debug(f"Target: {target_dir}\n")

        source_files = list(Path(source_dir).rglob("*"))
        self.log_debug(f"Found {len(source_files)} files in source directory\n")

        for source_file in source_files:
            if source_file.is_file():
                rel_path = source_file.relative_to(source_dir)
                target_file = Path(target_dir) / rel_path

                self.log_debug(f"Processing: {rel_path}")
                self.log_debug(f"Source: {source_file}")
                self.log_debug(f"Target: {target_file}")

                should_copy = False
                reason = ""

                if not target_file.exists():
                    should_copy = True
                    reason = "New file"
                else:
                    source_hash = self.calculate_hash(source_file)
                    target_hash = self.calculate_hash(target_file)
                    
                    self.log_debug(f"Source Hash: {source_hash}")
                    self.log_debug(f"Target Hash: {target_hash}")

                    if source_hash != target_hash:
                        should_copy = True
                        reason = "Modified file"

                if should_copy:
                    self.log_debug(f"Action Required: {reason}")
                    self.log_debug("Creating directory structure if needed...")
                    
                    target_file.parent.mkdir(parents=True, exist_ok=True)
                    
                    self.log_debug("Copying file...")
                    target_file.write_bytes(source_file.read_bytes())
                    self.log_debug("Copy completed successfully!")
                else:
                    self.log_debug("Action Required: None (files are identical)")
                
                self.log_debug("\n" + "-" * 80 + "\n")

        self.log_debug("=== File Synchronization Completed ===")
        GLib.idle_add(self.on_sync_complete)

    def on_sync_complete(self):
        self.status_label.set_text("Synchronization completed!")
        self.start_button.set_sensitive(True)

    @staticmethod
    def calculate_hash(file_path):
        hash_md5 = hashlib.md5()
        with open(file_path, "rb") as f:
            for chunk in iter(lambda: f.read(4096), b""):
                hash_md5.update(chunk)
        return hash_md5.hexdigest()

class Application(Gtk.Application):
    def __init__(self):
        super().__init__(application_id='org.proton.drivebridge',
                        flags=Gio.ApplicationFlags.FLAGS_NONE)

    def do_activate(self):
        win = self.props.active_window
        if not win:
            win = MainWindow(application=self)
        win.present()

def main():
    app = Application()
    return app.run(None)

if __name__ == '__main__':
    main() 