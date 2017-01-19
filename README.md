# ClipboardMonitor
A small C# Library for Monitoring Clipboard with P/Invokes (e.g.: Clipboard Content Changed event)

[Download the Library (.dll)](https://raw.githubusercontent.com/mrousavy/ClipboardMonitor/master/Downloads/ClipboardMonitor.dll)

# How to use
Add the `.dll` to your .NET Project's `References` Tree in the Solution View.

_Side note: ClipboardMonitor has to create a Window in order to get hwnd to process WndProc messages in WPF. I've made the Window so that it is not visible but keep in mind that there is a WPF Window created once in the background._

Attach (C#):

```C#
var cm = new ClipboardMonitor();
cm.ClipboardData += (ClipboardDataEventArgs args) => {
    MessageBox.Show($"Clipboard Changed! {args.Data.ToString()}");
};
```