namespace Pipeline.Interfaces {
    public interface IRow {
        object this[IField field] { get; set; }
    }
}