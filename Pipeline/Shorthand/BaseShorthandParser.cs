using System;
using Pipeline.Configuration;

namespace Pipeline.Shorthand {
    public class BaseShorthandParser {
        public static Transform DefaultConfiguration(Action<Transform> setter) {
            return new Transform().GetDefaultOf(setter);
        }

        public static Transform Guard() {
            return new Transform().GetDefaultOf<Transform>(t => { t.Method = "guard"; });
        }
    }
}