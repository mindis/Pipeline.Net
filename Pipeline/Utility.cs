using System;
using System.Globalization;
using System.Linq;

namespace Pipeline {

    public static class Shared {
        /// <summary>
        /// Splits a sting by a splitter (aka delimiter), 
        /// but first escapes any splitters prefixed with a forward slash.
        /// </summary>
        /// <param name="arg">arguments</param>
        /// <param name="splitter">the splitter (aka delimiter)</param>
        /// <param name="skip">An optional number of post-split elements to skip over.</param>
        /// <returns>properly split strings</returns>
        public static string[] Split(string arg, string splitter, int skip = 0) {
            if (arg.Equals(String.Empty))
                return new string[0];

            var placeHolder = arg.GetHashCode().ToString(CultureInfo.InvariantCulture);
            var split = arg.Replace("\\" + splitter, placeHolder).Split(new[] { splitter }, StringSplitOptions.None);
            return split.Select(s => s.Replace(placeHolder, splitter.ToString())).Skip(skip).ToArray();
        }
    }
}
