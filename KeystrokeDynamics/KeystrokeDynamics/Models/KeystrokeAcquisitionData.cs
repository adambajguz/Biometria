using System;

namespace KeystrokeDynamics.Models
{
    public class KeystrokeAcquisitionData
    {
        public char Key { get; set; }

        public DateTime KeyDownTimeStamp { get; set; }
        public DateTime KeyUpTimeStamp { get; set; }

        public KeystrokeData ToKeystrokeData()
        {
            return new KeystrokeData()
            {
                Key = Key,
                DwellTime = KeyUpTimeStamp - KeyDownTimeStamp
            };
        }
    }
}
