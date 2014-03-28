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
		public float speed = -500.0f;
		public float longitudinalSpread = 1000.0f;
		public float lateralSpread = 100.0f;
		public float longitudinalVelocitySpread = 400.0f;
		public float lateralVelocitySpread = 100.0f;
		public int spawnCount = 25;
		public float repeatRate = 0.25f;
		public int maxShrapnel = 500;
		private Queue<GameObject> shrapnel;

        internal override void Awake()
        {
			WindowCaption = "Kessler Chaos";
            WindowRect = new Rect(0, 0, 250, 50);
            Visible = true;

			SetRepeatRate(repeatRate);
            StartRepeatingWorker();
			shrapnel = new Queue<GameObject>(maxShrapnel);
        }

        internal override void RepeatingWorker()
        {
			try
			{
				// get ship transform
				// set shrapnel transform to ship transform
				// move it away a distance
				// give it a shove towards the ship
				// splosions

				if(!this.fire)
					return;

				for(int i = 0; i < spawnCount; i++)
				{
					GameObject shrap;
					if(shrapnel.Count < maxShrapnel)
					{
						shrap = GameObject.CreatePrimitive(PrimitiveType.Cube);
						shrap.AddComponent ("Rigidbody");
						shrap.rigidbody.useGravity = false;
					}
					else
					{
						shrap = this.shrapnel.Dequeue();
					}

					shrap.transform.position = FlightGlobals.ActiveVessel.transform.position;
					shrap.transform.Translate(500.0f + UnityEngine.Random.value*longitudinalSpread, (UnityEngine.Random.value-0.5f)*lateralSpread, (UnityEngine.Random.value-0.5f)*lateralSpread);
					shrap.rigidbody.velocity = new Vector3(this.speed + (UnityEngine.Random.value-0.5f)*longitudinalVelocitySpread, (UnityEngine.Random.value-0.5f)*lateralVelocitySpread, (UnityEngine.Random.value-0.5f)*lateralVelocitySpread);
					this.shrapnel.Enqueue(shrap);
				}

				//todo taper spawn rate at start and end of event
				//todo smaller cone for rigidbodies, purely graphical ones further out
				//todo make sure we free up memory on shutdown
			}
			catch(Exception e)
			{
				LogFormatted_DebugOnly("You fucked up: {0}", e);
				throw;
			}
        }

		internal override void DrawWindow(int id)
        {
			DragEnabled = true;
            ClampToScreen = true;
			TooltipsEnabled = false;

			if(GUILayout.Button("Fire!"))
				this.fire = !this.fire;

			GUILayout.Label(String.Format("Fire? {0}", this.fire.ToString()));

			GUILayout.BeginHorizontal();
            GUILayout.Label("spawn count");
            this.spawnCount=Convert.ToInt32(GUILayout.TextField(this.spawnCount.ToString()));
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
            GUILayout.Label("repeat rate");
            this.repeatRate=(float)Convert.ToDouble(GUILayout.TextField(this.repeatRate.ToString()));
			if(this.repeatRate != this.RepeatingWorkerRate)
				SetRepeatRate(repeatRate);
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
    }
}