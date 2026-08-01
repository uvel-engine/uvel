using System;
using System.Xml;

namespace Uvel.WPF
{
    /// <summary>
    /// XML Parser - Simple XML parsing utilities for Uvel Framework
    /// </summary>
    public class XmlParser
    {
        private WpfEngine _engine;
        
        public XmlParser(WpfEngine engine)
        {
            _engine = engine;
        }
        
        /// <summary>
        /// Get attribute value with default
        /// </summary>
        public string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null) return defaultValue;
            if (node.Attributes == null) return defaultValue;
            
            XmlAttribute attr = node.Attributes[name];
            if (attr == null) return defaultValue;
            
            return attr.Value;
        }
        
        /// <summary>
        /// Get integer attribute
        /// </summary>
        public int GetIntAttribute(XmlNode node, string name, int defaultValue)
        {
            string value = GetAttribute(node, name, defaultValue.ToString());
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Get double attribute
        /// </summary>
        public double GetDoubleAttribute(XmlNode node, string name, double defaultValue)
        {
            string value = GetAttribute(node, name, defaultValue.ToString());
            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Get boolean attribute
        /// </summary>
        public bool GetBoolAttribute(XmlNode node, string name, bool defaultValue)
        {
            string value = GetAttribute(node, name, "").ToLower();
            if (value == "true" || value == "1" || value == "yes")
            {
                return true;
            }
            if (value == "false" || value == "0" || value == "no")
            {
                return false;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Parse Logic section for event handlers
        /// </summary>
        public void ParseLogic(XmlNode logicNode)
        {
            if (logicNode == null) return;
            
            foreach (XmlNode child in logicNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                if (child.Name == "Handler")
                {
                    string name = GetAttribute(child, "Name", "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        _engine.EventHandlers[name] = child;
                    }
                }
                else if (child.Name == "Var" || child.Name == "Variable")
                {
                    string varName = GetAttribute(child, "Name", "");
                    string varValue = GetAttribute(child, "Value", "");
                    string varType = GetAttribute(child, "Type", "string");
                    
                    if (!string.IsNullOrEmpty(varName))
                    {
                        object value = ConvertValue(varValue, varType);
                        _engine.State[varName] = value;
                        _engine.Variables[varName] = value;
                    }
                }
            }
        }
        
        /// <summary>
        /// Convert string value to specified type
        /// </summary>
        private object ConvertValue(string value, string type)
        {
            switch (type.ToLower())
            {
                case "int":
                case "integer":
                    int intVal;
                    int.TryParse(value, out intVal);
                    return intVal;
                    
                case "double":
                case "float":
                case "number":
                    double dblVal;
                    double.TryParse(value, out dblVal);
                    return dblVal;
                    
                case "bool":
                case "boolean":
                    return value.ToLower() == "true" || value == "1";
                    
                default:
                    return value;
            }
        }
    }
}
