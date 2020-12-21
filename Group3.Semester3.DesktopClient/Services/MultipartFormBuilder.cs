using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Group3.Semester3.DesktopClient
{

    namespace WebClientMultipartExtension
    {
        public class MultipartFormBuilder
        {
            static readonly string MultipartContentType = "multipart/form-data; boundary=";
            static readonly string FileHeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
            static readonly string FormDataTemplate = "\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\";\r\n\r\n{2}";

            public string ContentType { get; private set; }

            string Boundary { get; set; }

            Dictionary<string, FileInfo> FilesToSend { get; set; } = new Dictionary<string, FileInfo>();
            Dictionary<string, string> FieldsToSend { get; set; } = new Dictionary<string, string>();

            public MultipartFormBuilder()
            {
                Boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

                ContentType = MultipartContentType + Boundary;
            }

            public void AddField(string key, string value)
            {
                FieldsToSend.Add(key, value);
            }

            public void AddFile(FileInfo file)
            {
                string key = file.Extension.Substring(1);
                FilesToSend.Add(key, file);
            }

            public void AddFile(string key, FileInfo file)
            {
                FilesToSend.Add(key, file);
            }

            public MemoryStream GetStream()
            {
                var memStream = new MemoryStream();

                WriteFields(memStream);
                WriteStreams(memStream);
                WriteTrailer(memStream);

                memStream.Seek(0, SeekOrigin.Begin);

                return memStream;
            }

            void WriteFields(Stream stream)
            {
                if (FieldsToSend.Count == 0)
                    return;

                foreach (var fieldEntry in FieldsToSend)
                {
                    string content = string.Format(FormDataTemplate, Boundary, fieldEntry.Key, fieldEntry.Value);

                    using (var fieldData = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        fieldData.CopyTo(stream);
                    }
                }
            }

            void WriteStreams(Stream stream)
            {
                if (FilesToSend.Count == 0)
                    return;

                foreach (var fileEntry in FilesToSend)
                {
                    WriteBoundary(stream);

                    string header = string.Format(FileHeaderTemplate, fileEntry.Key, fileEntry.Value.Name);
                    byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                    stream.Write(headerbytes, 0, headerbytes.Length);

                    using (var fileData = File.OpenRead(fileEntry.Value.FullName))
                    {
                        fileData.CopyTo(stream);
                    }
                }
            }

            void WriteBoundary(Stream stream)
            {
                byte[] boundarybytes = Encoding.UTF8.GetBytes("\r\n--" + Boundary + "\r\n");
                stream.Write(boundarybytes, 0, boundarybytes.Length);
            }

            void WriteTrailer(Stream stream)
            {
                byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + Boundary + "--\r\n");
                stream.Write(trailer, 0, trailer.Length);
            }
        }

        public static class WebClientExtensionMethods
        {
            public static byte[] UploadMultipart(this WebClient client, string address, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return client.UploadData(address, stream.ToArray());
                }
            }

            public static byte[] UploadMultipart(this WebClient client, Uri address, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return client.UploadData(address, stream.ToArray());
                }
            }

            public static byte[] UploadMultipart(this WebClient client, string address, string method, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return client.UploadData(address, method, stream.ToArray());
                }
            }

            public static byte[] UploadMultipart(this WebClient client, Uri address, string method, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return client.UploadData(address, method, stream.ToArray());
                }
            }

            public static void UploadMultipartAsync(this WebClient client, Uri address, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    client.UploadDataAsync(address, stream.ToArray());
                }
            }

            public static void UploadMultipartAsync(this WebClient client, Uri address, string method, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    client.UploadDataAsync(address, method, stream.ToArray());
                }
            }

            public static void UploadMultipartAsync(this WebClient client, Uri address, string method, MultipartFormBuilder multipart, object userToken)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    client.UploadDataAsync(address, method, stream.ToArray(), userToken);
                }
            }

            public static async Task<byte[]> UploadMultipartTaskAsync(this WebClient client, string address, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return await client.UploadDataTaskAsync(address, stream.ToArray());
                }
            }

            public static async Task<byte[]> UploadMultipartTaskAsync(this WebClient client, Uri address, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return await client.UploadDataTaskAsync(address, stream.ToArray());
                }
            }

            public static async Task<byte[]> UploadMultipartTaskAsync(this WebClient client, string address, string method, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return await client.UploadDataTaskAsync(address, method, stream.ToArray());
                }
            }

            public static async Task<byte[]> UploadMultipartTaskAsync(this WebClient client, Uri address, string method, MultipartFormBuilder multipart)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);

                using (var stream = multipart.GetStream())
                {
                    return await client.UploadDataTaskAsync(address, method, stream.ToArray());
                }
            }
        }
    }
}
