using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Extensions;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    public abstract class BaseTransform {
        private long _rowCount;

        public PipelineContext Context { get; private set; }
        public IPipelineLogger Logger { get; set; }

        protected BaseTransform(PipelineContext context) {
            Context = context;
        }

        public long RowCount {
            get { return _rowCount; }
            set { _rowCount = value; }
        }

        protected virtual void Increment() {
            _rowCount++;
            if (_rowCount % Context.Entity.LogInterval == 0) {
                Logger.Info(Context, _rowCount.ToString());
            }
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <returns></returns>
        private List<Field> ParametersToFields() {

            var fields = Context.Transform.Parameters
                .Where(p => p.Field != string.Empty)
                .Select(p =>
                    Context.Entity == null ?
                    Context.Process.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field) :
                    Context.Entity.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field)
                ).ToList();

            if (!fields.Any()) {
                fields.Add(Context.Field);
            }
            return fields;
        }

        public Field SingleInput() {
            return ParametersToFields().First();
        }

        /// <summary>
        /// Only used with producers, see Transform.Producers()
        /// </summary>
        /// <returns></returns>
        public Field SingleInputForMultipleOutput() {
            if (Context.Transform.Parameter != string.Empty) {
                return Context.Entity == null
                    ? Context.Process.GetAllFields()
                        .First(f => f.Alias == Context.Transform.Parameter || f.Name == Context.Transform.Parameter)
                    : Context.Entity.GetAllFields()
                        .First(f => f.Alias == Context.Transform.Parameter || f.Name == Context.Transform.Parameter);
            }
            return Context.Field;
        }

        public Field[] MultipleInput() {
            return ParametersToFields().ToArray();
        }

        public Field[] MultipleOutput() {
            return ParametersToFields().ToArray();
        }

        public static Transform DefaultConfiguration(Action<Transform> setter) {
            return new Transform().GetDefaultOf(setter);
        }

        public static Transform Guard() {
            return new Transform().GetDefaultOf<Transform>(t => { t.Method = "guard"; });
        }

        //public static Transform Parameterless(string method, string result, string args, List<string> problems) {
        //    if (!string.IsNullOrEmpty(args)) {
        //        problems.Add(string.Format("The {0} transform does not take parameters. It returns a {1} version of the value or values in the field. To get data into the field, proceed {0}() with copy(f1) or copy(f1,f2,etc) short-hand method.", method, result));
        //        return Guard();
        //    }

        //    return DefaultConfiguration(t => {
        //        t.Method = method;
        //        t.IsShortHand = true;
        //    });
        //}

        public static Transform Pad(string method, string arg, List<string> problems) {

            var split = SplitArguments(arg);

            if (split.Length < 2) {
                problems.Add(string.Format("The {0} method requires two pararmeters: the total width, and the padding character(s).  You've provided {1} parameter{2}.", method, split.Length, split.Length.Plural()));
                return Guard();
            }

            var element = DefaultConfiguration(t => {
                t.Method = method;
                t.IsShortHand = true;
            });

            int totalWidth;
            if (int.TryParse(split[0], out totalWidth)) {
                element.TotalWidth = totalWidth;
            } else {
                problems.Add(string.Format("The {0} method requires the first parameter to be total width; an integer. {1} is not an integer", method, split[0]));
                return Guard();
            }

            element.PaddingChar = split[1][0];

            if (element.PaddingChar == default(char)) {
                problems.Add(string.Format("The {0} method's padding character must be provided.", method));
                return Guard();
            }

            return element;
        }

        public static string[] SplitArguments(string arg, int skip = 0) {
            return Shared.Split(arg, ",", skip);
        }
    }
}