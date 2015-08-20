namespace Pipeline.Interfaces {
   public interface IField {
      short Index { get; }
      short MasterIndex { get; }
      string Type { get; }
   }
}