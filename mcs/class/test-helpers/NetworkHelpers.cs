using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace MonoTests.Helpers {

	public static class NetworkHelpers
	{
		static Random rndPort = new Random ();
		static HashSet<int> portsTable = new HashSet<int> ();

		public static int FindFreePort ()
		{
			return LocalEphemeralEndPoint ().Port;
		}

		public static IPEndPoint LocalEphemeralEndPoint ()
		{
#if FEATURE_NO_BSD_SOCKETS
			throw new PlatformNotSupportedException ();
#endif
			int counter = 0;

			while (counter < 1000) {
				var testingPort = rndPort.Next (10000, 60000);

				var ep = new IPEndPoint (IPAddress.Loopback, testingPort);

				try {
					lock (portsTable) {
						Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked", Thread.CurrentThread.ManagedThreadId);
						if (portsTable.Contains (testingPort)) {
							Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 1", Thread.CurrentThread.ManagedThreadId);
							continue;
						}

						++counter;
						Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 2 {1}", Thread.CurrentThread.ManagedThreadId, counter);

						try {
							using (var socket = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp)) {
								Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 3", Thread.CurrentThread.ManagedThreadId);
								socket.Bind (ep);
								Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 4", Thread.CurrentThread.ManagedThreadId);
								socket.Close ();
								Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 5", Thread.CurrentThread.ManagedThreadId);
							}

							portsTable.Add (testingPort);
							Console.WriteLine ("[{0}] LocalEphemeralEndPoint found port: {1}", Thread.CurrentThread.ManagedThreadId, testingPort);
							return ep;
						} catch (SocketException sex) {
							Console.WriteLine ("[{0}] LocalEphemeralEndPoint exc: {1}", Thread.CurrentThread.ManagedThreadId, sex.Message);
						}
						Console.WriteLine ("[{0}] LocalEphemeralEndPoint locked 6", Thread.CurrentThread.ManagedThreadId);
					}
				} finally {
					Console.WriteLine ("[{0}] LocalEphemeralEndPoint unlocked", Thread.CurrentThread.ManagedThreadId);
				}
			}

			throw new ApplicationException ($"Could not find available local port after {counter} retries");
		}
	}
}
