using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline {
    public class DefaultDeleteHandler : IEntityDeleteHandler {

        private readonly Entity _entity;
        private readonly IRead _inputReader;
        private readonly IRead _outputReader;
        private readonly IDelete _outputDeleter;

        public DefaultDeleteHandler(
            Entity entity,
            IRead inputReader,
            IRead outputReader,
            IDelete outputDeleter
            ) {
            _entity = entity;
            _inputReader = inputReader;
            _outputReader = outputReader;
            _outputDeleter = outputDeleter;
        }

        public IEnumerable<Row> DetermineDeletes() {
            return _outputReader.Read().Except(_inputReader.Read(), new KeyComparer(_entity.GetPrimaryKey()));
        }

        public void Delete() {
            if (!_entity.IsFirstRun()) {
                _outputDeleter.Delete(DetermineDeletes());
            }
        }
    }
}

