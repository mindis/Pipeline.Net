using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    public abstract class BaseTransform {

        public string Name { get; set; }

        public Process Process { get; private set; }
        public Entity Entity { get; private set; }
        public Field Field { get; private set; }
        public Transform Configuration { get; private set; }
        public PipelineContext Context { get; private set; }

        protected BaseTransform(Process process, Entity entity, Field field, Transform transform) {
            Process = process;
            Entity = entity;
            Field = field;
            Configuration = transform;
            Context = new PipelineContext(process, entity, field, transform);
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <returns></returns>
        public List<Field> ParametersToFields() {

            var fields = Configuration.Parameters
                .Where(p => p.Field != string.Empty)
                .Select(p =>
                    Entity == null ?
                    Process.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field) :
                    Entity.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field)
                ).ToList();

            if (!fields.Any()) {
                fields.Add(Field);
            }
            return fields;
        }

        public static Transform DefaultConfiguration(Action<Transform> setter) {
            return new Transform().GetDefaultOf(setter);
        }

        public static Transform Guard() {
            return new Transform().GetDefaultOf<Transform>(t => { t.Method = "guard"; });
        }

        public static Transform Parameterless(string method, string result, string args, List<string> problems) {
            if (!string.IsNullOrEmpty(args)) {
                problems.Add(string.Format("The {0} transform does not take parameters. It returns a {1} version of the value or values in the field. To get data into the field, proceed {0}() with copy(f1) or copy(f1,f2,etc) short-hand method.", method, result));
                return Guard();
            }

            return DefaultConfiguration(t => {
                t.Method = method;
                t.IsShortHand = true;
            });
        }

        public static string[] SplitArguments(string arg, int skip = 0) {
            return Shared.Split(arg, ",", skip);
        }

    }
}