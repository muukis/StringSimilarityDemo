using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StringSimilarityDemo.Common
{
    public class ObjectXmlSerializer
    {
        public string SerializeToXml<T>(T obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw))
                {
                    xmlSerializer.Serialize(writer, obj);
                    return sw.ToString();
                }
            }
        }

        public T DeserializeFromXml<T>(string xml)
        {
            using (TextReader reader = new StringReader(xml))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                return (T) xmlSerializer.Deserialize(reader);
            }
        }

        public void SerializeToFile<T>(T obj, string path)
        {
            File.WriteAllText(path, SerializeToXml(obj), Encoding.UTF8);
        }

        public T DeserializeFromFile<T>(string path)
        {
            return DeserializeFromXml<T>(File.ReadAllText(path));
        }

        public List<T> DeserializeFromFiles<T>(string rootPath, string searchPattern, Action<string> actionForEverySuccess = null)
        {
            var backupFiles = Directory.GetFiles(rootPath, searchPattern, SearchOption.TopDirectoryOnly);

            if (backupFiles.Length == 0)
            {
                return null;
            }

            var retval = new List<T>();

            foreach (string backupFile in backupFiles)
            {
                try
                {
                    retval.Add(DeserializeFromFile<T>(backupFile));
                    actionForEverySuccess?.Invoke(backupFile);
                }
                catch (Exception e)
                {
                    ConsoleLogger.Warning(e);
                }
            }

            return retval.Count == 0 ? null : retval;
        }
    }
}
