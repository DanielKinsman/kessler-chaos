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

            //Start the repeating worker to fire once each second
            StartRepeatingWorker(1);
        }

        internal override void RepeatingWorker()
        {
			try
			{
	            LogFormatted("Last RepeatFunction Ran for: {0}ms",RepeatingWorkerDuration.TotalMilliseconds);
	            LogFormatted("UT Since Last RepeatFunction: {0}",RepeatingWorkerUTPeriod);
			}
			catch(Exception e)
			{
				LogFormatted("You fucked up dan: {0}", e);
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