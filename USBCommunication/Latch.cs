using Spring.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovoCommunication.USBCommunication
{
    public class Latch : ISync
    {
        /// <summary>
        /// can acquire ?
        /// </summary>
        protected bool latched_ = false;

        /// <summary>
        /// Method mainly used by clients who are trying to get the latch
        /// </summary>
        public void Acquire()
        {
            lock (this)
            {
                while (!latched_)
                {
                    Monitor.Wait(this);
                }
            }
        }

        /// <summary>Wait at most msecs millisconds for a permit</summary>
        public bool Attempt(long msecs)
        {
            lock (this)
            {
                if (latched_)
                {
                    return true;
                }
                else if (msecs <= 0)
                {
                    return false;
                }
                else
                {
                    long waitTime = msecs;
                    //double start = new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds;
                    double start = Utils.CurrentTimeMillis;
                    for (;;)
                    {
                        Monitor.Wait(this, TimeSpan.FromMilliseconds(waitTime));
                        if (latched_)
                        {
                            return true;
                        }
                        else
                        {
                            waitTime = (long)(msecs - (Utils.CurrentTimeMillis - start));
                            if (waitTime <= 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enable all current and future acquires to pass 
        /// </summary>
        public void Release()
        {
            lock (this)
            {
                latched_ = true;
                Monitor.PulseAll(this);
            }
        }
    }
}
