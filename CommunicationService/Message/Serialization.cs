using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Message
{
    public static class Serialization
    {
        static BinaryFormatter formatter = new BinaryFormatter();
        public static T Deserialize<T>(this byte[] data)
        {
            T obj = default(T);
            using (var stream = new MemoryStream(data))
            {
                obj = (T)formatter.Deserialize(stream);
            }
            return obj;
        }
        public static byte[] Serialize<T>(this T obj)
        {
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }

        }
    }
}
