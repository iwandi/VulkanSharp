/* Please note that this file is generated by the VulkanSharp's generator. Do not edit directly.

   Licensed under the MIT license.

   Copyright 2016 Xamarin Inc

   This notice may not be removed from any source distribution.
   See LICENSE file for licensing details.
*/

using System;
using System.Runtime.InteropServices;

namespace Vulkan
{
	unsafe public partial class Win32SurfaceCreateInfoKhr
	{
		public UInt32 Flags {
			get { return m->Flags; }
			set { m->Flags = value; }
		}

		public IntPtr Hinstance {
			get { return m->Hinstance; }
			set { m->Hinstance = value; }
		}

		public IntPtr Hwnd {
			get { return m->Hwnd; }
			set { m->Hwnd = value; }
		}

		public static Win32SurfaceCreateInfoKhr Null = new Win32SurfaceCreateInfoKhr(null,false);

		internal Windows.Interop.Win32SurfaceCreateInfoKhr* m;

		public Win32SurfaceCreateInfoKhr ()
		{
			m = (Windows.Interop.Win32SurfaceCreateInfoKhr*) Interop.Structure.Allocate (typeof (Windows.Interop.Win32SurfaceCreateInfoKhr));
			Initialize ();
		}

		internal Win32SurfaceCreateInfoKhr (Windows.Interop.Win32SurfaceCreateInfoKhr* ptr)
		{
			m = ptr;
			Initialize ();
		}

		internal Win32SurfaceCreateInfoKhr (Windows.Interop.Win32SurfaceCreateInfoKhr* ptr, bool init)
		{
			m = ptr;
			if(init)
			{
				Initialize ();
			}
		}


		internal void Initialize ()
		{
			m->SType = StructureType.Win32SurfaceCreateInfoKhr;
		}

	}

}
