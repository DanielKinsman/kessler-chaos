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
    [KSPAddon(KSPAddon.Startup.Flight,false)]
    public class MBExtended : MonoBehaviourWindow
    {
		public bool fire = false;
		public float speed = 500.0f;
		public float longitudinalSpread = 1000.0f;
		public float lateralSpread = 200.0f;
		public float longitudinalVelocitySpread = 250.0f;
		public float lateralVelocitySpread = 100.0f;
		public int maxSpawnCount = 25;
		public float repeatRate = 0.25f;
		public float idleRepeatRate = 60.0f;
		public int maxShrapnel = 500;
		private Queue<GameObject> shrapnel;
		public Vector3 debrisOrigin;
		public float duration = 30.0f;
		public DateTime eventStart;
		public float intensity = 1.0f;

        internal override void Awake()
        {
			WindowCaption = "Kessler Chaos";
            WindowRect = new Rect(0, 0, 250, 50);
            Visible = true;

			SetRepeatRate(idleRepeatRate);
            StartRepeatingWorker();
			shrapnel = new Queue<GameObject>(maxShrapnel);
			eventStart = DateTime.MinValue;
        }

		internal override void OnDestroy()
		{
			base.OnDestroy();
			this.shrapnel.Clear();
		}

        internal override void RepeatingWorker()
        {
			try
			{
				//todo use simulation time not wall clock time
				var elapsed = DateTime.UtcNow - eventStart;
				if(elapsed.TotalSeconds > this.duration)
				{
					// Avoid settign the repeat rate unnecessarily as it
					// restarts the worker
					if(this.RepeatingWorkerRate != idleRepeatRate)
						SetRepeatRate(idleRepeatRate);

					// todo clean up shrapnel

					return;
				}

				// Modify the number of debris particles spawned depending on if we are
				// at the beginning, middle or end of the encounter.
				// More in the middle, less at either end.
				var timeFromPeakIntensity = Math.Abs(duration/2.0f - elapsed.TotalSeconds);
				var spawnRateModifier = 1.0f - (timeFromPeakIntensity / (duration/2.0f));
				var spawnCount = (int)Math.Ceiling((maxSpawnCount * intensity) * spawnRateModifier);

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
						shrap.rigidbody.mass = 0.03f;
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
			catch(Exception e)
			{
				LogFormatted_DebugOnly("You fucked up: {0}", e);
				throw;
			}
        }

		/// <summary>
		/// Sets up a new debris cloud encounter.
		/// </summary>
		public void NewEvent()
		{
			// set origin
			// set duration
			// set severity

			debrisOrigin = new Vector3(RandomPlusOrMinus(), RandomPlusOrMinus(), RandomPlusOrMinus());
			debrisOrigin.Normalize();
			debrisOrigin *= 2000.0f;
			intensity = 1.0f;
			duration = 30.0f;
			eventStart = DateTime.UtcNow;

			// Avoid settign the repeat rate unnecessarily as it
			// restarts the worker
			if(this.RepeatingWorkerRate != repeatRate)
				SetRepeatRate(repeatRate);
		}

		/// <summary>
		/// Counts the debris in the current sphere of influence.
		/// </summary>
		public int CountDebris ()
		{
			// todo implement
			return 0;
		}

		internal override void DrawWindow (int id)
		{
			DragEnabled = true;
			ClampToScreen = true;
			TooltipsEnabled = false;

			if (GUILayout.Button ("Fire!"))
				NewEvent();

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
        }

		public static float RandomPlusOrMinus()
		{
			return UnityEngine.Random.value - 0.5f;
		}
    }
}