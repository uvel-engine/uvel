using System;
using System.Collections.Generic;
using System.Xml;
using Uvel.WPF;

/// <summary>
/// JSON Parser Plugin for Uvel Engine
/// Parses JSON responses and extracts fields
/// </summary>
public class JsonPlugin : IUvelPlugin
{
    public string Name { get { return "JsonPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "JSON parsing and field extraction"; } }

    public void OnLoad(WpfEngine engine)
{
    // JsonArrayPush - Add item to JSON array
    engine.RegisterCommand("jsonarraypush", delegate(XmlNode node, object sender)
    {
        string source = GetAttr(node, "Source", "[]");
        string item = GetAttr(node, "Item", "");
        string toState = GetAttr(node, "ToState", "");

        if (source.StartsWith("{") && source.EndsWith("}"))
        {
            string varName = source.Substring(1, source.Length - 2);
            object val = engine.GetVariable(varName);
            source = val != null ? val.ToString() : "[]";
        }
        if (item.StartsWith("{") && item.EndsWith("}") && !item.Contains(":")) 
        {
            string varName = item.Substring(1, item.Length - 2);
            object val = engine.GetVariable(varName);
            item = val != null ? val.ToString() : "{}";
        }

        source = source.Trim();
        if (string.IsNullOrEmpty(source) || source == "[]")
        {
            source = "[" + item + "]";
        }
        else
        {
            if (source.EndsWith("]")) source = source.Substring(0, source.Length - 1);
            source = source + "," + item + "]";
        }

        if (!string.IsNullOrEmpty(toState)) engine.SetVariable(toState, source);
        return true;
    });

    // JsonFilter - Filter array by field value (contains)
    engine.RegisterCommand("jsonfilter", delegate(XmlNode node, object sender)
    {
        string source = GetAttr(node, "Source", "[]");
        string field = GetAttr(node, "Field", "");
        string value = GetAttr(node, "Value", "").ToLower();
        string toState = GetAttr(node, "ToState", "");

        if (source.StartsWith("{") && source.EndsWith("}"))
        {
            string varName = source.Substring(1, source.Length - 2);
            object val = engine.GetVariable(varName);
            source = val != null ? val.ToString() : "[]";
        }

        List<string> filtered = new List<string>();
        List<Dictionary<string, string>> items = ParseJsonArray(source);

        foreach (var item in items)
        {
            if (item.ContainsKey(field) && item[field].ToLower().Contains(value))
            {
                filtered.Add(SerializeObj(item));
            }
        }

        string result = "[" + string.Join(",", filtered.ToArray()) + "]";
        if (!string.IsNullOrEmpty(toState)) engine.SetVariable(toState, result);
        return true;
    });

    // JsonTable - Render rows
    engine.RegisterCommand("jsontable", delegate(XmlNode node, object sender)
    {
        string source = GetAttr(node, "Source", "[]");
        string containerName = GetAttr(node, "Container", "");

        if (source.StartsWith("{") && source.EndsWith("}"))
        {
            string varName = source.Substring(1, source.Length - 2);
            object val = engine.GetVariable(varName);
            source = val != null ? val.ToString() : "[]";
        }

        if (string.IsNullOrEmpty(containerName)) return true;

        System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
        {
            System.Windows.FrameworkElement container = engine.GetControl(containerName);
            if (container is System.Windows.Controls.Panel)
            {
                System.Windows.Controls.Panel panel = (System.Windows.Controls.Panel)container;
                panel.Children.Clear();

                List<Dictionary<string, string>> items = ParseJsonArray(source);
                int index = 0;

                foreach (var item in items)
                {
                    System.Windows.Controls.Border row = new System.Windows.Controls.Border 
                    { 
                        Background = new System.Windows.Media.SolidColorBrush(index % 2 == 0 ? System.Windows.Media.Color.FromRgb(30, 30, 40) : System.Windows.Media.Color.FromRgb(35, 35, 45)),
                        Padding = new System.Windows.Thickness(10),
                        BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 55)),
                        BorderThickness = new System.Windows.Thickness(0, 0, 0, 1)
                    };

                    System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(50) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1.5, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1.5, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(80) });

                    AddCell(grid, 0, item.ContainsKey("id") ? item["id"] : "", "#666677");
                    AddCell(grid, 1, item.ContainsKey("name") ? item["name"] : "", "White", true);
                    AddCell(grid, 2, item.ContainsKey("role") ? item["role"] : "", "#AAAAAA");
                    AddCell(grid, 3, item.ContainsKey("dept") ? item["dept"] : "", "#AAAAAA");
                    AddCell(grid, 4, item.ContainsKey("salary") ? "$" + item["salary"] : "", "#00FF88");

                    System.Windows.Controls.Button btnDel = new System.Windows.Controls.Button 
                    { 
                        Content = "DEL", 
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 68, 68)),
                        Foreground = System.Windows.Media.Brushes.White,
                        FontSize = 10,
                        Padding = new System.Windows.Thickness(5, 2, 5, 2),
                        BorderThickness = new System.Windows.Thickness(0)
                    };
                    System.Windows.Controls.Grid.SetColumn(btnDel, 5);
                    grid.Children.Add(btnDel);

                    row.Child = grid;
                    panel.Children.Add(row);
                    index++;
                }
            }
        }));

        return true;
    });
    
    // JsonCount implementation
    engine.RegisterCommand("jsoncount", delegate(XmlNode node, object sender)
    {
        string source = GetAttr(node, "Source", "[]");
        string toState = GetAttr(node, "ToState", "");
        
        if (source.StartsWith("{") && source.EndsWith("}"))
        {
            string varName = source.Substring(1, source.Length - 2);
            object val = engine.GetVariable(varName);
            source = val != null ? val.ToString() : "[]";
        }
        
        List<Dictionary<string, string>> items = ParseJsonArray(source);
        if (!string.IsNullOrEmpty(toState)) engine.SetVariable(toState, items.Count);
        
        return true;
    });

    engine.Log("PLUGIN", "JsonPlugin loaded - commands: jsonarraypush, jsonfilter, jsontable, jsoncount");
}
private void AddCell(System.Windows.Controls.Grid grid, int col, string text, string colorHex, bool bold = false)
{
    System.Windows.Controls.TextBlock txt = new System.Windows.Controls.TextBlock 
    { 
        Text = text, 
        Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex)),
        VerticalAlignment = System.Windows.VerticalAlignment.Center,
        FontWeight = bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal
    };
    System.Windows.Controls.Grid.SetColumn(txt, col);
    grid.Children.Add(txt);
}

