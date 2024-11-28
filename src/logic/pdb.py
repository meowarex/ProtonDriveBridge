import gi
gi.require_version("Gtk", "4.0")
from gi.repository import Gtk, Gio, Gdk, GdkPixbuf
import webbrowser
import os
import glob
from PIL import Image
import io

# Version Configuration
APP_VERSION = "DEV"  # Can be "DEV", "BETA", or "RELEASE"

def get_resource_path(relative_path):
    """Get absolute path to resource, works for dev and for AppImage"""
    if os.environ.get('APPIMAGE'):
        # Running from AppImage
        appdir = os.environ.get('APPDIR', '')
        if not appdir:
            mount_dirs = glob.glob('/tmp/.mount_Proton*')
            if mount_dirs:
                appdir = mount_dirs[0]
            else:
                return relative_path
        return os.path.join(appdir, 'usr/share', relative_path)
    else:
        # Running from development/build environment
        base_path = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
        return os.path.join(base_path, relative_path)

def get_icon_path(version):
    """Get the correct icon path based on environment"""
    icon_name = f"bridge-{version.lower()}.png"
    if os.environ.get('APPIMAGE'):
        return get_resource_path(f"pdb/assets/{icon_name}")
    else:
        return get_resource_path(f"ui/assets/{icon_name}")

def load_pixbuf_safely(path, size=None):
    """Load a pixbuf with error handling and optional sizing using PIL"""
    try:
        if not os.path.isfile(path):
            print(f"Warning: Icon file not found: {path}")
            print(f"Current directory: {os.getcwd()}")
            print(f"Directory contents: {os.listdir(os.path.dirname(path))}")
            return None
            
        # Load image with PIL
        with Image.open(path) as img:
            if size:
                img = img.resize((size, size), Image.Resampling.LANCZOS)
            
            # Convert to PNG in memory
            png_buffer = io.BytesIO()
            img.save(png_buffer, format='PNG')
            png_buffer.seek(0)
            
            # Load into GdkPixbuf
            loader = GdkPixbuf.PixbufLoader.new_with_type('png')
            loader.write(png_buffer.read())
            loader.close()
            
            return loader.get_pixbuf()
            
    except Exception as e:
        print(f"Error loading image {path}: {e}")
        print(f"File exists: {os.path.exists(path)}")
        print(f"File permissions: {oct(os.stat(path).st_mode)[-3:]}")
        print(f"File size: {os.path.getsize(path)}")
        print(f"File contents (first 16 bytes): {open(path, 'rb').read(16)}")
        return None

class PdbApp(Gtk.Application):
    def __init__(self):
        super().__init__(application_id="org.proton.drive.bridge",
                        flags=Gio.ApplicationFlags.FLAGS_NONE)
        
        # Load the icon for use in the header button
        version_icon_path = get_icon_path(APP_VERSION)
        self.icon = load_pixbuf_safely(version_icon_path)
        if not self.icon:
            print(f"Warning: Failed to load application icon from {version_icon_path}")
        
    def do_startup(self):
        Gtk.Application.do_startup(self)
        self.set_app_menu(None)
        
    def do_activate(self):
        # Initialize the GTK Builder
        self.builder = Gtk.Builder()
        self.builder.add_from_file(get_resource_path("ui/pdb.ui"))

        # Load CSS
        css_provider = Gtk.CssProvider()
        css_provider.load_from_path(get_resource_path("ui/style.css"))
        Gtk.StyleContext.add_provider_for_display(
            Gdk.Display.get_default(),
            css_provider,
            Gtk.STYLE_PROVIDER_PRIORITY_APPLICATION
        )

        # Load window controls CSS
        wc_provider = Gtk.CssProvider()
        wc_provider.load_from_path(get_resource_path("ui/window-controls.css"))
        Gtk.StyleContext.add_provider_for_display(
            Gdk.Display.get_default(),
            wc_provider,
            Gtk.STYLE_PROVIDER_PRIORITY_APPLICATION
        )

        # Get the main window from the .ui file
        self.window = self.builder.get_object("main_window")
        if not self.window:
            raise RuntimeError("main_window not found in UI file!")
            
        # Get the stack and buttons
        self.stack = self.builder.get_object("main_stack")
        self.page1_button = self.builder.get_object("page1_button")
        self.page2_button = self.builder.get_object("page2_button")
        self.url_button1 = self.builder.get_object("url_button1")
        self.url_button2 = self.builder.get_object("url_button2")
        
        # Set the appropriate icons
        self.settings_icon = self.builder.get_object("settings_icon")
        self.heart_icon = self.builder.get_object("heart_icon")
        
        # Load and set the version-specific icon for the header button
        version_icon_path = get_icon_path(APP_VERSION)
        icon_pixbuf = load_pixbuf_safely(version_icon_path, 32)
        if icon_pixbuf:
            self.settings_icon.set_from_pixbuf(icon_pixbuf)
        else:
            print(f"Warning: Failed to load header icon from {version_icon_path}")
        
        # Load and set the heart icon
        heart_icon_path = get_resource_path("ui/assets/heart.png")
        heart_pixbuf = load_pixbuf_safely(heart_icon_path, 16)
        if heart_pixbuf:
            self.heart_icon.set_from_pixbuf(heart_pixbuf)
        else:
            print(f"Warning: Failed to load heart icon from {heart_icon_path}")
        
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
