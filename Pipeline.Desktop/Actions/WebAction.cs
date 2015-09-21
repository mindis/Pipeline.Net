using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Pipeline.Interfaces;
using Action = Pipeline.Configuration.Action;

namespace Pipeline.Desktop.Actions {
    public class WebAction : IAction {
        private readonly PipelineContext _context;
        private readonly Action _action;

        public WebAction(PipelineContext context, Action action) {
            _context = context;
            _action = action;
        }

        public ActionResponse Execute() {
            _context.Info("Web request to {0}", _action.Url);
            return _action.Method == "get"
                ? Get(_action.Url, _action.TimeOut)
                : Post(_action.Url, _action.TimeOut, _action.Content);
        }

        public static ActionResponse Get(string url, int timeOut) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut == 0 ? Timeout.Infinite : timeOut;
            request.KeepAlive = timeOut == 0;

            try {
                using (var response = (HttpWebResponse)request.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return new ActionResponse { Code = (int)response.StatusCode };
                        var reader = new StreamReader(responseStream);
                        return new ActionResponse((int)response.StatusCode, reader.ReadToEnd());
                    }
                }
            } catch (Exception e) {
                return new ActionResponse(500, e.Message);
            }
        }

        public static ActionResponse Post(string url, int timeOut, string postData) {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = timeOut == 0 ? Timeout.Infinite : timeOut;
            request.KeepAlive = timeOut == 0;
            request.ContentType = "application/x-www-form-urlencoded";

            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;

            using (var dataStream = request.GetRequestStream()) {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try {
                using (var response = (HttpWebResponse)request.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return new ActionResponse((int)response.StatusCode);
                        var reader = new StreamReader(responseStream);
                        return new ActionResponse((int)response.StatusCode, reader.ReadToEnd());
                    }
                }
            } catch (Exception e) {
                return new ActionResponse(500, e.Message);
            }
        }

    }
}