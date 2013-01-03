﻿/* Touchpad.cs
 *
 * Copyright © 2013 by Adam Hellberg
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Disclaimer: SwitchBladeSteam is in no way affiliated
 * with Razer and/or any of its employees and/or licensors.
 * Adam Hellberg does not take responsibility for any harm caused, direct
 * or indirect, to any Razer peripherals via the use of SwitchBladeSteam.
 * 
 * "Razer" is a trademark of Razer USA Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using F16Gaming.SwitchBladeSteam.Native;
using F16Gaming.SwitchBladeSteam.Razer.Exceptions;
using log4net;
using log4net.Core;

namespace F16Gaming.SwitchBladeSteam.Razer
{
	public class Touchpad
	{
		private ILog _log;

		/// <summary>
		/// Image that will show if Aero is disabled.
		/// </summary>
		public string DisabledImage { get; private set; }

		/// <summary>
		/// Will be set to IntPtr.Zero if no handle associated.
		/// </summary>
		public IntPtr CurrentHandle { get; private set; }

		/// <summary>
		/// Current image shown on the Touchpad, null if not using static image.
		/// </summary>
		public string CurrentImage { get; private set; }

		private Touchpad()
		{
			_log = Logging.LogManager.GetLogger(this);
			_log.Debug(">< Touchpad()");
		}

		internal Touchpad(IntPtr handle) : this()
		{
			_log.Debug(">> Touchpad([handle])");
			SetHandle(handle);
			_log.Debug("<< Touchpad()");
		}

		internal Touchpad(string image) : this()
		{
			_log.DebugFormat(">> Touchpad({0})", image);
			SetImage(image);
			_log.Debug("<< Touchpad()");
		}

		public void SetHandle(IntPtr handle)
		{
			_log.Debug(">> SetHandle([handle])");
			StopRender(false);

			var hResult = RazerAPI.RzSBWinRenderStart(handle, true, Constants.DebugEnabled);
			if (!HRESULT.RZSB_SUCCESS(hResult))
				throw new RazerNativeException(hResult);

			_log.Debug("<< SetHandle()");
		}

		public void SetKeyboardEnabledControls(IEnumerable<IntPtr> controlHandles)
		{
			_log.Debug(">> SetKeyboardEnabledControls([handles])");
			var handles = controlHandles as IntPtr[] ?? controlHandles.ToArray();
			var controls = new RazerAPI.RZSB_KEYEVTCTRLS[handles.Length];
			for (int i = 0; i < handles.Length; i++)
				controls[i] = new RazerAPI.RZSB_KEYEVTCTRLS {hwndTarget = handles[i], bReleaseOnEnter = true};
			var hResult = RazerAPI.RzSBWinRenderAddKeyInputCtrls(controls, controls.Length, true);
			if (!HRESULT.RZSB_SUCCESS(hResult))
				throw new RazerNativeException(hResult);
			_log.Debug("<< SetKeyboardEnabledControls()");
		}

		public void SetImage(string image)
		{
			_log.DebugFormat(">> SetImage({0})", image);
			StopRender();

			var hResult = RazerAPI.RzSBSetImageTouchpad(image);
			if (!HRESULT.RZSB_SUCCESS(hResult))
				throw new RazerNativeException(hResult);

			_log.Debug("<< SetImage()");
		}

		public void StopRender(bool erase = true)
		{
			_log.DebugFormat(">> StopRender({0})", erase ? "true" : "false");
			if (CurrentHandle == IntPtr.Zero)
				return;

			var hResult = RazerAPI.RzSBWinRenderStop(erase);
			if (!HRESULT.RZSB_SUCCESS(hResult))
				throw new RazerNativeException(hResult);

			_log.Debug("<< StopRender()");
		}

		public void ClearImage()
		{
			_log.Debug(">> ClearImage()");
			var hResult = RazerAPI.RzSBSetImageTouchpad(Helpers.IO.GetAbsolutePath(Constants.BlankTouchpadImage));
			if (!HRESULT.RZSB_SUCCESS(hResult))
				throw new RazerNativeException(hResult);
			_log.Debug("<< ClearImage()");
		}
	}
}