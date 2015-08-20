using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IEntityMatchingReader {
        Row[] Read(IEnumerable<Row> input);
    }

}
