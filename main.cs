using System;
using KSP;
using UnityEngine;

namespace kesslerchaos
{
    [KSPAddon(KSPAddon.Startup.Flight,false)]
    public class MBExtended : MonoBehaviourExtended
    {
        internal override void Awake()
        {
            LogFormatted("Parent is awake");

            //Create a Child Object
            gameObject.AddComponent<MBExtendedChild>();

            //Start the repeating worker to fire x times each second
            StartRepeatingWorker(1);
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

				var shrapnel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				shrapnel.transform.position = FlightGlobals.ActiveVessel.transform.position;
				shrapnel.transform.Translate(10.0f, 0.0f, 0.0f);
				shrapnel.AddComponent ("Rigidbody");
				shrapnel.rigidbody.useGravity = false;
				shrapnel.rigidbody.velocity = new Vector3(-1.0f, 0.0f, 0.0f);
			}
			catch(Exception e)
			{
				LogFormatted_DebugOnly("You fucked up: {0}", e);
				throw;
			}
        }
    }

    public class MBExtendedChild : MonoBehaviourExtended
    {
        internal override void Awake()
        {
            LogFormatted("Child is awake");
        }
    }
}