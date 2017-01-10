using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ClipboardMonitor {
    public class ClipboardMonitor : Window {
        private HwndSource source = null;
        private IntPtr nextClipboardViewer;
        private IntPtr handle {
            get {
                return new WindowInteropHelper(this).Handle;
            }
        }
        public ClipboardMonitor() {
            //"show" the window in order to obtain hwnd to process WndProc messages in WPF
            this.Top = -10;
            this.Left = -10;
            this.Width = 0;
            this.Height = 0;
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = false;
            this.ShowActivated = false;
            this.Show();
            this.Hide();
        }

        #region Dependency properties
        public static readonly DependencyProperty ClipboardContainsImageProperty =
         DependencyProperty.Register(
         "ClipboardContainsImage",
         typeof(bool),
         typeof(Window),
         new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ClipboardContainsTextProperty =
         DependencyProperty.Register(
         "ClipboardContainsText",
         typeof(bool),
         typeof(Window),
         new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ClipboardTextProperty =
         DependencyProperty.Register(
         "ClipboardText",
         typeof(string),
         typeof(Window),
         new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty ClipboardImageProperty =
         DependencyProperty.Register(
         "ClipboardImage",
         typeof(BitmapSource),
         typeof(Window),
         new FrameworkPropertyMetadata(null));
        public bool ClipboardContainsImage {
            get { return (bool)GetValue(ClipboardMonitor.ClipboardContainsImageProperty); }
            set { SetValue(ClipboardMonitor.ClipboardContainsImageProperty, value); }
        }
        public bool ClipboardContainsText {
            get { return (bool)GetValue(ClipboardMonitor.ClipboardContainsTextProperty); }
            set { SetValue(ClipboardMonitor.ClipboardContainsTextProperty, value); }
        }
        public string ClipboardText {
            get { return (string)GetValue(ClipboardMonitor.ClipboardTextProperty); }
            set { SetValue(ClipboardMonitor.ClipboardTextProperty, value); }
        }
        public BitmapSource ClipboardImage {
            get { return (BitmapSource)GetValue(ClipboardMonitor.ClipboardImageProperty); }
            set { SetValue(ClipboardMonitor.ClipboardImageProperty, value); }
        }
        #endregion

        #region Routed Events
        public static readonly RoutedEvent ClipboardDataEvent = EventManager.RegisterRoutedEvent("ClipboardData", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Window));
        /// <summary>
        /// Fires upon Clipboard Content change
        /// </summary>
        public event RoutedEventHandler ClipboardData {
            add { AddHandler(ClipboardMonitor.ClipboardDataEvent, value); }
            remove { RemoveHandler(ClipboardMonitor.ClipboardDataEvent, value); }
        }
        protected virtual void OnRaiseClipboardData(ClipboardDataEventArgs e) {
            RaiseEvent(e);
        }
        #endregion

        #region Win32 API
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        [DllImport("User32.dll")]
        private static extern int SetClipboardViewer(int hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region overrides
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.handle);
            source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            ChangeClipboardChain(this.handle, nextClipboardViewer);
            if(null != source)
                source.RemoveHook(WndProc);
        }
        #endregion

        #region Clipboard data
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch(msg) {
                case WM_DRAWCLIPBOARD:
                    clipboardData();
                    SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if(wParam == nextClipboardViewer)
                        nextClipboardViewer = lParam;
                    else
                        SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;
            }
            return IntPtr.Zero;
        }
        private void clipboardData() {
            IDataObject iData = Clipboard.GetDataObject();
            this.ClipboardContainsImage = iData.GetDataPresent(DataFormats.Bitmap);
            this.ClipboardContainsText = iData.GetDataPresent(DataFormats.Text);
            this.ClipboardImage = this.ClipboardContainsImage ? iData.GetData(DataFormats.Bitmap) as BitmapSource : null;
            this.ClipboardText = this.ClipboardContainsText ? iData.GetData(DataFormats.Text) as string : string.Empty;
            OnRaiseClipboardData(new ClipboardDataEventArgs(ClipboardMonitor.ClipboardDataEvent, iData));
        }
        #endregion
    }

    public class ClipboardDataEventArgs : RoutedEventArgs {
        public IDataObject Data { get; set; }
        public ClipboardDataEventArgs(RoutedEvent routedEvent, IDataObject data)
         : base(routedEvent) {
            this.Data = data;
        }
    }
}