private string SerializeObj(Dictionary<string, string> item)
{
    List<string> parts = new List<string>();
    foreach (var kvp in item)
    {
        parts.Add("\"" + kvp.Key + "\":\"" + kvp.Value + "\"");
    }
    return "{" + string.Join(",", parts.ToArray()) + "}";
}

private List<Dictionary<string, string>> ParseJsonArray(string json)
{
    List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
    json = json.Trim();
    if (!json.StartsWith("[")) return list;

    int i = 1;
    while (i < json.Length)
    {
        int start = json.IndexOf('{', i);
        if (start == -1) break;
        int end = json.IndexOf('}', start);
        if (end == -1) break;

        string obj = json.Substring(start + 1, end - start - 1);
        Dictionary<string, string> item = new Dictionary<string, string>();
        
        string[] pairs = obj.Split(',');
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if (kv.Length == 2)
            {
                string key = kv[0].Trim().Trim('"');
                string val = kv[1].Trim().Trim('"');
                item[key] = val;
            }
        }
        list.Add(item);
        i = end + 1;
    }
    return list;
}
// Helper: Render JSON to Grid
private void RenderJsonTable(System.Windows.Controls.Grid grid, string json)
{
    grid.Children.Clear();
    grid.RowDefinitions.Clear();
    grid.ColumnDefinitions.Clear();

    json = json.Trim();
    if (!json.StartsWith("[")) return; // Only array of objects supported

    // Parse manually (simple parser)
    List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();
    List<string> headers = new List<string>();

    int i = 1;
    while (i < json.Length)
    {
        int objStart = json.IndexOf('{', i);
        if (objStart == -1) break;
        int objEnd = FindMatchingBracket(json, objStart, '{', '}');
        if (objEnd == -1) break;

        string objJson = json.Substring(objStart, objEnd - objStart + 1);
        Dictionary<string, string> item = new Dictionary<string, string>();
        
        // Simple property extraction
        int propStart = 1;
        while (propStart < objJson.Length)
        {
            int keyStart = objJson.IndexOf('"', propStart);
            if (keyStart == -1) break;
            int keyEnd = FindStringEnd(objJson, keyStart + 1);
            string key = objJson.Substring(keyStart + 1, keyEnd - keyStart - 1);
            
            if (!headers.Contains(key)) headers.Add(key);

            int colon = objJson.IndexOf(':', keyEnd);
            int valStart = colon + 1;
            while (valStart < objJson.Length && (objJson[valStart] == ' ' || objJson[valStart] == '"')) valStart++;
            
            // Assuming simple values for now (string/number)
            int valEnd = valStart;
            bool inStr = false;
            if (objJson[valStart-1] == '"') { valEnd--; inStr = true; valStart--; } // handle quote start
            
            if (inStr) {
                valEnd = FindStringEnd(objJson, valStart + 1);
                item[key] = objJson.Substring(valStart + 1, valEnd - valStart - 1);
            } else {
                while (valEnd < objJson.Length && objJson[valEnd] != ',' && objJson[valEnd] != '}') valEnd++;
                item[key] = objJson.Substring(valStart, valEnd - valStart);
            }
            
            propStart = valEnd + 1;
        }
        
        items.Add(item);
        i = objEnd + 1;
    }

    // Render Grid
    // Headers
    grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
    
    for (int c = 0; c < headers.Count; c++)
    {
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        
        System.Windows.Controls.Border header = new System.Windows.Controls.Border 
        { 
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 60)),
            Padding = new System.Windows.Thickness(5)
        };
        System.Windows.Controls.TextBlock txt = new System.Windows.Controls.TextBlock 
        { 
            Text = headers[c], 
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = System.Windows.FontWeights.Bold
        };
        header.Child = txt;
        System.Windows.Controls.Grid.SetRow(header, 0);
        System.Windows.Controls.Grid.SetColumn(header, c);
        grid.Children.Add(header);
    }

    // Rows
    for (int r = 0; r < items.Count; r++)
    {
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
        var item = items[r];
        
        for (int c = 0; c < headers.Count; c++)
        {
            string val = item.ContainsKey(headers[c]) ? item[headers[c]] : "";
            
            System.Windows.Controls.Border cell = new System.Windows.Controls.Border 
            { 
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new System.Windows.Thickness(0, 0, 0, 1),
                Padding = new System.Windows.Thickness(5)
            };
            System.Windows.Controls.TextBlock txt = new System.Windows.Controls.TextBlock 
            { 
                Text = val, 
                Foreground = System.Windows.Media.Brushes.LightGray 
            };
            cell.Child = txt;
            System.Windows.Controls.Grid.SetRow(cell, r + 1);
            System.Windows.Controls.Grid.SetColumn(cell, c);
            grid.Children.Add(cell);
        }
    }
}

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }

    #region JSON Parsing (Manual - no external libs)

    private static string ExtractJsonField(string json, string fieldName, int arrayIndex)
{
    if (string.IsNullOrEmpty(json)) return "";

    json = json.Trim();

    // FIXED: Handle root array if it's the target
    if (json.StartsWith("[") && (string.IsNullOrEmpty(fieldName) || arrayIndex >= 0))
    {
        // Agar fieldName bo'sh bo'lsa yoki biz aniq array ichidan qidirayotgan bo'lsak
        // avval array elementini olamiz
        string item = GetArrayItem(json, arrayIndex);
        
        // Agar faqat item kerak bo'lsa (field so'ralmagan)
        if (string.IsNullOrEmpty(fieldName)) return item;
        
        // Agar item topilgan bo'lsa, endi qidiruvni shu item ichida davom ettiramiz
        if (!string.IsNullOrEmpty(item))
        {
            json = item;
        }
    }

    // Find "fieldName": or "fieldName" :
    string searchKey = "\"" + fieldName + "\"";
    int keyIndex = json.IndexOf(searchKey);
    if (keyIndex == -1) return "";

    // Find the colon after the key
    int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);
    if (colonIndex == -1) return "";

    // Skip whitespace after colon
    int valueStart = colonIndex + 1;
    while (valueStart < json.Length && (json[valueStart] == ' ' || json[valueStart] == '\t' || json[valueStart] == '\n' || json[valueStart] == '\r'))
    {
        valueStart++;
    }

    if (valueStart >= json.Length) return "";

    char firstChar = json[valueStart];

    // String value
    if (firstChar == '"')
    {
        int strStart = valueStart + 1;
        int strEnd = FindStringEnd(json, strStart);
        if (strEnd > strStart)
        {
            return json.Substring(strStart, strEnd - strStart).Replace("\\\"", "\"").Replace("\\n", "\n");
        }
    }
    // Array value
    else if (firstChar == '[')
    {
        int arrayEnd = FindMatchingBracket(json, valueStart, '[', ']');
        if (arrayEnd > valueStart)
        {
            return json.Substring(valueStart, arrayEnd - valueStart + 1);
        }
    }
    // Object value
    else if (firstChar == '{')
    {
        int objEnd = FindMatchingBracket(json, valueStart, '{', '}');
        if (objEnd > valueStart)
        {
            return json.Substring(valueStart, objEnd - valueStart + 1);
        }
    }
    // Number, boolean, null
    else
    {
        int valEnd = valueStart;
        while (valEnd < json.Length && json[valEnd] != ',' && json[valEnd] != '}' && json[valEnd] != ']' && json[valEnd] != '\n')
        {
            valEnd++;
        }
        return json.Substring(valueStart, valEnd - valueStart).Trim();
    }

    return "";
}

