using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoTests.Helpers {

	public static class PathHelpers
	{
		static string root;
		static long last_number;

		static PathHelpers ()
		{
			root = Path.Combine (Path.GetTempPath (), "mcs-tests-temporary-dir", Assembly.GetExecutingAssembly ().GetName ().Name);
			Directory.CreateDirectory (root);
			last_number = (int) DateTime.Now.Ticks;
		}

		[DllImport ("libc", SetLastError = true)]
		static extern int mkdir (string path, ushort mode);

		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool CreateDirectory (string path, IntPtr securityAttributes);

		static bool RunningOnWindows ()
		{
			int i = (int) Environment.OSVersion.Platform;
			return ((i != 4) && (i != 128));
		}

		public static string CreateTemporaryDirectory (string name = null)
		{
			var dir_root = root;
			if (string.IsNullOrEmpty (name)) {
				var calling_method = new StackFrame (1).GetMethod ();
				if (calling_method != null) {
					dir_root = Path.Combine (root, calling_method.DeclaringType.FullName, calling_method.Name);
				} else {
					dir_root = Path.Combine (root, "unknown-test");
				}
			} else {
				dir_root = Path.Combine (root, name);
			}
			Directory.CreateDirectory (dir_root);

			var rv = Path.Combine (dir_root, last_number.ToString ());
			for (long i = last_number; i < 10000 + last_number; i++) {
				// There's no way to know if Directory.CreateDirectory
				// created the directory or not (which would happen if the directory
				// already existed). Checking if the directory exists before
				// creating it would result in a race condition if multiple
				// threads create temporary directories at the same time.
				bool createResult;

				if (RunningOnWindows ()) {
					createResult = CreateDirectory (rv, IntPtr.Zero);
				} else {
					createResult = mkdir (rv, 511 /*Convert.ToUInt16 ("777", 8)*/) == 0;
				}
				if (createResult) {
					last_number = i;
					return rv;
				}
				rv = Path.Combine (dir_root, i.ToString ());
			}
			rv = Path.GetTempFileName ();
			File.Delete (rv);
			Directory.CreateDirectory (rv);
			return rv;
		}
	}
}
