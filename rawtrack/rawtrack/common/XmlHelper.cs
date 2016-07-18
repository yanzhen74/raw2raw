using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Poac.Common
{
    public static class XmlHelper
    {

        public static XmlDocument LoadXmlFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                CreateXmlFile(fileName);
            }
            XmlDocument xmlFile = new XmlDocument();
            xmlFile.Load(fileName);
            return xmlFile;
        }


        public static void CreateXmlFile(string fileName, string rootName="Root")
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter writer = XmlWriter.Create(fileName, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement(rootName);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        #region 读取Node属性值
        public static string GetNodeAttribute(XmlNode xmlNode, string attribute, string defaultValue)
        {
            if (xmlNode.Attributes[attribute] != null)
            {
                return xmlNode.Attributes[attribute].Value.ToString();
            }
            return defaultValue;
        }

        public static bool GetNodeAttribute(XmlNode xmlNode, string attribute, bool defaultValue)
        {
            string strAttribute = GetNodeAttribute(xmlNode, attribute, defaultValue.ToString());
            try
            {
                return Convert.ToBoolean(strAttribute);
            }
            catch (Exception) { ; }
            return false;
        }

        public static int GetNodeAttribute(XmlNode xmlNode, string attribute, int defaultValue)
        {
            string strAttribute = GetNodeAttribute(xmlNode, attribute, defaultValue.ToString());
            try
            {
                return Convert.ToInt32(strAttribute);
            }
            catch (Exception) { ; }
            return defaultValue;
        }


        public static T GetNodeAttribute<T>(XmlNode xmlNode, string attribute, T defaultValue)
        {
            string strAttribute = GetNodeAttribute(xmlNode, attribute, defaultValue.ToString());
            try
            {
                return (T)Enum.Parse(typeof(T), strAttribute);
            }
            catch (Exception) { ; }
            return defaultValue;
        }

        public static T GetEnum<T>(string value, T defaultValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch (Exception) { ;}
            return defaultValue;
        }

        public static int GetInt(string value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception) { ; }
            return defaultValue;
        }

        public static bool GetBool(string value, bool defaultValue)
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception) { ; }
            return defaultValue;
        }
        #endregion

        #region 直接读写指定字段

        public static XmlNode ReadNode(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc.DocumentElement;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时返回该属性值，否则返回串联值</param>
        /// <returns>string</returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Read(path, "/Node", "")
         * XmlHelper.Read(path, "/Node/Element[@Attribute='Name']", "Attribute")
         ************************************************/
        public static string Read(string path, string node, string attribute)
        {
            string value = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node);
                value = (attribute.Equals("") ? xn.InnerText : xn.Attributes[attribute].Value);
            }
            catch { }
            return value;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="element">元素名，非空时插入新元素，否则在该元素中插入属性</param>
        /// <param name="attribute">属性名，非空时插入该元素属性值，否则插入元素值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "Element", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Element", "Attribute", "Value")
         * XmlHelper.Insert(path, "/Node", "", "Attribute", "Value")
         ************************************************/
        public static void Insert(string path, string node, string element, string attribute, string value)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node);
                if (element.Equals(""))
                {
                    if (!attribute.Equals(""))
                    {
                        XmlElement xe = (XmlElement)xn;
                        xe.SetAttribute(attribute, value);
                    }
                }
                else
                {
                    XmlElement xe = doc.CreateElement(element);
                    if (attribute.Equals(""))
                        xe.InnerText = value;
                    else
                        xe.SetAttribute(attribute, value);
                    xn.AppendChild(xe);
                }
                doc.Save(path);
            }
            catch { }
        }

        public static void Insert(string path, string itemName, string attribute, string value)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                foreach (XmlNode node in doc.GetElementsByTagName("item"))
                {
                    if (node.Attributes["name"].Value == itemName)
                    {
                        XmlElement xe = (XmlElement)node;
                        xe.SetAttribute(attribute, value);
                        break;
                    }
                }
                doc.Save(path);
            }
            catch { }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时修改该节点属性值，否则修改节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Attribute", "Value")
         ************************************************/
        public static void Update(string path, string node, string attribute, string value)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node);
                XmlElement xe = (XmlElement)xn;
                if (attribute.Equals(""))
                    xe.InnerText = value;
                else
                    xe.SetAttribute(attribute, value);
                doc.Save(path);
            }
            catch { }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时删除该节点属性值，否则删除节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Delete(path, "/Node", "")
         * XmlHelper.Delete(path, "/Node", "Attribute")
         ************************************************/
        public static void Delete(string path, string node, string attribute)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node);
                XmlElement xe = (XmlElement)xn;
                if (attribute.Equals(""))
                    xn.ParentNode.RemoveChild(xn);
                else
                    xe.RemoveAttribute(attribute);
                doc.Save(path);
            }
            catch { }
        }

        internal static string Read(string path, string node, string attribute, string value)
        {
            string result = Read(path, node, attribute);
            if (result == "")
            {
                result = value;
            }
            return result;
        }
        #endregion

        public static XmlNode AddChild(XmlDocument doc, XmlNode node, string name, string value = "")
        {
            XmlElement subNode = doc.CreateElement(name);
            if (value != "")
            {
                subNode.InnerText = value;
            }
            node.AppendChild(subNode);
            return subNode;
        }

    }
}
