namespace Pipeline {
    public class ActionResponse {
        public ActionResponse() {
        }
        public ActionResponse(string content) {
            Content = content;
        }
        public ActionResponse(int code) {
            Code = code;
        }
        public ActionResponse(int code, string content) {
            Code = code;
            Content = content;
        }

        public string Content { get; set; } = string.Empty;
        public int Code { get; set; } = 200;
    }
}
