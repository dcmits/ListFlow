using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using System.Net;

namespace SeatFlow.Helpers
{
    public static class ToolBox
    {
        public static string GetTempDirectory()
        {
            string path;
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));

            Directory.CreateDirectory(path);

            return path;
        }

        public static T DeepClone<T>(this T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);

            }
        }

        public static int GetLineCount(this TextBlock tb)
        {
            PropertyInfo propertyInfo = GetPrivatePropertyInfo(typeof(TextBlock), "LineCount");
            int result = (int)propertyInfo.GetValue(tb);
            return result;
        }

        private static PropertyInfo GetPrivatePropertyInfo(Type type, string propertyName)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);
            return props.FirstOrDefault(propInfo => propInfo.Name == propertyName);
        }

        /// <summary>
        /// Contrôle la validité du nom du fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier à contrôler.</param>
        /// <returns>True si le nom du fichier est valide, false si il contient des caractères non autorisés.</returns>
        public static bool IsValidFilename(string fileName)
        {
            return !Regex.IsMatch(fileName, string.Format($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]"));
        }

        /// <summary>
        /// Contrôle si le nom du fichier contient une extension et si elle correspond au paramètre.
        /// Si le nom du fichier ne contient pas d'extension ou qu'elle n'est pas valide, elle est ajoutée/remplacée.
        /// </summary>
        /// <param name="fileName">Nom du fichier à contrôler.</param>
        /// <param name="validExtension">Extension valide.</param>
        /// <returns>Nom du fichier avec l'extension valide.</returns>
        public static string CheckExtension(string fileName, string validExtension)
        {
            return (string.IsNullOrEmpty(Path.GetExtension(fileName)) || string.Compare(Path.GetExtension(fileName), validExtension, true) != 0) ? Path.ChangeExtension(fileName, validExtension) : fileName;
        }

        /// <summary>
        /// Ectrait le fichier binaire des ressources.
        /// </summary>
        /// <param name="nameSpace">Project Namespace.</param>
        /// <param name="outDirectory">Ouput folder</param>
        /// <param name="internalFilePath">The name of the folder inside visual studio which the files are in.</param>
        /// <param name="resourceName">The name of the file</param>
        public static void ExtractFromResources(string nameSpace, string outDirectory, string internalFilePath, string resourceName)
        {
            //nameSpace = the namespace of your project, located right above your class' name;
            //outDirectory = where the file will be extracted to;
            //internalFilePath = the name of the folder inside visual studio which the files are in;
            //resourceName = the name of the file;
            Assembly assembly = Assembly.GetCallingAssembly();

            using (Stream s = assembly.GetManifestResourceStream(nameSpace + "." + (internalFilePath == "" ? "" : internalFilePath + ".") + resourceName))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(outDirectory + "\\" + resourceName, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                w.Write(r.ReadBytes((int)s.Length));
            }
        }

        public static async Task EnsureUnpacked(string saveDirectory)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
            foreach (var name in assembly.GetManifestResourceNames())
            {
                var stream = assembly.GetManifestResourceStream(name);

                var stringBuilder = new StringBuilder();
                var parts = name
                    .Replace(typeof(ToolBox).Namespace + ".", string.Empty)
                    .Split('.')
                    .ToList();
                for (int i = 0; i < parts.Count; ++i)
                {
                    var part = parts[i];
                    if (string.Equals(part, string.Empty))
                    {
                        stringBuilder.Append(".");      // Append '.' in file name.
                    }
                    else if (i == parts.Count - 2)
                    {
                        stringBuilder.Append(part);     // Append file name and '.'.
                        stringBuilder.Append('.');
                    }
                    else if (i == parts.Count - 1)
                    {
                        stringBuilder.Append(part);     // Append file extension.
                    }
                    else
                    {
                        stringBuilder.Append(part);     // Append file path.
                        stringBuilder.Append('\\');
                    }
                }

                var filePath = Path.Combine(saveDirectory, stringBuilder.ToString());
                using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        /// <summary>
        /// Assemble des URL entre elles.
        /// </summary>
        /// <param name="baseUrl">URL de base.</param>
        /// <param name="path">Chemin à ajouter à l'URL de base.</param>
        /// <returns>URL combiné.</returns>
        public static string CombineUrl(string baseUrl, string path)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, path);
            return uriBuilder.ToString();
        }

        public static bool UrlExist(Uri uri)
        {
            WebRequest webRequest = HttpWebRequest.Create(uri);
            webRequest.Method = "HEAD";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }


    }
}
