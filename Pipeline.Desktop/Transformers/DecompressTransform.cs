using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Desktop.Transformers {
    public class DecompressTransform : BaseTransform, ITransform {
        readonly Field _input;
        readonly Field _output;

        public DecompressTransform(PipelineContext context) : base(context) {
            _input = SingleInput();
            _output = context.Field;
        }
        public Row Transform(Row row) {
            row.SetString(_output, Decompress(row.GetString(_input)));
            Increment();
            return row;
        }

        static string Decompress(string compressedText) {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream()) {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress)) {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
