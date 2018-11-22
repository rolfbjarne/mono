using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoTests.Helpers {

	public static class PathHelpers
	{
		static string root;
		static int last_number;

		static PathHelpers ()
		{
			root = Path.Combine (Path.GetTempPath (), "mcs-tests-temporary-dir", Assembly.GetExecutingAssembly ().GetName ().Name);
			Directory.CreateDirectory (root);
		}

		[DllImport ("libc", SetLastError = true)]
		static extern int mkdir (string path, ushort mode);

		public static string CreateTemporaryDirectory (string name = null)
		{
			if (string.IsNullOrEmpty (name)) {
				var calling_method = new StackFrame (1).GetMethod ();
				if (calling_method != null) {
					name = calling_method.DeclaringType.FullName + "." + calling_method.Name;
				} else {
					name = "unknown-test";
				}
			}

			var rv = Path.Combine (root, name);
			for (int i = last_number; i < 10000 + last_number; i++) {
				// There's no way to know if Directory.CreateDirectory
				// created the directory or not (which would happen if the directory
				// already existed). Checking if the directory exists before
				// creating it would result in a race condition if multiple
				// threads create temporary directories at the same time.
				if (mkdir (rv, Convert.ToUInt16 ("777", 8)) == 0) {
					last_number = i;
					return rv;
				}
				rv = Path.Combine (root, name + i);
			}

			throw new Exception ("Could not create temporary directory");
		}
	}
}
