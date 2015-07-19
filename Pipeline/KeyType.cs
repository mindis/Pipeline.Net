using System;

namespace Pipeline {
    [Flags]
    public enum KeyType : short {
        None = 0,
        Foreign = 1,
        Primary = 2
    }
}