using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityMatchingReader {
        Row[] Read(IEnumerable<Row> input);
    }

}