private static string ExtractJsonPath(string json, string path)
{
    if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(path)) return "";

    json = json.Trim();
    string current = json;

    // Handle root array access: [0].field
    if (path.StartsWith("[") && current.StartsWith("["))
    {
        int closeBracket = path.IndexOf(']');
        if (closeBracket > 1)
        {
            string indexStr = path.Substring(1, closeBracket - 1);
            int index = 0;
            if (int.TryParse(indexStr, out index))
            {
                current = GetArrayItem(current, index);
                
                if (closeBracket + 1 < path.Length && path[closeBracket + 1] == '.')
                {
                    path = path.Substring(closeBracket + 2); // Skip ].
                }
                else
                {
                    return current; // Just array item requested
                }
            }
        }
    }

    string[] segments = SplitPath(path);

    foreach (string segment in segments)
    {
        if (string.IsNullOrEmpty(segment)) continue;

        // Check if segment has array index: fieldName[0]
        int bracketPos = segment.IndexOf('[');
        if (bracketPos >= 0)
        {
            string fieldPart = segment.Substring(0, bracketPos);
            string indexStr = segment.Substring(bracketPos + 1).TrimEnd(']');
            int idx = 0;
            int.TryParse(indexStr, out idx);

            if (!string.IsNullOrEmpty(fieldPart))
            {
                current = ExtractJsonField(current, fieldPart, 0);
            }

            // Now current should be an array
            current = current.Trim();
            if (current.StartsWith("["))
            {
                current = GetArrayItem(current, idx);
            }
        }
        else
        {
            // Simple field extraction
            current = ExtractJsonField(current, segment, 0);
        }

        if (string.IsNullOrEmpty(current)) return "";
    }

    // Remove surrounding quotes if present
    current = current.Trim();
    if (current.StartsWith("\"") && current.EndsWith("\"") && current.Length >= 2)
    {
        current = current.Substring(1, current.Length - 2);
    }

    return current.Replace("\\\"", "\"").Replace("\\n", "\n");
}

    

    private static string[] SplitPath(string path)
    {
        List<string> parts = new List<string>();
        string current = "";

        foreach (char c in path)
        {
            if (c == '.' && !current.Contains("["))
            {
                if (current.Length > 0)
                {
                    parts.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current);
        }

        return parts.ToArray();
    }

    private static string GetArrayItem(string arrayJson, int index)
    {
        arrayJson = arrayJson.Trim();
        if (!arrayJson.StartsWith("[")) return "";

        int depth = 0;
        int itemIndex = 0;
        int itemStart = -1;

        for (int i = 1; i < arrayJson.Length; i++)
        {
            char c = arrayJson[i];

            // Skip strings
            if (c == '"')
            {
                i = FindStringEnd(arrayJson, i + 1);
                continue;
            }

            if (c == '{' || c == '[') depth++;
            if (c == '}' || c == ']')
            {
                if (depth == 0)
                {
                    // End of array
                    if (itemIndex == index && itemStart >= 0)
                    {
                        return arrayJson.Substring(itemStart, i - itemStart).Trim();
                    }
                    break;
                }
                depth--;
            }

            if (depth == 0)
            {
                if (itemStart == -1)
                {
                    if (c != ' ' && c != '\n' && c != '\r' && c != '\t' && c != ',')
                    {
                        itemStart = i;
                    }
                }

                if (c == ',')
                {
                    if (itemIndex == index && itemStart >= 0)
                    {
                        return arrayJson.Substring(itemStart, i - itemStart).Trim();
                    }
                    itemIndex++;
                    itemStart = -1;
                }
            }
        }

        // Last item
        if (itemIndex == index && itemStart >= 0)
        {
            string result = arrayJson.Substring(itemStart).Trim();
            if (result.EndsWith("]"))
            {
                result = result.Substring(0, result.Length - 1).Trim();
            }
            return result;
        }

        return "";
    }

    private static int CountJsonArray(string json, string fieldName)
    {
        string arrayStr = "";

        if (string.IsNullOrEmpty(fieldName))
        {
            arrayStr = json.Trim();
        }
        else
        {
            arrayStr = ExtractJsonField(json, fieldName, 0);
        }

        if (string.IsNullOrEmpty(arrayStr)) return 0;
        arrayStr = arrayStr.Trim();
        if (!arrayStr.StartsWith("[")) return 0;

        int count = 0;
        int depth = 0;

        for (int i = 1; i < arrayStr.Length; i++)
        {
            char c = arrayStr[i];

            if (c == '"')
            {
                i = FindStringEnd(arrayStr, i + 1);
                continue;
            }

            if (c == '{' || c == '[') depth++;
            if (c == '}' || c == ']')
            {
                if (depth == 0) break;
                depth--;
            }

            if (depth == 0 && c == ',')
            {
                count++;
            }
        }

        // If array is not empty, count is commas + 1
        if (arrayStr.Length > 2) count++;

        return count;
    }

    private static int FindStringEnd(string json, int startAfterQuote)
    {
        for (int i = startAfterQuote; i < json.Length; i++)
        {
            if (json[i] == '\\')
            {
                i++; // Skip escaped char
                continue;
            }
            if (json[i] == '"')
            {
                return i;
            }
        }
        return json.Length - 1;
    }

    private static int FindMatchingBracket(string json, int openPos, char open, char close)
    {
        int depth = 0;
        for (int i = openPos; i < json.Length; i++)
        {
            char c = json[i];

            if (c == '"')
            {
                i = FindStringEnd(json, i + 1);
                continue;
            }

            if (c == open) depth++;
            if (c == close)
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static string FormatJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int indent = 0;
        bool inString = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (c == '"' && (i == 0 || json[i - 1] != '\\'))
            {
                inString = !inString;
                sb.Append(c);
                continue;
            }

            if (inString)
            {
                sb.Append(c);
                continue;
            }

            switch (c)
            {
                case '{':
                case '[':
                    sb.Append(c);
                    sb.AppendLine();
                    indent++;
                    sb.Append(new string(' ', indent * 2));
                    break;
                case '}':
                case ']':
                    sb.AppendLine();
                    indent--;
                    if (indent < 0) indent = 0;
                    sb.Append(new string(' ', indent * 2));
                    sb.Append(c);
                    break;
                case ',':
                    sb.Append(c);
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 2));
                    break;
                case ':':
                    sb.Append(": ");
                    break;
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    #endregion

    private static string GetAttr(XmlNode node, string name, string defaultValue)
    {
        if (node == null || node.Attributes == null) return defaultValue;
        XmlAttribute attr = node.Attributes[name];
        return attr != null ? attr.Value : defaultValue;
    }
}