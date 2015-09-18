﻿using Pipeline.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline {
    public class ProcessController : IProcessController {

        private readonly IEnumerable<IEntityPipeline> _entityPipelines;
        private readonly IEnumerable<IEntityDeleteHandler> _deleteHandlers;
        public List<IAction> PreActions { get; } = new List<IAction>();
        public List<IAction> PostActions { get; } = new List<IAction>();

        public ProcessController(
            IEnumerable<IEntityPipeline> entityPipelines,
            IEnumerable<IEntityDeleteHandler> deleteHandlers
        ) {
            _entityPipelines = entityPipelines;
            _deleteHandlers = deleteHandlers;
        }

        public void PreExecute() {
            foreach (var action in PreActions) {
                action.Execute();
            }
        }

        public void Execute() {
            foreach (var entity in _entityPipelines) {
                entity.Initialize();
                entity.Execute();
            }
        }

        public void PostExecute() {
            foreach (var handler in _deleteHandlers) {
                handler.Delete();
            }
            foreach (var action in PostActions) {
                action.Execute();
            }
        }

        public IEnumerable<Row> Run() {
            // todo, flatten and send all entity data back, for now, take first entity
            return _entityPipelines.First().Run();
        }
    }
}
