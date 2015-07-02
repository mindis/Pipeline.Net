namespace Pipeline.Transformers {
    /// <summary>
    /// all transformers should implement this
    /// </summary>
    public interface ITransformer {
        Row Transform(Row row);
    }
}