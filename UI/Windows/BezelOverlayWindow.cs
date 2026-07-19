using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;

namespace Mesen.Windows
{
	public class BezelOverlayWindow : Window
	{
		private const int GwlExStyle = -20;
		private const int WsExTransparent = 0x20;
		private const int WsExToolWindow = 0x80;
		private const int WsExLayered = 0x80000;
		private const int WsExNoActivate = 0x8000000;
		private const int LwaColorKey = 0x1;
		private const int SwpNoSize = 0x1;
		private const int SwpNoMove = 0x2;
		private const int SwpNoActivate = 0x10;
		private const int SwpShowWindow = 0x40;
		private static readonly IntPtr HwndTopmost = new IntPtr(-1);

		private readonly Image _image = new Image() {
			Stretch = Stretch.Fill,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			IsHitTestVisible = false
		};
		private string? _loadedPath;

		public BezelOverlayWindow()
		{
			SystemDecorations = SystemDecorations.None;
			ShowInTaskbar = false;
			CanResize = false;
			Topmost = true;
			ShowActivated = false;
			Background = Brushes.Black;
			Content = _image;
		}

		public void SetBezel(string path)
		{
			if(path == _loadedPath) {
				return;
			}

			_image.Source = new Bitmap(path);
			_loadedPath = path;
		}

		public void FitToOwnerScreen(Window owner)
		{
			Screen? screen = owner.Screens.ScreenFromWindow(owner) ?? owner.Screens.Primary;
			if(screen == null && owner.Screens.All.Count > 0) {
				screen = owner.Screens.All[0];
			}
			if(screen == null) {
				return;
			}

			Position = screen.Bounds.Position;
			Width = screen.Bounds.Width / screen.Scaling;
			Height = screen.Bounds.Height / screen.Scaling;
		}

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);
			MakeClickThrough();
		}

		private void MakeClickThrough()
		{
			if(!OperatingSystem.IsWindows()) {
				return;
			}

			IntPtr hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
			if(hwnd == IntPtr.Zero) {
				return;
			}

			int style = GetWindowLong(hwnd, GwlExStyle);
			SetWindowLong(hwnd, GwlExStyle, style | WsExTransparent | WsExLayered | WsExToolWindow | WsExNoActivate);
			SetLayeredWindowAttributes(hwnd, 0, 0, LwaColorKey);
			SetWindowPos(hwnd, HwndTopmost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate | SwpShowWindow);
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
	}
}
