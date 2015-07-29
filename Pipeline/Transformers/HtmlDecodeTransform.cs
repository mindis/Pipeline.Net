using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipeline.Configuration;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Transformers {
   /// <summary>
   /// Default (portable) XML,HTML Decode borrows from Cfg-Net
   /// </summary>
   public class HtmlDecodeTransform : BaseTransform, ITransform {
      readonly Field _input;
      readonly StringBuilder _builder;

      public HtmlDecodeTransform(PipelineContext context)
            : base(context) {
         _input = SingleInput();
         _builder = new StringBuilder();
      }

      public Row Transform(Row row) {
         row.SetString(Context.Field, CfgNode.Decode(row.GetString(_input), _builder));
         Increment();
         return row;
      }

   }
}