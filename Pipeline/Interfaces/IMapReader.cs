using Pipeline.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Interfaces {
   public interface IMapReader {
      IEnumerable<MapItem> Read(PipelineContext context); 
   }

   public class DefaultMapReader : IMapReader {

      public IEnumerable<MapItem> Read(PipelineContext context) {
         return context.Process.Maps.First(m => m.Name == context.Transform.Map).Items;
      }
   }
}
