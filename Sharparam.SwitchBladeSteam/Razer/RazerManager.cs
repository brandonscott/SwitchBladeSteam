﻿/* RazerManager.cs
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
using System.IO;
using F16Gaming.SwitchBladeSteam.Native;
using F16Gaming.SwitchBladeSteam.Razer.Events;
using F16Gaming.SwitchBladeSteam.Razer.Exceptions;
using log4net;

using LogManager = F16Gaming.SwitchBladeSteam.Logging.LogManager;

namespace F16Gaming.SwitchBladeSteam.Razer
{
	public delegate void VoidDelegate();

	public class RazerManager
	{
		public event AppEventEventHandler AppEvent;
		public event DynamicKeyEventHandler DynamicKeyEvent;

		private const string RazerControlFile = "DO_NOT_TOUCH__RAZER_CONTROL_FILE";

		private readonly ILog _log;
		private static readonly ILog StaticLog = LogManager.GetLogger(typeof (RazerManager));

		private Touchpad _touchpad;
		private readonly DynamicKey[] _dynamicKeys;
		private readonly VoidDelegate[] _dkCallbacks;

		// Native code callbacks
		private static RazerAPI.AppEventCallbackDelegate _appEventCallback;
		private static RazerAPI.DynamicKeyCallbackFunctionDelegate _dkCallback;

		public RazerManager(string disabledImage = Constants.DisabledTouchpadImage)
		{
			_log = LogManager.GetLogger(this);

			_log.Debug(">> RazerManager()");

			_log.Info("RazerManager is initializing");

			if (File.Exists(RazerControlFile))
			{
				_log.Error("Detected control file presence, throwing exception.");
				_log.Debug("<< RazerManager()");
				throw new RazerUnstableShutdownException();
			}

			CreateControlFile();

			_log.Debug("Calling RzSBStart()");

			var hResult = RazerAPI.RzSBStart();
			if (HRESULT.RZSB_FAILED(hResult))
			{
				// Try one more time
				hResult = RazerAPI.RzSBStart();
				if (HRESULT.RZSB_FAILED(hResult))
					NativeCallFailure("RzSBStart", hResult);
			}

			_log.Debug("Calling RzSBWinRenderSetDisabledImage");
			
			hResult = RazerAPI.RzSBWinRenderSetDisabledImage(Helpers.IO.GetAbsolutePath(disabledImage));
			if (HRESULT.RZSB_FAILED(hResult))
				NativeCallFailure("RzSBWinRenderSetDisabledImage", hResult);

			_log.Info("Setting up dynamic keys");

			_log.Debug("Creating new app event callback");
			_appEventCallback = HandleAppEvent;
			_log.Debug("Calling RzSBAppEventSetCallback");
			hResult = RazerAPI.RzSBAppEventSetCallback(_appEventCallback);
			if (HRESULT.RZSB_FAILED(hResult))
				NativeCallFailure("RzSBAppEventSetCallback", hResult);

			_log.Info("Setting up touchpad");
			_touchpad = new Touchpad();

			_log.Debug("Creating dynamic key callback");
			_dkCallback = HandleDynamicKeyEvent;
			_log.Debug("Calling RzSBDynamicKeySetCallback");
			hResult = RazerAPI.RzSBDynamicKeySetCallback(_dkCallback);
			if (HRESULT.RZSB_FAILED(hResult))
				NativeCallFailure("RzSBDynamicKeySetCallback", hResult);

			_log.Debug("Initializing dynamic key arrays");

			_dynamicKeys = new DynamicKey[RazerAPI.DYNAMIC_KEYS_COUNT];
			_dkCallbacks = new VoidDelegate[RazerAPI.DYNAMIC_KEYS_COUNT];

			_log.Debug("<< RazerManager()");
		}

		private void OnAppEvent(RazerAPI.RZSDKAPPEVENTTYPE type, RazerAPI.RZSDKAPPEVENTMODE mode, uint processId)
		{
			var func = AppEvent;
			if (func != null)
				func(this, new AppEventEventArgs(type, mode, processId));
		}

		private void OnDynamicKeyEvent(RazerAPI.RZDYNAMICKEY key, RazerAPI.RZDKSTATE state)
		{
			var func = DynamicKeyEvent;
			if (func != null)
				func(this, new DynamicKeyEventArgs(key, state));
		}

		public static void CreateControlFile()
		{
			try
			{
				if (File.Exists(RazerControlFile))
					StaticLog.Warn("CreateControlFile: File already exists");
				else
				{
					File.Create(RazerControlFile);
					StaticLog.Info("CreateControlFile: Success!");
				}
			}
			catch (IOException ex)
			{
				StaticLog.ErrorFormat("CreateControlFile: [IOException] Failed to create control file: {0}", ex.Message);
			}
		}

		public static void DeleteControlFile()
		{
			try
			{
				if (File.Exists(RazerControlFile))
				{
					File.Delete(RazerControlFile);
					StaticLog.Info("DeleteControlFile: Success!");
				}
				else
					StaticLog.Warn("DeleteControlFile: File does not exist");
			}
			catch (IOException ex)
			{
				StaticLog.ErrorFormat("DeleteControlFile: [IOException] Failed to delete control file: {0}", ex.Message);
			}
		}

		public static void Stop(bool cleanup = true)
		{
			StaticLog.Info("RazerManager is stopping! Calling RzSBStop...");
			var hResult = RazerAPI.RzSBStop();
			if (HRESULT.RZSB_FAILED(hResult))
				NativeCallFailure("RzSBStop", hResult);
			if (cleanup)
				DeleteControlFile();
			StaticLog.Info("RazerManager has stopped.");
		}

		internal static void NativeCallFailure(string func, HRESULT result)
		{
			StaticLog.FatalFormat("Call to RazerAPI native function {0} FAILED with error: {1}", func, result.ToString());
			StaticLog.Debug("Throwing exception...");
			throw new RazerNativeException(result);
		}

		public Touchpad GetTouchpad()
		{
			return _touchpad;
		}

		public DynamicKey GetDynamicKey(RazerAPI.RZDYNAMICKEY key)
		{
			return _dynamicKeys[(int) key - 1];
		}

		public DynamicKey EnableDynamicKey(RazerAPI.RZDYNAMICKEY key, DynamicKeyPressedEventHandler callback, string upImage, string downImage = null, bool replace = false)
		{
			_log.DebugFormat(">> EnableDynamicKey({0}, [callback], \"{1}\", {2}, {3})", key, upImage,
			                 downImage == null ? "null" : "\"" + downImage + "\"", replace ? "true" : "false");

			var index = (int) key - 1;
			if (_dynamicKeys[index] != null && !replace)
			{
				_log.Info("Dynamic key already enabled and replace is false.");
				_log.Debug("<< EnableDynamicKey()");
				return _dynamicKeys[index];
			}

			_log.Debug("Resetting dynamic key (DisableDynamicKey)");
			DisableDynamicKey(key);
			try
			{
				_log.Debug("Creating new DynamicKey object");
				var dk = new DynamicKey(key, upImage, downImage, callback);
				_dynamicKeys[index] = dk;
			}
			catch (RazerNativeException ex)
			{
				_log.ErrorFormat("Failed to enable dynamic key {0}: {1}", key, ex.Hresult);
				_log.Debug("<< EnableDynamicKey()");
				return null;
			}

			_log.Debug("<< EnableDynamicKey()");
			return _dynamicKeys[index];
		}

		public void DisableDynamicKey(RazerAPI.RZDYNAMICKEY key)
		{
			_log.DebugFormat(">> DisableDynamicKey({0})", key);
			var index = (int) key - 1;
			var dk = _dynamicKeys[index];
			if (dk != null)
				dk.Disable();
			_dynamicKeys[index] = null;
			_log.Debug("<< DisableDynamicKey()");
		}

		private HRESULT HandleAppEvent(RazerAPI.RZSDKAPPEVENTTYPE type, uint appMode, uint processId)
		{
			var hResult = HRESULT.RZSB_OK;
			if (type != RazerAPI.RZSDKAPPEVENTTYPE.APPMODE)
			{
				_log.DebugFormat("Unsupported AppEventType: {0}", type);
				return hResult;
			}

			OnAppEvent(type, (RazerAPI.RZSDKAPPEVENTMODE) appMode, processId);

			return hResult;
		}

		private HRESULT HandleDynamicKeyEvent(RazerAPI.RZDYNAMICKEY key, RazerAPI.RZDKSTATE state)
		{
			_log.DebugFormat(">> HandleDynamicKeyEvent({0}, {1})", key, state);

			var result = HRESULT.RZSB_OK;

			_log.Debug("Raising DynamicKeyEvent event");
			OnDynamicKeyEvent(key, state);

			var index = (int) key - 1;
			var dk = _dynamicKeys[index];
			if (dk == null)
			{
				_log.Debug("Key has not been registered by app");
				_log.Debug("<< HandleDynamicKeyEvent()");
				return result;
			}

			_log.Debug("Updating key state");
			// UpdateState will check if it's a valid press and call any event subscribers
			dk.UpdateState(state);

			_log.Debug("<< HandleDynamicKeyEvent()");
			return result;
		}
	}
}