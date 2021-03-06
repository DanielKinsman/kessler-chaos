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
using System.Collections.Generic;
using KSP;
using UnityEngine;

namespace kesslerchaos
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightBehaviour : MonoBehaviourWindow
    {
		public bool fire = false;
		public float speed = 437.5f;
		public float longitudinalSpread = 1000.0f;
		public float lateralSpread = 100.0f;
		public float longitudinalVelocitySpread = 62.5f;
		public float lateralVelocitySpread = 100.0f;
		public int maxSpawnCount = 25;
		public float repeatRate = 0.25f;
		public float idleRepeatRate = 60.0f;
		public int maxShrapnel = 500;
		private Queue<GameObject> shrapnel;
		public Vector3 debrisOrigin;
		public float duration = 30.0f;
		public double eventStart = -60.0;
		public float intensity = 1.0f;
		private bool timeWarpHighAllowedPreviously;
		private bool blockingTimeWarp = false;
		private object blockTimeWarpLock = new object();
		private Settings settings;
		private IButton showGUIButton;
		private int debrisCount;

        internal override void Awake()
        {
			settings = new Settings();
			if(!settings.modEnabled)
				return;

			WindowCaption = "Kessler Chaos";
            WindowRect = new Rect(0, 0, 250, 50);

			if(ToolbarManager.ToolbarAvailable)
			{
				Visible = false;
				showGUIButton = ToolbarManager.Instance.add(SettingsBehaviour.TOOLBAR_NAMESPACE, "KCflight");
				showGUIButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
				showGUIButton.ToolTip = WindowCaption;
				showGUIButton.TexturePath = SettingsBehaviour.TOOLBAR_ICON;
				showGUIButton.OnClick += (e) => this.Visible = !this.Visible;
			}
			else
			{
				Visible = true;
				showGUIButton = null;
			}

			SetRepeatRate(idleRepeatRate);
            StartRepeatingWorker();
			shrapnel = new Queue<GameObject>(maxShrapnel);
			eventStart = 0.0;
        }

		internal override void OnDestroy()
		{
			base.OnDestroy();
			this.shrapnel.Clear();
			RestoreTimeWarp();
			if(showGUIButton != null)
				showGUIButton.Destroy();
		}

        internal override void RepeatingWorker()
        {
			try
			{
				var elapsed = Planetarium.GetUniversalTime() - eventStart;
				if(elapsed > this.duration)
				{
					// Avoid setting the repeat rate unnecessarily as it
					// restarts the worker
					if(this.RepeatingWorkerRate != idleRepeatRate)
					{
						RestoreTimeWarp();
						// We just finished a debris cloud encounter
						SetRepeatRate(idleRepeatRate);
						// todo clean up shrapnel
						return;
					}

					if(CheckForNewDebrisEncounter(elapsed))
						NewDebrisEncounter();

					return;
				}

				SpawnDebrisParticles(elapsed);
			}
			catch(Exception e)
			{
				LogFormatted_DebugOnly("You fucked up: {0}", e);
				throw;
			}
        }

		public double EncounterProbability(double elapsed)
		{
			// 	* no encounter in atmosphere or below max terrain height
			// 	* higher likelyhood the lower you are
			// 	* higher likelyhood depending on debrisCount and worstDebrisCount
			// 	* higher likelyhood the longer it's been since the last debris cloud encounter

			float minDebrisAltitude = Math.Max(FlightGlobals.currentMainBody.maxAtmosphereAltitude, FlightGlobals.currentMainBody.timeWarpAltitudeLimits[1]);

			if(FlightGlobals.ship_altitude < minDebrisAltitude)
				return 0.0f;

			var altitudeMultiplier = 1.0f / (FlightGlobals.ship_altitude / minDebrisAltitude);
			var litterMultiplier = Math.Min(1.0f, debrisCount / (float)settings.worstDebrisCount);
			var frequencyMultiplier = Math.Min(1.0f, elapsed / (5.0f * 60.0f)); // after 5 minutes bring it on

			return altitudeMultiplier * litterMultiplier * frequencyMultiplier;
		}

		public float EncounterIntensity()
		{
			return Math.Min(1.0f, debrisCount / (float)settings.worstDebrisCount);
		}

		/// <summary>
		/// See if we should spawn a new debris cloud encounter.
		/// </summary>
		/// <returns>
		/// True if we should spawn a new debris encounter.
		/// </returns>
		/// <param name='elapsed'>
		/// The time that has elapsed since the start of the last encounter.
		/// </param>
		public bool CheckForNewDebrisEncounter(double elapsed)
		{
			debrisCount = CountDebris();

			var probability = EncounterProbability(elapsed);
			if(probability == 0.0) //avoid the rng coming up with 0.0 too
				return false;

			var roll = UnityEngine.Random.value;
			LogFormatted_DebugOnly("Debris encounter roll {0} < probability {1}?",
			                       roll,
			                       probability);

			if(roll < probability)
				return true;

			return false;
		}

		/// <summary>
		/// Sets up a new debris cloud encounter.
		/// </summary>
		public void NewDebrisEncounter(bool forceIntensity = false)
		{
			// set origin
			// set duration
			// set severity

			debrisOrigin = new Vector3(RandomPlusOrMinus(), RandomPlusOrMinus(), RandomPlusOrMinus());
			debrisOrigin.Normalize();
			debrisOrigin *= 2000.0f;
			intensity = EncounterIntensity();
			intensity = forceIntensity ? 1.0f : intensity;
			duration = 30.0f;

			LogFormatted("Debris cloud encountered, intensity {0}, duration {1}", intensity, duration);

			eventStart = Planetarium.GetUniversalTime();

			// Avoid setting the repeat rate unnecessarily as it
			// restarts the worker
			if(this.RepeatingWorkerRate != repeatRate)
				SetRepeatRate(repeatRate);

			BlockTimeWarp();
		}

		/// <summary>
		/// Spawns the debris particles.
		/// </summary>
		/// <param name='elapsed'>
		/// The time that has elapsed since the start of the encounter.
		/// </param>
		public void SpawnDebrisParticles(double elapsed)
		{
			// Modify the number of debris particles spawned depending on if we are
			// at the beginning, middle or end of the encounter.
			// More in the middle, less at either end.
			var timeFromPeakIntensity = Math.Abs(duration/2.0f - elapsed);
			var spawnRateModifier = 1.0f - (timeFromPeakIntensity / (duration/2.0f));
			var spawnCount = (int)Math.Ceiling((maxSpawnCount * TimeWarp.CurrentRate * intensity) * spawnRateModifier);

			for(int i = 0; i < spawnCount; i++)
			{
				// Recycle old shrapnel particles using a queue so we don't put too much load
				// on the physics / rendering engines.
				GameObject shrap;
				if(shrapnel.Count < maxShrapnel)
				{
					shrap = GameObject.CreatePrimitive(PrimitiveType.Cube);
					shrap.name = "Kessler chaos debris";
					shrap.AddComponent ("Rigidbody");
					shrap.rigidbody.useGravity = false;
					shrap.rigidbody.mass = 0.035f;
					var collider = shrap.rigidbody.collider as BoxCollider;
					if(collider == null)
						LogFormatted("No box collider for kessler chaos shrapnel! You aren't going to get many collisions.");
					else
						collider.size *= 10.0f;
				}
				else
				{
					shrap = this.shrapnel.Dequeue();
					shrap.rigidbody.angularVelocity = Vector3.zero;
					shrap.transform.rotation = new Quaternion();
				}

				// get ship transform
				// set shrapnel transform to ship transform
				// move it away a distance
				// give it a shove towards the ship
				// splosions

				shrap.transform.position = FlightGlobals.ActiveVessel.transform.position + debrisOrigin;
				shrap.transform.LookAt(FlightGlobals.ActiveVessel.transform.position);

				// forward is (0, 0, 1)
				shrap.transform.Translate(
					RandomPlusOrMinus() * lateralSpread,
					RandomPlusOrMinus() * lateralSpread,
					UnityEngine.Random.value * longitudinalSpread);

				shrap.rigidbody.velocity = shrap.transform.TransformDirection(
					RandomPlusOrMinus() * lateralVelocitySpread,
					RandomPlusOrMinus() * lateralVelocitySpread,
					this.speed + RandomPlusOrMinus() * longitudinalVelocitySpread);

				shrap.rigidbody.angularVelocity = new Vector3(RandomPlusOrMinus(), RandomPlusOrMinus(), RandomPlusOrMinus());
				this.shrapnel.Enqueue(shrap);
			}
		}

		/// <summary>
		/// Counts the debris in the current sphere of influence.
		/// </summary>
		public int CountDebris()
		{
			var debris = FlightGlobals.Vessels.FindAll(
				x => x.vesselType == VesselType.Debris &&
				x.orbit.referenceBody.Equals(FlightGlobals.ActiveVessel.orbit.referenceBody) &&
				x.situation != Vessel.Situations.LANDED &&
				x.situation != Vessel.Situations.PRELAUNCH &&
				x.situation != Vessel.Situations.SPLASHED &&
				x.situation != Vessel.Situations.SUB_ORBITAL
				);

			return debris.Count;
		}

		internal override void DrawWindow (int id)
		{
			DragEnabled = true;
			ClampToScreen = true;
			TooltipsEnabled = false;

			var elapsed = Planetarium.GetUniversalTime() - eventStart;

			GUILayout.Label("Debris Cloud Forecast");
			GUILayout.BeginHorizontal(HighLogic.Skin.textArea);
			GUILayout.Label(string.Format("Probability {0:N0}%", EncounterProbability(elapsed)*100.0));
			GUILayout.Label(string.Format("Intensity {0:N0}%", EncounterIntensity()*100.0f));
			GUILayout.EndHorizontal();

			double minutesSinceLastEncounter = elapsed / 60.0f;
			string minutesSinceLastEncounterDescription = minutesSinceLastEncounter > 60.0 ? "∞" : string.Format("{0:N1}", minutesSinceLastEncounter);

			GUILayout.Label("Contributing factors");
			GUILayout.BeginVertical(HighLogic.Skin.textArea);
			GUILayout.Label(string.Format("Debris in orbit {0} / {1}", debrisCount, settings.worstDebrisCount));
			GUILayout.Label(string.Format("Altitude {0:N2}km", FlightGlobals.ship_altitude / 1000.0));
			GUILayout.Label(string.Format("Minutes since last encounter {0}", minutesSinceLastEncounterDescription));
			GUILayout.EndVertical();

			if(GUILayout.Button ("Trigger debris cloud"))
				NewDebrisEncounter(true);

			if(showGUIButton == null)
				GUILayout.Label("Install the Toolbar mod to hide this window");

#if DEBUG
			GUILayout.Label("debug only options");

			GUILayout.BeginHorizontal();
            GUILayout.Label("max spawn count");
            this.maxSpawnCount=Convert.ToInt32(GUILayout.TextField(this.maxSpawnCount.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("repeat rate");
            this.repeatRate=(float)Convert.ToDouble(GUILayout.TextField(this.repeatRate.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("speed");
            this.speed=(float)Convert.ToDouble(GUILayout.TextField(this.speed.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("longitudinal spread");
            this.longitudinalSpread=(float)Convert.ToDouble(GUILayout.TextField(this.longitudinalSpread.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("lateral spread");
            this.lateralSpread=(float)Convert.ToDouble(GUILayout.TextField(this.lateralSpread.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("longitudinal velocity spread");
            this.longitudinalVelocitySpread=(float)Convert.ToDouble(GUILayout.TextField(this.longitudinalVelocitySpread.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("lateral velocity spread");
            this.lateralVelocitySpread=(float)Convert.ToDouble(GUILayout.TextField(this.lateralVelocitySpread.ToString()));
            GUILayout.EndHorizontal();
#endif
        }

		public static float RandomPlusOrMinus()
		{
			return UnityEngine.Random.value - 0.5f;
		}

		public void BlockTimeWarp()
		{
			// Probably not multithreaded here but what the hell
			lock(this.blockTimeWarpLock)
			{
				if(this.blockingTimeWarp)
				{
					LogFormatted_DebugOnly("Already blocking time warp!");
					return;
				}

				timeWarpHighAllowedPreviously = HighLogic.CurrentGame.Parameters.Flight.CanTimeWarpHigh;
				this.blockingTimeWarp = true;
			}

			LogFormatted_DebugOnly("Blocking time warp");
			TimeWarp.SetRate(0, false);

			// This is a dirty hack and could conflict with other mods trying the same trick
			// Could also get stuck if the mod dies due to an exception
			HighLogic.CurrentGame.Parameters.Flight.CanTimeWarpHigh = false;
		}

		public void RestoreTimeWarp()
		{
			// Probably not multithreaded here but what the hell
			LogFormatted_DebugOnly("Restoring time warp");
			lock(this.blockTimeWarpLock)
			{
				if(!this.blockingTimeWarp)
				{
					LogFormatted_DebugOnly("Not currently blocking time warp!");
					return;
				}

				HighLogic.CurrentGame.Parameters.Flight.CanTimeWarpHigh = timeWarpHighAllowedPreviously;
				this.blockingTimeWarp = false;
			}
		}
    }
}