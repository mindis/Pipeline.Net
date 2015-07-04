using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class BaseTransformer {

        public Process Process { get; set; }
        public Entity Entity { get; set; }
        public Field Field { get; set; }
        public Transform Transform { get; set; }

        public BaseTransformer(Process process, Entity entity, Field field) {
            Process = process;
            Entity = entity;
            Field = field;
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public List<Field> GetInput(Transform transform) {

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

        public static string[] SplitArguments(string arg, int skip = 0) {
            return Shared.Split(arg, ",", skip);
        }

    }
}