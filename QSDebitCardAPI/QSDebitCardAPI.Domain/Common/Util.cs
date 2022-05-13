using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace QSDataUpdateAPI.Domain
{
    public class Util
    {
        public static string GetXmlTagValue(string xmlObject, string element, string namespacePrefix = "", bool ignoreCase = true)
        {
            try
            {
                xmlObject = xmlObject.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("&lt;", "<").Replace("&gt;", ">");
                var pattern = string.IsNullOrEmpty(namespacePrefix) ? $@"<{element}>.+</{element}>" : $@"<{namespacePrefix}:{element}>.+</{namespacePrefix}:{element}>";
                var matches = ignoreCase ? Regex.Matches(xmlObject, pattern, RegexOptions.IgnoreCase) : Regex.Matches(xmlObject,pattern);
                var matchCount = matches.Count;
                if (matchCount < 1)
                    return "";
                var openingTag = string.IsNullOrEmpty(namespacePrefix) ? $"<{element}>" : $"<{namespacePrefix}:{element}>";
                var closingTag = string.IsNullOrEmpty(namespacePrefix) ? $"</{element}>" : $"</{namespacePrefix}:{element}>";
                var value = matches[0].Value;
                value = value.ToString().Replace(openingTag, "").Replace(closingTag, "")?.Trim();
                return value;
            }
            catch (Exception exception)
            {
                return "";
            }
        }

        public static string GetTagValue(string xmlObject, string element, string namespacePrefix = "", bool retainTag = false, bool ignoreCase = true)
        {
            try
            {
                xmlObject = xmlObject.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("&lt;", "<").Replace("&gt;", ">");
                var openingTag = string.IsNullOrEmpty(namespacePrefix) ? $"<{element}>" : $"<{namespacePrefix}:{element}>";
                var closingTag = string.IsNullOrEmpty(namespacePrefix) ? $"</{element}>" : $"</{namespacePrefix}:{element}>";
                var pattern = $@"{openingTag}.+{closingTag}";
                var matches = ignoreCase ? Regex.Matches(xmlObject, pattern, RegexOptions.IgnoreCase) : Regex.Matches(xmlObject,pattern);
                string tagContent = string.Empty;
                var matchCount = matches.Count;
                if (matchCount < 1)
                {
                    if (xmlObject.Contains(openingTag))
                    {
                        var firstIndexOfOTag = xmlObject.IndexOf(openingTag);
                        var indexOfClosingTag = xmlObject.IndexOf(closingTag);
                        var textLength = xmlObject.Length;
                        tagContent = xmlObject.Substring(firstIndexOfOTag, (indexOfClosingTag - firstIndexOfOTag) + closingTag.Length);
                    }
                    if (string.IsNullOrEmpty(tagContent))
                        return string.Empty;
                }
                else
                {
                    tagContent = matches[0].Value;
                }
                if (retainTag)
                    return tagContent;
                var value = tagContent?.ToString().Replace(openingTag, "").Replace(closingTag, "")?.Trim();
                return value;
            }
            catch (Exception exception)
            {
                //Log.Error(new Exception($"Encountered an error while attempting to get XMLElement with name {element} from Xmlobject \n: {exception}"));
                return "";
            }
        }

        public static string GetSecondTagValue(string xmlObject, string element, string namespacePrefix = "", bool retainTag = false, bool ignoreCase = true)
        {
            try
            {
                xmlObject = xmlObject.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("&lt;", "<").Replace("&gt;", ">");
                var openingTag = string.IsNullOrEmpty(namespacePrefix) ? $"<{element}>" : $"<{namespacePrefix}:{element}>";
                var closingTag = string.IsNullOrEmpty(namespacePrefix) ? $"</{element}>" : $"</{namespacePrefix}:{element}>";
                string tagContent = string.Empty;
                if (xmlObject.Contains(openingTag))
                {
                    var firstIndexOfOTag = xmlObject.IndexOf(openingTag, xmlObject.IndexOf(openingTag)+ 1);
                    var indexOfClosingTag = xmlObject.IndexOf(closingTag, xmlObject.IndexOf(closingTag) + 1);
                    var textLength = xmlObject.Length;
                    tagContent = xmlObject.Substring(firstIndexOfOTag, (indexOfClosingTag - firstIndexOfOTag) + closingTag.Length);
                }
                if (string.IsNullOrEmpty(tagContent))
                    return string.Empty;
                if (retainTag)
                    return tagContent;
                var value = tagContent?.ToString().Replace(openingTag, "").Replace(closingTag, "")?.Trim();
                return value;
            }
            catch (Exception exception)
            {
                //Log.Error(new Exception($"Encountered an error while attempting to get XMLElement with name {element} from Xmlobject \n: {exception}"));
                return "";
            }
        }

        
             public static List<string> GetAllMarchedTagValue(string xmlObject, string element, string namespacePrefix = "", bool retainTag = false, bool ignoreCase = true)
        {
            try
            {
                xmlObject = xmlObject.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("&lt;", "<").Replace("&gt;", ">");
                var openingTag = string.IsNullOrEmpty(namespacePrefix) ? $"<{element}>" : $"<{namespacePrefix}:{element}>";
                var closingTag = string.IsNullOrEmpty(namespacePrefix) ? $"</{element}>" : $"</{namespacePrefix}:{element}>";
                var pattern = $@"{openingTag}.+{closingTag}";
                var matches = ignoreCase ? Regex.Matches(xmlObject, pattern, RegexOptions.IgnoreCase) : Regex.Matches(xmlObject, pattern);
                List<string> tagContent = new List<string>();
                var matchCount = matches.Count;
                if (matchCount < 1)
                {
                    if (xmlObject.Contains(openingTag))
                    {
                        var firstIndexOfOTag = xmlObject.IndexOf(openingTag);
                        var indexOfClosingTag = xmlObject.IndexOf(closingTag);
                        var textLength = xmlObject.Length;
                        tagContent.Add(xmlObject.Substring(firstIndexOfOTag, (indexOfClosingTag - firstIndexOfOTag) + closingTag.Length));
                    }
                    if (tagContent.Count == 0)
                        return new List<string>();
                }
                else
                {
                    for(int i =0; i<matches.Count; i++)
                    {
                        tagContent.Add(matches[i].Value);

                    }
                }
                return tagContent;
               
            }
            catch (Exception exception)
            {
                //Log.Error(new Exception($"Encountered an error while attempting to get XMLElement with name {element} from Xmlobject \n: {exception}"));
                return new List<string>();
            }
        }
        public static string GetFirstTagValue(string xmlObject, string element, string namespacePrefix = "", bool retainTag = false, bool ignoreCase = true)
        {
            try
            {
                xmlObject = xmlObject.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("&lt;", "<").Replace("&gt;", ">");
                var openingTag = string.IsNullOrEmpty(namespacePrefix) ? $"<{element}>" : $"<{namespacePrefix}:{element}>";
                var closingTag = string.IsNullOrEmpty(namespacePrefix) ? $"</{element}>" : $"</{namespacePrefix}:{element}>";
                string tagContent = string.Empty;
                if (xmlObject.Contains(openingTag))
                    {
                        var firstIndexOfOTag = xmlObject.IndexOf(openingTag);
                        var indexOfClosingTag = xmlObject.IndexOf(closingTag);
                        var textLength = xmlObject.Length;
                        tagContent = xmlObject.Substring(firstIndexOfOTag, (indexOfClosingTag - firstIndexOfOTag) + closingTag.Length);
                    }
                if (string.IsNullOrEmpty(tagContent))
                        return string.Empty;
                if (retainTag)
                    return tagContent;
                var value = tagContent?.ToString().Replace(openingTag, "").Replace(closingTag, "")?.Trim();
                return value;
            }
            catch (Exception exception)
            {
                //Log.Error(new Exception($"Encountered an error while attempting to get XMLElement with name {element} from Xmlobject \n: {exception}"));
                return "";
            }
        }

        public static (string responseCode, string responseDescription) ParseRedboxGenericResponse(string xmlResponseObj)
        {
            var responseCode = GetTagValue(xmlResponseObj, "ResponseCode");
            var responseDesc = GetTagValue(xmlResponseObj, "ResponseDescription");
            return (responseCode, responseDesc);
        }

        public static T DeserializeXML<T>(string objectData)
        {
            objectData = objectData.Replace("\n", "");
            var serializer = new XmlSerializer(typeof(T));
            object result;
            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }
            return (T)result;

        }

        public static string TimeStampCode(string prefix = "")
        {
            Thread.Sleep(1);
            string stamp = DateTime.Now.ToString("yyMMddHHmmssffffff");
            long num = long.Parse(stamp);
            var g = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return prefix + num + g;
        }

    }
}
