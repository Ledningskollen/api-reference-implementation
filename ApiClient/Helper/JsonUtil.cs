using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace ApiClient.Helper {
    public class JsonUtils<T> where T : class {
        /// <summary>
        /// Global Json serializer settings
        /// </summary>
        private static DataContractJsonSerializerSettings Settings {
            get {
                return new DataContractJsonSerializerSettings {
                    UseSimpleDictionaryFormat = true,
                    RootName = "root",
                    MaxItemsInObjectGraph = int.MaxValue
                };
            }
        }

        /// <summary>
        /// Serialize object of type T to string
        /// </summary>
        /// <param name="obj">type T object</param>
        /// <returns>serialized string</returns>
        public static string Serialize(T obj) {
            string result;
            MemoryStream stream = null;
            try {
                stream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(obj.GetType(), Settings);
                serializer.WriteObject(stream, obj);
                byte[] buffer = stream.ToArray();
                result = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            } finally {
                if (stream != null) {
                    stream.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Deserialize string to object type T
        /// </summary>
        /// <param name="json">string T object</param>
        /// <param name="isResponseSuccess"></param>
        /// <returns>deserialized T</returns>
        public static T Deserialize(string json, bool isResponseSuccess = true) {
            //T result = Activator.CreateInstance<T>();

            T result = null;
            MemoryStream stream = null;
            try {
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream = new MemoryStream(buffer, 0, buffer.Length);
                
                if (!isResponseSuccess) {
                    var serializer =
                        new DataContractJsonSerializer(typeof (ErrorMessage), Settings);
                    var em = (ErrorMessage) serializer.ReadObject(stream);
                    throw new JsonException(em.Message);

                } else {
                    var serializer =
                        new DataContractJsonSerializer(typeof (T), Settings);
                    result = (T) serializer.ReadObject(stream);
                }

            } finally {
                if (stream != null) {
                    stream.Dispose();
                }
            }
            return result;
        }
    }

    [DataContract]
    class ErrorMessage {
        [DataMember]
        public string Message { get; set; }
    }
}