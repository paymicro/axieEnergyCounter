using System;

namespace axieEnergyCounter
{
    public class CounterEventArgs: EventArgs
    {
        public int Change { get; set; }

        public int Current { get; set; }
    }
}
