using System.Collections.Generic;

namespace KeystrokeDynamics.Storage.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }

        public Dictionary<char, long> KeystrokeVector { get; set; } = new Dictionary<char, long>();
    }
}
