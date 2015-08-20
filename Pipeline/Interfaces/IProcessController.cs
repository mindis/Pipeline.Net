using System;
using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IProcessController {
        void Initialize();
        IEnumerable<IEntityPipeline> EntityPipelines { get; set; }
    }    
}
