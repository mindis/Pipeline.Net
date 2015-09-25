using System;
using System.Globalization;
using System.Text;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using System.Linq;

namespace Pipeline.Provider.SqlServer {

    public static class SqlFilterExtensions {
        public static string ResolveFilter(this IContext c) {

            foreach (var filter in c.Entity.Filter) {
                Field field;
                if (c.Entity.TryGetField(filter.Left, out field)) {
                    filter.LeftField = field;
                    filter.LeftIsField = true;
                }
                if (c.Entity.TryGetField(filter.Right, out field)) {
                    filter.RightField = field;
                    filter.RightIsField = true;
                }
            }

            var builder = new StringBuilder("(");
            var last = c.Entity.Filter.Count - 1;

            for (var i = 0; i < c.Entity.Filter.Count; i++) {
                var filter = c.Entity.Filter[i];
                builder.Append(ResolveExpression(c, filter));
                if (i >= last) continue;
                builder.Append(" ");
                builder.Append(filter.Continuation);
                builder.Append(" ");
            }

            builder.Append(")");
            return builder.ToString();
        }

        private static string ResolveExpression(this IContext c, Filter filter) {
            if (!string.IsNullOrEmpty(filter.Expression))
                return filter.Expression;

            var builder = new StringBuilder();
            var rightSide = ResolveSide(filter, "right");
            var resolvedOperator = ResolveOperator(filter.Operator);
            if (rightSide.Equals("NULL")) {
                if (filter.Operator == "=") {
                    resolvedOperator = "IS";
                }
                if (filter.Operator == "!=") {
                    resolvedOperator = "IS NOT";
                }
            }

            builder.Append(ResolveSide(filter, "left"));
            builder.Append(" ");
            builder.Append(resolvedOperator);
            builder.Append(" ");
            builder.Append(rightSide);

            var expression = builder.ToString();
            c.Info("Filter: {0}", expression);
            return expression;
        }

        private static string ResolveSide(Filter filter, string side) {

            bool isField;
            string value;
            bool otherIsField;
            Field otherField;

            if (side == "left") {
                isField = filter.LeftIsField;
                value = filter.Left;
                otherIsField = filter.RightIsField;
                otherField = filter.RightField;
            } else {
                isField = filter.RightIsField;
                value = filter.Right;
                otherIsField = filter.LeftIsField;
                otherField = filter.LeftField;
            }

            if (isField)
                return SqlConstants.L + value + SqlConstants.R;

            if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                return "NULL";

            if (!otherIsField) {
                double number;
                if (double.TryParse(value, out number)) {
                    return number.ToString(CultureInfo.InvariantCulture);
                }
                return SqlConstants.T + value + SqlConstants.T;
            }

            if (SqlConstants.StringTypes.Any(st => st == otherField.Type)) {
                return SqlConstants.T + value + SqlConstants.T;
            }
            return value;
        }

        private static string ResolveOperator(string op) {
            switch (op) {
                case "equal":
                    return "=";
                case "greaterthan":
                    return ">";
                case "greaterthanequal":
                    return ">=";
                case "lessthan":
                    return "<";
                case "lessthanequal":
                    return "<=";
                case "notequal":
                    return "!=";
                default:
                    return "=";
            }

        }
    }
}