namespace Pipeline {
    public interface IRow {
        object this[int index] { get; set; }
        object this[IField field] { get; set; }
    }
}