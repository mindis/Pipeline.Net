using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class BaseTransform {

        public string Name { get; set; }
        public Process Process { get; set; }
        public Entity Entity { get; set; }
        public Field Field { get; set; }

        public BaseTransform(Process process, Entity entity, Field field) {
            Process = process;
            Entity = entity;
            Field = field;
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public List<Field> ParametersToFields(Transform transform) {

            var fields = transform.Parameters
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

        public static Transform Configuration(Action<Transform> setter) {
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

            return Configuration(t => {
                t.Method = method;
                t.IsShortHand = true;
            });
        }

        public static string[] SplitArguments(string arg, int skip = 0) {
            return Shared.Split(arg, ",", skip);
        }

    }
}