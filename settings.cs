/*

Copyright 2014 Daniel Kinsman.

This file is part of Kessler Chaos.

Kessler Chaos is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Kessler Chaos is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kessler Chaos.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.IO;
using KSP;
using UnityEngine;

namespace kesslerchaos
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class SettingsBehaviour: MonoBehaviourWindow
	{
		public const string TOOLBAR_NAMESPACE = "kesslerchaos";
		public const string TOOLBAR_ICON = "kesslerchaos/toolbaricon";// todo breaks on windows?
		public Settings settings{get; private set;}
		private IButton showGUIButton;

		internal override void Awake()
        {
			WindowCaption = "Kessler Chaos Settings";
            WindowRect = new Rect(0, 0, 250, 50);

			if(ToolbarManager.ToolbarAvailable)
			{
				Visible = false;
				showGUIButton = ToolbarManager.Instance.add(TOOLBAR_NAMESPACE, "KCsettings");
				showGUIButton.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
				showGUIButton.ToolTip = WindowCaption;
				showGUIButton.TexturePath = TOOLBAR_ICON;
				showGUIButton.OnClick += (e) => this.Visible = !this.Visible;
			}
			else
			{
				Visible = true;
				showGUIButton = null;
			}

			settings = new Settings();
        }

		internal override void OnDestroy()
		{
			base.OnDestroy();
			if(showGUIButton != null)
				showGUIButton.Destroy();
		}

		internal override void DrawWindow(int id)
		{
			DragEnabled = true;
			ClampToScreen = true;
			TooltipsEnabled = false;

			GUILayout.BeginHorizontal();
            GUILayout.Label("Mod enabled for this save game:");
            settings.modEnabled=Convert.ToBoolean(GUILayout.Toggle(settings.modEnabled, "mod enabled"));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("Stock ksp debris count before we reach full chaos (lower for more encounters):");
            settings.worstDebrisCount=Convert.ToInt32(GUILayout.TextField(settings.worstDebrisCount.ToString()));
            GUILayout.EndHorizontal();
		}
	}

	public class Settings : ConfigNodeStorage
	{
		[Persistent]
		private bool _modEnabled = true;

		public bool modEnabled
		{
			get
			{
				return _modEnabled;
			}
			set
			{
				if(this._modEnabled != value)
				{
					this._modEnabled = value;
					this.Save();
				}
			}
		}

		[Persistent]
		private int _worstDebrisCount = 250;

		public int worstDebrisCount
		{
			get
			{
				return _worstDebrisCount;
			}
			set
			{
				if(this._worstDebrisCount != value)
				{
					this._worstDebrisCount = value;
					this.Save();
				}
			}
		}

		public Settings()
		{
			// This will save settings in the same directory as the savegames.
			// KSP is naughty and stores savegames in the application directory.
			this.FilePath = Path.Combine(
								Path.Combine(KSPUtil.ApplicationRootPath, "saves"),
								Path.Combine(HighLogic.SaveFolder, "kesslerchaos_settings")
							);
			Load();
		}
	}
}