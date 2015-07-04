namespace Pipeline {
    public interface IRow {
        object this[IField field] { get; set; }
    }
}