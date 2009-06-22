
using System;
using System.Runtime.InteropServices;

namespace Monsoon
{
	public class OpenSSLSha1 : System.Security.Cryptography.SHA1
	{
		IntPtr context;
		public OpenSSLSha1 ()
		{
			// The native SHA1 structure is 96 bytes in length
			context = Marshal.AllocHGlobal (96);
			if (SHA1_Init (context) != 1)
				throw new Exception ("Could not init context");
		}
		
		public override void Initialize ()
		{
			if (SHA1_Init (context) != 1)
				throw new Exception ("Could not init context");
		}
		
		protected override unsafe void HashCore (byte[] array, int ibStart, int cbSize)
		{
			if (ibStart > array.Length)
				throw new IndexOutOfRangeException ("ibStart");
			if ((array.Length - ibStart) < cbSize)
				throw new IndexOutOfRangeException ("cbSize");
			
			if (cbSize == 0)
				return;
			
			fixed (byte *ptr = array)
				if (SHA1_Update (context, ptr + ibStart, (ulong) cbSize) != 1)
					throw new Exception ("Could not hash data");
		}

		protected override byte[] HashFinal ()
		{
			// Result must be at least of length 20
			byte [] result = new byte [20];
			if (SHA1_Final (result, context) != 1)
				throw new Exception ("Could not hash final chunk");
			return result;
		}

		// All return '1' for success, '0' for failure
		[DllImport ("libssl.so.0.9.8")]
		static extern int SHA1_Init (IntPtr context);
		[DllImport ("libssl.so.0.9.8")]
		static extern unsafe int SHA1_Update (IntPtr context, byte *data, ulong len);
		[DllImport ("libssl.so.0.9.8")]
		static extern int SHA1_Final (byte [] buffer, IntPtr context);
	}
}
