using Cfg.Net;

namespace Pipeline.Configuration {
    public class Filter : CfgNode {
        string _continuation;

        /// <summary>
        /// Optional.  Default is `And`
        /// 
        /// A continuation operator.  Valid values are:
        /// 
        /// * And
        /// * Or
        /// </summary>
        [Cfg(value = "And", domain = "And,Or", ignoreCase = true)]
        public string Continuation {
            get { return _continuation; }
            set {
                if (value != null)
                    _continuation = value.ToUpper();
            }
        }

        /// <summary>
        /// Optional
        /// 
        /// A free-form, un-checked expression.  This is passed directly into a generated query.
        /// </summary>
        [Cfg(value = "")]
        public string Expression { get; set; }

        /// <summary>
        /// Optional
        /// 
        /// A reference to an entity field's name\alias or a literal on the left side of an expression.
        /// </summary>
        [Cfg(value = "")]
        public string Left { get; set; }

        /// <summary>
        /// Optional
        /// 
        /// A reference to an entity field's name\alias or a literal on the right side of an expression.
        /// </summary>
        [Cfg(value = "")]
        public string Right { get; set; }

        /// <summary>
        /// Optional.  Default is `Equal`
        /// 
        /// A comparison operator.  Valid values are:
        /// 
        /// * Equal
        /// * GreaterThan
        /// * GreaterThanEqual 
        /// * LessThan
        /// * LessThanEqual
        /// * NotEqual
        /// </summary>
        [Cfg(value = "Equal", domain = Constants.ComparisonDomain, toLower = true, ignoreCase = true)]
        public string Operator { get; set; }

        public bool LeftIsField { get; set; }
        public bool RightIsField { get; set; }
        public Field LeftField { get; set; }
        public Field RightField { get; set; }
    }
}