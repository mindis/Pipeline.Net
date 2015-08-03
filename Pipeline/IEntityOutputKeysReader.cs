using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityOutputKeysReader {
        Row[] Read(IEnumerable<Row> input);
    }

}
