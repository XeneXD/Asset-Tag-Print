using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace AssetTagPrinter.Icons
{
	public static class IconManager
	{
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);

		private readonly struct IconSlot
		{
			public IconSlot(string fileName, int size)
			{
				FileName = fileName;
				Size = size;
			}

			public string FileName { get; }
			public int Size { get; }
		}

		private static readonly IconSlot[] IconSlots = new[]
		{
			new IconSlot("16x16.ico", 16),
			new IconSlot("24x24.ico", 24),
			new IconSlot("32x32.ico", 32),
			new IconSlot("48x48.ico", 48),
			new IconSlot("256x256.ico", 256)
		};

		private static string? ResolveIconPath(string fileName)
		{
			// Preferred legacy folder name
			string pathInOutput = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", ".ico Files", fileName);
			if (File.Exists(pathInOutput))
			{
				return pathInOutput;
			}

			// Also support Icons root if files are stored directly there
			string pathInOutputRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", fileName);
			if (File.Exists(pathInOutputRoot))
			{
				return pathInOutputRoot;
			}

			string pathInCwd = Path.Combine(Environment.CurrentDirectory, "Icons", ".ico Files", fileName);
			if (File.Exists(pathInCwd))
			{
				return pathInCwd;
			}

			string pathInCwdRoot = Path.Combine(Environment.CurrentDirectory, "Icons", fileName);
			if (File.Exists(pathInCwdRoot))
			{
				return pathInCwdRoot;
			}

			DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while (dir != null)
			{
				string candidate = Path.Combine(dir.FullName, "Icons", ".ico Files", fileName);
				if (File.Exists(candidate))
				{
					return candidate;
				}

				string candidateRoot = Path.Combine(dir.FullName, "Icons", fileName);
				if (File.Exists(candidateRoot))
				{
					return candidateRoot;
				}

				dir = dir.Parent;
			}

			return null;
		}

		private static Icon? LoadByIndex(int fileIndex, int[] fallbackOrder)
		{
			if (fileIndex >= 0 && fileIndex < IconSlots.Length)
			{
				IconSlot selectedSlot = IconSlots[fileIndex];
				Icon? selected = LoadIconFromPath(selectedSlot.FileName, selectedSlot.Size);
				if (selected != null)
				{
					return selected;
				}
			}

			foreach (int idx in fallbackOrder)
			{
				if (idx >= 0 && idx < IconSlots.Length)
				{
					IconSlot fallbackSlot = IconSlots[idx];
					Icon? fallback = LoadIconFromPath(fallbackSlot.FileName, fallbackSlot.Size);
					if (fallback != null)
					{
						return fallback;
					}
				}
			}

			return null;
		}

		private static Icon? LoadIconFromPath(string fileName, int preferredSize)
		{
			try
			{
				string? fullPath = ResolveIconPath(fileName);
				if (string.IsNullOrEmpty(fullPath))
				{
					return null;
				}

				// First try parsing as a native .ico container.
				using (Icon icon = new Icon(fullPath))
				{
					return (Icon)icon.Clone();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Icon parse failed for '{fileName}', trying bitmap fallback: {ex.Message}");
			}

			// Fallback for PNG-encoded files with .ico extension.
			try
			{
				string? fullPath = ResolveIconPath(fileName);
				if (string.IsNullOrEmpty(fullPath))
				{
					return null;
				}

				using (Bitmap source = new Bitmap(fullPath))
				using (Bitmap sized = new Bitmap(source, new Size(preferredSize, preferredSize)))
				{
					IntPtr hIcon = sized.GetHicon();
					try
					{
						using (Icon temp = Icon.FromHandle(hIcon))
						{
							return (Icon)temp.Clone();
						}
					}
					finally
					{
						DestroyIcon(hIcon);
					}
				}
			}
			catch (Exception fallbackEx)
			{
				System.Diagnostics.Debug.WriteLine($"Bitmap fallback failed for '{fileName}': {fallbackEx.Message}");
				return null;
			}
		}

		/// <summary>
		/// Load title bar icon by file index.
		/// Index map: 0=16x16, 1=24x24, 2=32x32, 3=48x48, 4=256x256
		/// </summary>
		public static Icon? LoadTitleBarIcon(int iconIndex = 0)
		{
			return LoadByIndex(iconIndex, new[] { 0, 2, 1, 3, 4 });
		}

		/// <summary>
		/// Load taskbar icon by file index.
		/// Index map: 0=16x16, 1=24x24, 2=32x32, 3=48x48, 4=256x256
		/// </summary>
		public static Icon? LoadTaskbarIcon(int iconIndex = 0)
		{
			return LoadByIndex(iconIndex, new[] { 3, 4, 2, 1, 0 });
		}
	}
}
