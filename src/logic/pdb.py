import gi
gi.require_version("Gtk", "4.0")
from gi.repository import Gtk, Gio, Gdk, GdkPixbuf
import webbrowser
import os

# Version Configuration
APP_VERSION = "DEV"  # Can be "DEV", "BETA", or "RELEASE"

# Icon paths
ICON_PATHS = {
    "DEV": "ui/assets/bridge-dev.png",
    "BETA": "ui/assets/bridge-beta.png",
    "RELEASE": "ui/assets/bridge.png",
}

class PdbApp(Gtk.Application):
    def __init__(self):
        super().__init__(application_id="org.proton.drive.bridge",
                        flags=Gio.ApplicationFlags.FLAGS_NONE)
        
        # Set application icon
        version_icon_path = os.path.join(ICON_PATHS[APP_VERSION])
        try:
            icon = Gtk.IconTheme.get_default()
            icon.add_search_path("ui/assets")
            self.props.application_icon = version_icon_path
        except:
            print(f"Could not set application icon: {version_icon_path}")
        
    def do_activate(self):
        # Initialize the GTK Builder
        self.builder = Gtk.Builder()
        self.builder.add_from_file("ui/pdb.ui")

        # Load CSS
        css_provider = Gtk.CssProvider()
        css_provider.load_from_path("ui/style.css")
        Gtk.StyleContext.add_provider_for_display(
            Gdk.Display.get_default(),
            css_provider,
            Gtk.STYLE_PROVIDER_PRIORITY_APPLICATION
        )

        # Get the main window from the .ui file
        self.window = self.builder.get_object("main_window")
        if not self.window:
            raise RuntimeError("main_window not found in UI file!")
            
        # Set the appropriate icons
        self.settings_icon = self.builder.get_object("settings_icon")
        
        # Load and set the version-specific icon for the header button
        version_icon_path = os.path.join(ICON_PATHS[APP_VERSION])
        icon_pixbuf = GdkPixbuf.Pixbuf.new_from_file(version_icon_path)

        # Scale the icon to fill the button (32x32)
        scaled_pixbuf = icon_pixbuf.scale_simple(32, 32, GdkPixbuf.InterpType.BILINEAR)
        self.settings_icon.set_from_pixbuf(scaled_pixbuf)
        
        # Get the stack and buttons
        self.stack = self.builder.get_object("main_stack")
        self.page1_button = self.builder.get_object("page1_button")
        self.page2_button = self.builder.get_object("page2_button")
        self.url_button1 = self.builder.get_object("url_button1")
        self.url_button2 = self.builder.get_object("url_button2")
        
        # Connect signals
        self.page1_button.connect("toggled", self.on_page_button_toggled, "page1")
        self.page2_button.connect("toggled", self.on_page_button_toggled, "page2")
        self.url_button1.connect("clicked", self.on_url_button_clicked, "https://atomix.one")
        self.url_button2.connect("clicked", self.on_url_button_clicked, "https://atomix.one")
        
        # Get the icon button
        self.icon_button = self.builder.get_object("icon_button")
        
        # Connect icon button signal
        self.icon_button.connect("clicked", self.on_url_button_clicked, "https://atomix.one")
            
        # Set the application for the window
        self.window.set_application(self)
        
        # Show the window
        self.window.present()
        
    def on_page_button_toggled(self, button, page_name):
        if button.get_active():  # Only respond when button is being activated
            # Switch to the selected page
            self.stack.set_visible_child_name(page_name)
            
            # Untoggle the other button
            if button == self.page1_button:
                self.page2_button.set_active(False)
            else:
                self.page1_button.set_active(False)
        
    def on_url_button_clicked(self, button, url):
        webbrowser.open(url)

if __name__ == "__main__":
    app = PdbApp()
    app.run(None)
