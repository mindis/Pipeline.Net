using System.Collections.Generic;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    /// <summary>
    /// all transformers should implement this, they need to transform the data and Increment()
    /// </summary>
    public interface ITransform {

        IPipelineLogger Logger { get; set; }
        PipelineContext Context { get; }

        /// <summary>
        /// This transforms the row in the pipeline.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        Row Transform(Row row);
    }

}