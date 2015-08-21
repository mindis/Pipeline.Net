using Pipeline.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Shorthand;

namespace Pipeline.Configuration {
   public class Field : CfgNode, IField {

      static readonly List<string> _invalidNames = new List<string>() { "tflhashcode", "tflbatchid", "tflkey" };

      static readonly Dictionary<string, Func<string, object>> ConversionMap = new Dictionary<string, Func<string, object>> {
            {"string", (x => x)},
            {"int16", (x => System.Convert.ToInt16(x))},
            {"short", (x => System.Convert.ToInt16(x))},
            {"int32", (x => System.Convert.ToInt32(x))},
            {"int", (x => System.Convert.ToInt32(x))},
            {"int64", (x => System.Convert.ToInt64(x))},
            {"long", (x => System.Convert.ToInt64(x))},
            {"uint16", (x => System.Convert.ToUInt16(x))},
            {"uint32", (x => System.Convert.ToUInt32(x))},
            {"uint64", (x => System.Convert.ToUInt64(x))},
            {"double", (x => System.Convert.ToDouble(x))},
            {"decimal", (x => Decimal.Parse(x, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo))))},
            {"char", (x => System.Convert.ToChar(x))},
            {"datetime", (x => System.Convert.ToDateTime(x))},
            {"boolean", (x => System.Convert.ToBoolean(x))},
            {"single", (x => System.Convert.ToSingle(x))},
            {"real", (x => System.Convert.ToSingle(x))},
            {"float", (x => System.Convert.ToSingle(x))},
            {"guid", (x => Guid.Parse(x))},
            {"byte", (x => System.Convert.ToByte(x))},
            {"byte[]", (HexStringToByteArray)}
        };

      string _type;
      string _length;

      /// <summary>
      /// Optional.  Default is `false`
      /// 
      /// Usually a field is set to a default if it is NULL.  If set to true, the default will overwrite blank values as well.
      /// 
      ///     <add name="Name" default="None" default-blank="true" />
      /// </summary>
      [Cfg(value = false)]
      public bool DefaultBlank { get; set; }

      /// <summary>
      /// Optional. Default to `false`
      /// 
      /// Usually a field is set to a default if it is NULL.  If this is set to true, the default will overwrite empty values as well (same as DefaultBlank).
      /// 
      ///     <add name="Name" default="None" default-empty="true" />
      /// </summary>
      [Cfg(value = false)]
      public bool DefaultEmpty { get; set; }

      /// <summary>
      /// Optional. Default to `false`
      /// 
      /// Usually a field is set to a default if it is NULL.  If this is set to true, the default will overwrite white-space values as well.
      /// 
      ///     <add name="Name" default="None" default-empty="true" default-white-space="true" />
      /// </summary>
      [Cfg(value = false)]
      public bool DefaultWhiteSpace { get; set; }

      /// <summary>
      /// Optional. Default is `false`
      /// 
      /// Used in conjunction with count, join, and concat aggregate functions.
      /// </summary>
      [Cfg(value = false)]
      public bool Distinct { get; set; }

      /// <summary>
      /// Optional. Default is `true`
      /// 
      /// Indicates a value is expected from the source (or *input*).
      /// </summary>
      [Cfg(value = true)]
      public bool Input { get; set; }

      /// <summary>
      /// Optional. Default is `false`
      /// 
      /// Used when importing delimited files.  Fields at the end of a line may be marked as optional.
      /// </summary>
      [Cfg(value = false)]
      public bool Optional { get; set; }

      /// <summary>
      /// Optional. Default is `true`
      /// 
      /// Indicates this field is *output* to the defined connection (or destination).
      /// </summary>
      [Cfg(value = true)]
      public bool Output { get; set; }

      /// <summary>
      /// Optional. Default is `false`
      /// 
      /// Indicates this field is (or is part of) the entity's primary key (or unique identifier).
      /// </summary>
      [Cfg(value = false)]
      public bool PrimaryKey { get; set; }

      /// <summary>
      /// Optional. Default is `false`
      /// 
      /// Used to tell rendering tools that the contents of this field should not be encoded.  The contents should be rendered raw (un-touched).
      /// For example: This is a useful indicator if you've created HTML content, and you don't want something encoding it later.
      /// </summary>
      [Cfg(value = false)]
      public bool Raw { get; set; }

      /// <summary>
      /// Optional. Default is `true`
      /// 
      /// Used when fields are defined inside a `fromXml` transform.  In this type of transform, the field's name corresponds to an element's name.
      /// If this setting is true, `<Name>Contents</Name>` yields `Contents` (what's inside the element)
      /// If false, `<Name>Contents</Name>` yields `<Name>Contents</Name>` (the element and what's inside the element)
      /// </summary>
      [Cfg(value = true)]
      public bool ReadInnerXml { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// Used when importing delimited files.  Some formats, like .csv for example, use a character to quote string (or text) values.
      /// 
      ///     <add name="Name" type="string" quoted-with="&quot;" />
      /// 
      /// </summary>
      [Cfg(value = default(char))]
      public char QuotedWith { get; set; }

      /// <summary>
      /// Optional
      /// 
      /// Used to over-ride the default ordering of the fields.
      /// 
      ///     <add name="Name" index="0" />
      /// </summary>
      [Cfg(value = short.MaxValue)]
      public short Index { get; set; }

      /// <summary>
      /// Optional. Default is `18`
      /// </summary>
      [Cfg(value = 18)]
      public int Precision { get; set; }

      /// <summary>
      /// Optional. Default is `9`
      /// </summary>
      [Cfg(value = 9)]
      public int Scale { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// An aggregate function is only applicable when entity is set to group (e.g. `group="true"`). Note: When the entity is set to group, all output fields must have an aggregate function defined.
      /// 
      /// * array
      /// * concat
      /// * count
      /// * first
      /// * group
      /// * join
      /// * last
      /// * max
      /// * maxlength
      /// * min
      /// * minlength
      /// * sum
      /// 
      /// </summary>
      [Cfg(value = "last", domain = "array,concat,count,first,group,join,last,max,maxlength,min,minlength,sum", toLower = true)]
      public string Aggregate { get; set; }
      string alias;

      /// <summary>
      /// Optional
      /// 
      /// The name should always correspond with the input field's name.  Alias is used to rename it to something 
      /// else.  An alias must be unique across the entire process.  The only exception to this rule is when the 
      /// field is a primary key that is related to another entity's foreign key (of the same name).
      /// </summary>
      [Cfg(value = "", required = false, unique = true)]
      public string Alias {
         get {
            return alias;
         }
         set {
            if (value != null) {
               if (value != string.Empty && _invalidNames.Contains(value.ToLower())) {
                  Error("You may not alias fields TflHashCode, TflBatchId, or TflKey.  These are reserved words.  If you have fields named any of these, you must alias them to something different.");
               }
               alias = value;
            }
         }
      }

      /// <summary>
      /// Optional. The default varies based on type.
      /// 
      /// This value overwrites NULL values so you don't have to worry about NULLs in the pipeline.  It can also be configured to overwrite blanks 
      /// and white-space by other attributes. 
      /// </summary>
      [Cfg(value = Constants.DefaultSetting)]
      public string Default { get; set; }

      /// <summary>
      /// Optional.  Default is `, `
      /// 
      /// Used in join aggregations.  Note: This only takes affect when the entity has the `group="true"` attribute set.
      /// 
      ///     <add name="Name" delimiter="|" aggregate="join" />
      /// </summary>
      [Cfg(value = ", ")]
      public string Delimiter { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// A field's label.  A label is a more descriptive name that can contain spaces, etc.  Used by user interface builders.
      /// </summary>
      [Cfg(value = "")]
      public string Label { get; set; }

      /// <summary>
      /// Optional. Default is `64`
      /// 
      /// This is the maximum length allowed for a field.  Any content exceeding this length will be truncated. 
      /// Note: A warning is issued in the logs when this occurs, so you can increase the length if necessary.
      /// </summary>
      [Cfg(value = "64", toLower = true)]
      public string Length {
         get { return _length; }
         set {
            if (value == null)
               return;
            int number;
            if (int.TryParse(value, out number)) {
               if (number <= 0) {
                  Error("A field's length must be a number greater than zero, or max.");
               }
            } else {
               if (!value.Equals("max", StringComparison.OrdinalIgnoreCase)) {
                  Error("A field's length must be a number greater than zero, or max.");
               }
            }
            _length = value;
         }
      }

      /// <summary>
      /// **Required**
      /// 
      ///     <add name="Name" />
      /// </summary>
      [Cfg(required = true)]
      public string Name { get; set; }

      /// <summary>
      /// Optional. Default is `element`
      /// 
      /// Used when fields are defined inside a `fromXml` transform.  In this type of transform, 
      /// the field's name corresponds to an *element*'s name or an *attribute*'s name.
      /// </summary>
      [Cfg(value = "element")]
      public string NodeType { get; set; }

      /// <summary>
      /// Optional.  Default is `default`
      /// 
      /// Used with search engine outputs like Lucene, Elasticsearch, and SOLR.  Corresponds to a defined search type.
      /// 
      ///     <add name="Name" search-type="keyword" />
      /// </summary>
      [Cfg(value = "default")]
      public string SearchType { get; set; }

      /// <summary>
      /// Optional
      /// 
      /// * asc
      /// * desc
      /// </summary>
      [Cfg(value = "none", domain = "asc,desc,none", toLower = true)]
      public string Sort { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// An alternate (shorter) way to define simple transformations.
      /// 
      ///     <add name="Name" t="trim()" />
      /// </summary>
      [Cfg(value = "", shorthand = true)]
      public string T { get; set; }

      /// <summary>
      /// Optional.  Default is `string`
      /// 
      /// This may be one of these types:
      /// 
      /// * bool
      /// * boolean
      /// * byte
      /// * byte[]
      /// * char
      /// * date
      /// * datetime
      /// * decimal
      /// * double
      /// * float
      /// * guid
      /// * int
      /// * int16
      /// * int32
      /// * int64
      /// * long
      /// * object
      /// * real
      /// * rowversion
      /// * short
      /// * single
      /// * string
      /// * uint64
      /// * xml
      /// </summary>
      [Cfg(value = "string", domain = Constants.TypeDomain, toLower = true)]
      public string Type {
         get { return _type; }
         set {
            if (value == null)
               return;

            if (value == "date") {
               _type = "datetime";
            } else {
               _type = value.StartsWith("system.") ? value.Replace("system.", string.Empty) : value;
            }
         }
      }

      [Cfg(value = true)]
      public bool Unicode { get; set; }

      [Cfg(value = true)]
      public bool VariableLength { get; set; }

      //lists
      [Cfg()]
      public List<NameReference> SearchTypes { get; set; }
      [Cfg()]
      public List<Transform> Transforms { get; set; }
      [Cfg()]
      public List<string> Domain { get; set; }

      /// <summary>
      /// Default is `false`.
      /// 
      /// If true, the contents of this field are copied into master.
      /// </summary>
      [Cfg(value = false)]
      public bool Denormalize { get; set; }

      /// <summary>
      /// Set by Process.ModifyKeys for keyed dependency injection
      /// </summary>
      public string Key { get; set; }

      /// <summary>
      /// Set by Process.ModifyKeyTypes
      /// </summary>
      public KeyType KeyType { get; set; }
      public short EntityIndex { get; internal set; }
      public short StringIndex { get; internal set; }
      public short MasterIndex { get; set; }

      //custom
      protected override void PreValidate() {

         if (string.IsNullOrEmpty(Alias)) { Alias = Name; }

         if (Label == string.Empty) { Label = Alias; }

         if (Type != "string") { DefaultBlank = true; }

         if (Type == "rowversion") { Length = "8"; }

         if (PrimaryKey && !Output) {
            Warn("Primary Keys must be output. Overriding output to true for {0}.", Alias);
            Output = true;
         }

         if (Denormalize && !Output) {
            Warn("Denormailized fields must be output.  Overriding output to true for {0}.", Alias);
            Output = true;
         }
      }

      static byte[] HexStringToByteArray(string hex) {
         var bytes = new byte[hex.Length / 2];
         var hexValue = new[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

         for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1) {
            bytes[x] = (byte)(hexValue[Char.ToUpper(hex[i + 0]) - '0'] << 4 |
                              hexValue[Char.ToUpper(hex[i + 1]) - '0']);
         }
         return bytes;
      }

      static string BytesToHexString(byte[] bytes) {
         var c = new char[bytes.Length * 2];
         for (var i = 0; i < bytes.Length; i++) {
            var b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
         }
         return new string(c);
      }

      public object Convert(string value) {
         return Type == "string" ? value : ConversionMap[Type](value);
      }

      public void ExpandShortHandTransforms() {

         if (RequiresCopyParameters()) {
            var expression = new Expressions(T).First();
            var first = Transforms.First();
            foreach (var p in expression.Parameters) {
               var parameter = first.GetDefaultOf<Parameter>();
               if (p.Contains(":")) {
                  //named values
                  var named = p.Split(':');
                  parameter.Name = named[0];
                  parameter.Value = named[1];
               } else if (p.Contains(".")) {
                  // entity, field combinations
                  var dotted = p.Split('.');
                  parameter.Entity = dotted[0];
                  parameter.Field = dotted[1];
               } else {
                  parameter.Field = p; // just fields
               }
               first.Parameters.Add(parameter);
            }
         }

         // e.g. t="copy(x).is(int).between(3,5), both is() and between() should refer to x.
         if (RequiresCompositeValidator()) {
            var first = Transforms.First();
            foreach (var transform in Transforms.Skip(1)) {
               transform.Parameter = transform.Parameter == string.Empty ? first.Parameter : transform.Parameter;
               transform.Parameters = transform.Parameters.Count == 0 ? first.Parameters : transform.Parameters;
            }
         }
      }

      bool RequiresCopyParameters() {
         return T.StartsWith("copy(", StringComparison.Ordinal) && Transforms.Any();
      }

      internal bool Is(string type) {
         return type == Type;
      }

      public bool RequiresCompositeValidator() {
         return Transforms.Count > 1 && Transforms.All(t => t.IsValidator());
      }

      public string FieldName() {
         return Constants.GetExcelName(EntityIndex) + (Index + 1);
      }

      public override string ToString() {
         return string.Format("{0}:{1}", Alias, Type);
      }

   }
}