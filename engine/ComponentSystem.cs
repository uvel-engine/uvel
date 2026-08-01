using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;

namespace Uvel.WPF
{
    /// <summary>
    /// Uvel Component System v1.0
    /// Supports: Styles, Resources, Components, Templates
    /// </summary>
    public class ComponentSystem
    {
        private WpfEngine _engine;
        
        // Resources (Colors, FontSizes, Brushes, etc.)
        private Dictionary<string, string> _colors;
        private Dictionary<string, double> _fontSizes;
        private Dictionary<string, double> _spacing;
        private Dictionary<string, string> _strings;
        private Dictionary<string, object> _resources;
        
        // Styles
        private Dictionary<string, StyleDefinition> _styles;
        
        // Components (reusable UI templates)
        private Dictionary<string, ComponentDefinition> _components;
        
        // Themes
        private Dictionary<string, ThemeDefinition> _themes;
        private string _currentTheme;

        public Dictionary<string, string> Colors { get { return _colors; } }
        public Dictionary<string, StyleDefinition> Styles { get { return _styles; } }
        public Dictionary<string, ComponentDefinition> Components { get { return _components; } }

        public ComponentSystem(WpfEngine engine)
        {
            _engine = engine;
            _colors = new Dictionary<string, string>();
            _fontSizes = new Dictionary<string, double>();
            _spacing = new Dictionary<string, double>();
            _strings = new Dictionary<string, string>();
            _resources = new Dictionary<string, object>();
            _styles = new Dictionary<string, StyleDefinition>();
            _components = new Dictionary<string, ComponentDefinition>();
            _themes = new Dictionary<string, ThemeDefinition>();
            _currentTheme = "Dark";
            
            // Initialize default resources
            InitializeDefaultResources();
        }

        #region Default Resources

        private void InitializeDefaultResources()
        {
            // === DARK THEME COLORS ===
            _colors["Primary"] = "#8B5CF6";
            _colors["PrimaryLight"] = "#A78BFA";
            _colors["PrimaryDark"] = "#7C3AED";
            
            _colors["Secondary"] = "#3B82F6";
            _colors["SecondaryLight"] = "#60A5FA";
            _colors["SecondaryDark"] = "#2563EB";
            
            _colors["Accent"] = "#06B6D4";
            _colors["AccentLight"] = "#22D3EE";
            _colors["AccentDark"] = "#0891B2";
            
            _colors["Success"] = "#10B981";
            _colors["SuccessLight"] = "#34D399";
            _colors["SuccessDark"] = "#059669";
            
            _colors["Warning"] = "#F59E0B";
            _colors["WarningLight"] = "#FBBF24";
            _colors["WarningDark"] = "#D97706";
            
            _colors["Error"] = "#EF4444";
            _colors["ErrorLight"] = "#F87171";
            _colors["ErrorDark"] = "#DC2626";
            
            _colors["Background"] = "#0A0E1A";
            _colors["BackgroundLight"] = "#1E293B";
            _colors["BackgroundDark"] = "#050810";
            
            _colors["Surface"] = "#FFFFFF08";
            _colors["SurfaceLight"] = "#FFFFFF12";
            _colors["SurfaceDark"] = "#FFFFFF05";
            
            _colors["Border"] = "#FFFFFF15";
            _colors["BorderLight"] = "#FFFFFF20";
            _colors["BorderDark"] = "#FFFFFF08";
            
            _colors["Text"] = "#F8FAFC";
            _colors["TextSecondary"] = "#94A3B8";
            _colors["TextMuted"] = "#64748B";
            _colors["TextDisabled"] = "#475569";
            
            _colors["White"] = "#FFFFFF";
            _colors["Black"] = "#000000";
            _colors["Transparent"] = "Transparent";
            
            // Glass colors
            _colors["Glass"] = "#FFFFFF06";
            _colors["GlassLight"] = "#FFFFFF10";
            _colors["GlassBorder"] = "#FFFFFF12";
            _colors["GlassGlow"] = "#8B5CF630";
            
            // === FONT SIZES ===
            _fontSizes["XS"] = 10;
            _fontSizes["SM"] = 12;
            _fontSizes["MD"] = 14;
            _fontSizes["LG"] = 16;
            _fontSizes["XL"] = 18;
            _fontSizes["XXL"] = 24;
            _fontSizes["XXXL"] = 32;
            _fontSizes["Display"] = 48;
            _fontSizes["Hero"] = 64;
            
            // === SPACING ===
            _spacing["XS"] = 4;
            _spacing["SM"] = 8;
            _spacing["MD"] = 12;
            _spacing["LG"] = 16;
            _spacing["XL"] = 20;
            _spacing["XXL"] = 24;
            _spacing["XXXL"] = 32;
            
            // === CORNER RADIUS ===
            _resources["RadiusSM"] = 6.0;
            _resources["RadiusMD"] = 12.0;
            _resources["RadiusLG"] = 16.0;
            _resources["RadiusXL"] = 20.0;
            _resources["RadiusFull"] = 9999.0;
            
            // === BLUR RADIUS ===
            _resources["BlurSM"] = 10.0;
            _resources["BlurMD"] = 25.0;
            _resources["BlurLG"] = 40.0;
            _resources["BlurXL"] = 60.0;

            _engine.Log("COMPONENT", "Default resources initialized: " + _colors.Count + " colors, " + _fontSizes.Count + " font sizes");
        }

        #endregion

        #region Parse Components Section

        public void ParseComponentsSection(XmlNode componentsNode)
        {
            if (componentsNode == null) return;

            foreach (XmlNode child in componentsNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.Name)
                {
                    case "Resources":
                        ParseResources(child);
                        break;
                    case "Colors":
                        ParseColors(child);
                        break;
                    case "FontSizes":
                        ParseFontSizes(child);
                        break;
                    case "Spacing":
                        ParseSpacing(child);
                        break;
                    case "Styles":
                        ParseStyles(child);
                        break;
                    case "Component":
                        ParseComponent(child);
                        break;
                    case "Theme":
                        ParseTheme(child);
                        break;
                }
            }

            _engine.Log("COMPONENT", "Parsed: " + _styles.Count + " styles, " + _components.Count + " components");
        }

        private void ParseResources(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                string name = GetAttribute(child, "Name", "");
                string value = GetAttribute(child, "Value", "");
                
                if (string.IsNullOrEmpty(name)) continue;

                switch (child.Name)
                {
                    case "Color":
                        _colors[name] = value;
                        break;
                    case "FontSize":
                        double fs;
                        if (double.TryParse(value, out fs))
                            _fontSizes[name] = fs;
                        break;
                    case "Spacing":
                        double sp;
                        if (double.TryParse(value, out sp))
                            _spacing[name] = sp;
                        break;
                    case "String":
                        _strings[name] = value;
                        break;
                    default:
                        _resources[name] = value;
                        break;
                }
            }
        }

        private void ParseColors(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                string name = GetAttribute(child, "Name", "");
                string value = GetAttribute(child, "Value", child.InnerText.Trim());
                
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    _colors[name] = value;
                }
            }
        }

        private void ParseFontSizes(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                string name = GetAttribute(child, "Name", "");
                string value = GetAttribute(child, "Value", child.InnerText.Trim());
                
                double size;
                if (!string.IsNullOrEmpty(name) && double.TryParse(value, out size))
                {
                    _fontSizes[name] = size;
                }
            }
        }

        private void ParseSpacing(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                string name = GetAttribute(child, "Name", "");
                string value = GetAttribute(child, "Value", child.InnerText.Trim());
                
                double sp;
                if (!string.IsNullOrEmpty(name) && double.TryParse(value, out sp))
                {
                    _spacing[name] = sp;
                }
            }
        }

        private void ParseStyles(XmlNode node)
{
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;
        
        if (child.Name == "Style")
        {
            string name = GetAttribute(child, "Name", "");
            string targetType = GetAttribute(child, "TargetType", "Border");
            
            if (string.IsNullOrEmpty(name)) continue;

            StyleDefinition style = new StyleDefinition();
            style.Name = name;
            style.TargetType = targetType;
            style.Properties = new Dictionary<string, string>();

            foreach (XmlNode propNode in child.ChildNodes)
            {
                if (propNode.NodeType != XmlNodeType.Element) continue;

                if (propNode.Name == "Setter" || propNode.Name == "Set")
                {
                    string prop = GetAttribute(propNode, "Property", "");
                    string val = GetAttribute(propNode, "Value", "");
                    
                    if (!string.IsNullOrEmpty(prop))
                    {
                        val = ResolveResourceReference(val);
                        style.Properties[prop] = val;
                    }
                }
            }

            _styles[name] = style;
        }
    }
}

        private void ParseComponent(XmlNode node)
        {
            string name = GetAttribute(node, "Name", "");
            if (string.IsNullOrEmpty(name)) return;

            ComponentDefinition component = new ComponentDefinition();
            component.Name = name;
            component.Parameters = new Dictionary<string, ComponentParameter>();
            component.Template = null;

            // Parse parameters
            XmlNode paramsNode = node.SelectSingleNode("Params");
            if (paramsNode == null) paramsNode = node.SelectSingleNode("Parameters");
            
            if (paramsNode != null)
            {
                foreach (XmlNode paramNode in paramsNode.ChildNodes)
                {
                    if (paramNode.NodeType != XmlNodeType.Element) continue;
                    if (paramNode.Name != "Param" && paramNode.Name != "Parameter") continue;

                    string paramName = GetAttribute(paramNode, "Name", "");
                    if (string.IsNullOrEmpty(paramName)) continue;

                    ComponentParameter param = new ComponentParameter();
                    param.Name = paramName;
                    param.Type = GetAttribute(paramNode, "Type", "string");
                    param.DefaultValue = GetAttribute(paramNode, "Default", "");
                    param.Required = GetAttribute(paramNode, "Required", "false").ToLower() == "true";

                    component.Parameters[paramName] = param;
                }
            }

            // Parse template
            XmlNode templateNode = node.SelectSingleNode("Template");
            if (templateNode != null)
            {
                component.Template = templateNode;
            }
            else
            {
                // Template is directly inside Component
                component.Template = node;
            }

            _components[name] = component;
            _engine.Log("COMPONENT", "Registered component: " + name + " with " + component.Parameters.Count + " params");
        }

        private void ParseTheme(XmlNode node)
{
    string name = GetAttribute(node, "Name", "");
    if (string.IsNullOrEmpty(name)) return;

    ThemeDefinition theme = new ThemeDefinition();
    theme.Name = name;
    theme.Colors = new Dictionary<string, string>();

    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;

        if (child.Name == "Colors")
        {
            foreach (XmlNode colorNode in child.ChildNodes)
            {
                if (colorNode.NodeType != XmlNodeType.Element) continue;
                string colorName = GetAttribute(colorNode, "Name", "");
                string colorValue = GetAttribute(colorNode, "Value", "");
                if (!string.IsNullOrEmpty(colorName))
                {
                    theme.Colors[colorName] = colorValue;
                }
            }
        }
    }

    _themes[name] = theme;
}

        #endregion

        #region Resource Resolution

        public string ResolveResourceReference(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Format: {ResourceName} or {Color.Primary} or {FontSize.LG}
            if (!value.StartsWith("{") || !value.EndsWith("}")) return value;

            string resourceKey = value.Substring(1, value.Length - 2);

            // Check for dot notation: Color.Primary, FontSize.LG
            if (resourceKey.Contains("."))
            {
                string[] parts = resourceKey.Split('.');
                string category = parts[0];
                string name = parts[1];

                switch (category.ToLower())
                {
                    case "color":
                    case "colors":
                        if (_colors.ContainsKey(name)) return _colors[name];
                        break;
                    case "fontsize":
                    case "font":
                        if (_fontSizes.ContainsKey(name)) return _fontSizes[name].ToString();
                        break;
                    case "spacing":
                    case "space":
                        if (_spacing.ContainsKey(name)) return _spacing[name].ToString();
                        break;
                }
            }

            // Direct lookup
            if (_colors.ContainsKey(resourceKey)) return _colors[resourceKey];
            if (_fontSizes.ContainsKey(resourceKey)) return _fontSizes[resourceKey].ToString();
            if (_spacing.ContainsKey(resourceKey)) return _spacing[resourceKey].ToString();
            if (_strings.ContainsKey(resourceKey)) return _strings[resourceKey];
            if (_resources.ContainsKey(resourceKey)) return _resources[resourceKey].ToString();

            return value;
        }

        public string ResolveAllReferences(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (!text.Contains("{")) return text;

            StringBuilder result = new StringBuilder();
            int i = 0;

            while (i < text.Length)
            {
                if (text[i] == '{')
                {
                    int closeIndex = text.IndexOf('}', i + 1);
                    if (closeIndex > i + 1)
                    {
                        string reference = text.Substring(i, closeIndex - i + 1);
                        string resolved = ResolveResourceReference(reference);
                        
                        // If not resolved, try as state variable
                        if (resolved == reference)
                        {
                            string varName = reference.Substring(1, reference.Length - 2);
                            object stateVal = _engine.GetVariable(varName);
                            if (stateVal != null)
                            {
                                resolved = stateVal.ToString();
                            }
                        }
                        
                        result.Append(resolved);
                        i = closeIndex + 1;
                        continue;
                    }
                }
                result.Append(text[i]);
                i++;
            }

            return result.ToString();
        }

        #endregion

        #region Style Application

        public void ApplyStyle(FrameworkElement element, string styleName)
        {
            if (!_styles.ContainsKey(styleName)) return;

            StyleDefinition style = _styles[styleName];

            foreach (KeyValuePair<string, string> prop in style.Properties)
            {
                ApplyPropertyToElement(element, prop.Key, prop.Value);
            }
        }

        public Dictionary<string, string> GetStyleProperties(string styleName)
        {
            if (_styles.ContainsKey(styleName))
            {
                return new Dictionary<string, string>(_styles[styleName].Properties);
            }
            return new Dictionary<string, string>();
        }

        private void ApplyPropertyToElement(FrameworkElement element, string property, string value)
        {
            value = ResolveAllReferences(value);

            try
            {
                switch (property.ToLower())
                {
                    case "background":
                        if (element is Control)
                            ((Control)element).Background = CreateBrush(value);
                        else if (element is Border)
                            ((Border)element).Background = CreateBrush(value);
                        else if (element is Panel)
                            ((Panel)element).Background = CreateBrush(value);
                        break;

                    case "foreground":
                        if (element is Control)
                            ((Control)element).Foreground = CreateBrush(value);
                        else if (element is TextBlock)
                            ((TextBlock)element).Foreground = CreateBrush(value);
                        break;

                    case "borderbrush":
                        if (element is Control)
                            ((Control)element).BorderBrush = CreateBrush(value);
                        else if (element is Border)
                            ((Border)element).BorderBrush = CreateBrush(value);
                        break;

                    case "borderthickness":
                        Thickness bt = ParseThickness(value);
                        if (element is Control)
                            ((Control)element).BorderThickness = bt;
                        else if (element is Border)
                            ((Border)element).BorderThickness = bt;
                        break;

                    case "cornerradius":
                        if (element is Border)
                            ((Border)element).CornerRadius = ParseCornerRadius(value);
                        break;

                    case "padding":
                        Thickness pd = ParseThickness(value);
                        if (element is Control)
                            ((Control)element).Padding = pd;
                        else if (element is Border)
                            ((Border)element).Padding = pd;
                        break;

                    case "margin":
                        element.Margin = ParseThickness(value);
                        break;

                    case "width":
                        double w;
                        if (double.TryParse(value, out w)) element.Width = w;
                        break;

                    case "height":
                        double h;
                        if (double.TryParse(value, out h)) element.Height = h;
                        break;

                    case "fontsize":
                        double fs;
                        if (double.TryParse(value, out fs))
                        {
                            if (element is Control) ((Control)element).FontSize = fs;
                            else if (element is TextBlock) ((TextBlock)element).FontSize = fs;
                        }
                        break;

                    case "fontweight":
                        FontWeight fw = ParseFontWeight(value);
                        if (element is Control) ((Control)element).FontWeight = fw;
                        else if (element is TextBlock) ((TextBlock)element).FontWeight = fw;
                        break;

                    case "opacity":
                        double op;
                        if (double.TryParse(value, out op)) element.Opacity = op;
                        break;

                    case "visibility":
                        element.Visibility = value.ToLower() == "collapsed" ? Visibility.Collapsed :
                                            value.ToLower() == "hidden" ? Visibility.Hidden : Visibility.Visible;
                        break;

                    case "horizontalalignment":
                        element.HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), value, true);
                        break;

                    case "verticalalignment":
                        element.VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), value, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _engine.Log("WARN", "Failed to apply property " + property + ": " + ex.Message);
            }
        }

        #endregion

        #region Component Instantiation

        public string InstantiateComponent(string componentName, Dictionary<string, string> parameters)
        {
            if (!_components.ContainsKey(componentName))
            {
                _engine.Log("ERROR", "Component not found: " + componentName);
                return "";
            }

            ComponentDefinition component = _components[componentName];
            
            // Merge default values with provided parameters
            Dictionary<string, string> mergedParams = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ComponentParameter> param in component.Parameters)
            {
                if (parameters.ContainsKey(param.Key))
                {
                    mergedParams[param.Key] = parameters[param.Key];
                }
                else if (!string.IsNullOrEmpty(param.Value.DefaultValue))
                {
                    mergedParams[param.Key] = param.Value.DefaultValue;
                }
                else if (param.Value.Required)
                {
                    _engine.Log("WARN", "Required parameter missing: " + param.Key + " for component " + componentName);
                }
            }

            // Clone and process template
            string templateXml = component.Template.InnerXml;
            
            // Replace parameter placeholders
            foreach (KeyValuePair<string, string> param in mergedParams)
            {
                templateXml = templateXml.Replace("{" + param.Key + "}", param.Value);
                templateXml = templateXml.Replace("{Param." + param.Key + "}", param.Value);
            }

            // Resolve resource references
            templateXml = ResolveAllReferences(templateXml);

            return templateXml;
        }

        public bool HasComponent(string name)
        {
            return _components.ContainsKey(name);
        }

        public bool HasStyle(string name)
        {
            return _styles.ContainsKey(name);
        }

        #endregion

        #region Theme Management

        public void ApplyTheme(string themeName)
        {
            if (!_themes.ContainsKey(themeName)) return;

            ThemeDefinition theme = _themes[themeName];
            _currentTheme = themeName;

            // Override colors with theme colors
            foreach (KeyValuePair<string, string> color in theme.Colors)
            {
                _colors[color.Key] = color.Value;
            }

            // Override styles with theme styles
            
            _engine.Log("THEME", "Applied theme: " + themeName);
        }

        public string GetCurrentTheme()
        {
            return _currentTheme;
        }

        #endregion

        #region Helper Methods

        private Brush CreateBrush(string value)
        {
            if (string.IsNullOrEmpty(value) || value.ToLower() == "transparent")
                return Brushes.Transparent;

            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(value);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        private Thickness ParseThickness(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Thickness(0);

            string[] parts = value.Split(',');
            if (parts.Length == 1)
            {
                double v;
                if (double.TryParse(parts[0].Trim(), out v))
                    return new Thickness(v);
            }
            else if (parts.Length == 2)
            {
                double h, v;
                if (double.TryParse(parts[0].Trim(), out h) && double.TryParse(parts[1].Trim(), out v))
                    return new Thickness(h, v, h, v);
            }
            else if (parts.Length == 4)
            {
                double l, t, r, b;
                if (double.TryParse(parts[0].Trim(), out l) &&
                    double.TryParse(parts[1].Trim(), out t) &&
                    double.TryParse(parts[2].Trim(), out r) &&
                    double.TryParse(parts[3].Trim(), out b))
                    return new Thickness(l, t, r, b);
            }

            return new Thickness(0);
        }

        private CornerRadius ParseCornerRadius(string value)
        {
            if (string.IsNullOrEmpty(value)) return new CornerRadius(0);

            string[] parts = value.Split(',');
            if (parts.Length == 1)
            {
                double v;
                if (double.TryParse(parts[0].Trim(), out v))
                    return new CornerRadius(v);
            }
            else if (parts.Length == 4)
            {
                double tl, tr, br, bl;
                if (double.TryParse(parts[0].Trim(), out tl) &&
                    double.TryParse(parts[1].Trim(), out tr) &&
                    double.TryParse(parts[2].Trim(), out br) &&
                    double.TryParse(parts[3].Trim(), out bl))
                    return new CornerRadius(tl, tr, br, bl);
            }

            return new CornerRadius(0);
        }

        private FontWeight ParseFontWeight(string value)
        {
            switch (value.ToLower())
            {
                case "thin": return FontWeights.Thin;
                case "extralight": return FontWeights.ExtraLight;
                case "light": return FontWeights.Light;
                case "normal": case "regular": return FontWeights.Normal;
                case "medium": return FontWeights.Medium;
                case "semibold": return FontWeights.SemiBold;
                case "bold": return FontWeights.Bold;
                case "extrabold": return FontWeights.ExtraBold;
                case "black": case "heavy": return FontWeights.Black;
                default: return FontWeights.Normal;
            }
        }

        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }

        #endregion

        #region Built-in Component Templates

        public void RegisterBuiltinComponents()
        {
            // Win11 Button
            RegisterWin11Button();
            
            // iOS Liquid Card
            RegisteriOSLiquidCard();
            
            // Glass Card
            RegisterGlassCard();
            
            // Glass Button
            RegisterGlassButton();
            
            // Avatar
            RegisterAvatar();
            
            // Badge
            RegisterBadge();
            
            // Status Indicator
            RegisterStatusIndicator();
            
            // Icon Button
            RegisterIconButton();
            
            // Stat Card
            RegisterStatCard();
            
            // Action Card
            RegisterActionCard();
            
            // Nav Item
            RegisterNavItem();
            
            // Search Bar
            RegisterSearchBar();
            
            // Divider
            RegisterDivider();

            _engine.Log("COMPONENT", "Registered " + _components.Count + " builtin components");
        }

        private void RegisterWin11Button()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "Win11Button";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Text"] = new ComponentParameter { Name = "Text", Type = "string", DefaultValue = "Button" };
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "" };
            comp.Parameters["Background"] = new ComponentParameter { Name = "Background", Type = "string", DefaultValue = "#FFFFFF0F" };
            comp.Parameters["Foreground"] = new ComponentParameter { Name = "Foreground", Type = "string", DefaultValue = "#FFFFFF" };
            comp.Parameters["HoverBackground"] = new ComponentParameter { Name = "HoverBackground", Type = "string", DefaultValue = "#FFFFFF15" };
            comp.Parameters["PressedBackground"] = new ComponentParameter { Name = "PressedBackground", Type = "string", DefaultValue = "#FFFFFF08" };
            comp.Parameters["CornerRadius"] = new ComponentParameter { Name = "CornerRadius", Type = "string", DefaultValue = "6" };
            comp.Parameters["Padding"] = new ComponentParameter { Name = "Padding", Type = "string", DefaultValue = "16,8" };
            comp.Parameters["Width"] = new ComponentParameter { Name = "Width", Type = "string", DefaultValue = "Auto" };
            comp.Parameters["Height"] = new ComponentParameter { Name = "Height", Type = "string", DefaultValue = "Auto" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            // Template is handled specially in XAML generation
            _components["Win11Button"] = comp;
        }

        private void RegisteriOSLiquidCard()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "iOSLiquidCard";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Title"] = new ComponentParameter { Name = "Title", Type = "string", DefaultValue = "" };
            comp.Parameters["Subtitle"] = new ComponentParameter { Name = "Subtitle", Type = "string", DefaultValue = "" };
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#8B5CF6" };
            comp.Parameters["BlurRadius"] = new ComponentParameter { Name = "BlurRadius", Type = "string", DefaultValue = "35" };
            comp.Parameters["CornerRadius"] = new ComponentParameter { Name = "CornerRadius", Type = "string", DefaultValue = "20" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["iOSLiquidCard"] = comp;
        }

        private void RegisterGlassCard()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "GlassCard";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Background"] = new ComponentParameter { Name = "Background", Type = "string", DefaultValue = "#FFFFFF08" };
            comp.Parameters["GlowColor"] = new ComponentParameter { Name = "GlowColor", Type = "string", DefaultValue = "#8B5CF620" };
            comp.Parameters["BlurRadius"] = new ComponentParameter { Name = "BlurRadius", Type = "string", DefaultValue = "30" };
            comp.Parameters["CornerRadius"] = new ComponentParameter { Name = "CornerRadius", Type = "string", DefaultValue = "16" };
            comp.Parameters["BorderColor"] = new ComponentParameter { Name = "BorderColor", Type = "string", DefaultValue = "#FFFFFF12" };
            comp.Parameters["Padding"] = new ComponentParameter { Name = "Padding", Type = "string", DefaultValue = "16" };
            comp.Parameters["Margin"] = new ComponentParameter { Name = "Margin", Type = "string", DefaultValue = "0" };

            _components["GlassCard"] = comp;
        }

        private void RegisterGlassButton()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "GlassButton";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Text"] = new ComponentParameter { Name = "Text", Type = "string", DefaultValue = "" };
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#8B5CF6" };
            comp.Parameters["Size"] = new ComponentParameter { Name = "Size", Type = "string", DefaultValue = "44" };
            comp.Parameters["CornerRadius"] = new ComponentParameter { Name = "CornerRadius", Type = "string", DefaultValue = "14" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["GlassButton"] = comp;
        }

        private void RegisterAvatar()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "Avatar";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "👤" };
            comp.Parameters["Size"] = new ComponentParameter { Name = "Size", Type = "string", DefaultValue = "48" };
            comp.Parameters["Background"] = new ComponentParameter { Name = "Background", Type = "string", DefaultValue = "#8B5CF620" };
            comp.Parameters["GlowColor"] = new ComponentParameter { Name = "GlowColor", Type = "string", DefaultValue = "#8B5CF6" };
            comp.Parameters["ShowGlow"] = new ComponentParameter { Name = "ShowGlow", Type = "string", DefaultValue = "true" };

            _components["Avatar"] = comp;
        }

        private void RegisterBadge()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "Badge";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Text"] = new ComponentParameter { Name = "Text", Type = "string", DefaultValue = "" };
            comp.Parameters["Count"] = new ComponentParameter { Name = "Count", Type = "string", DefaultValue = "0" };
            comp.Parameters["Background"] = new ComponentParameter { Name = "Background", Type = "string", DefaultValue = "#EF4444" };
            comp.Parameters["Foreground"] = new ComponentParameter { Name = "Foreground", Type = "string", DefaultValue = "#FFFFFF" };
            comp.Parameters["Size"] = new ComponentParameter { Name = "Size", Type = "string", DefaultValue = "20" };

            _components["Badge"] = comp;
        }

        private void RegisterStatusIndicator()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "StatusIndicator";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Status"] = new ComponentParameter { Name = "Status", Type = "string", DefaultValue = "active" };
            comp.Parameters["Size"] = new ComponentParameter { Name = "Size", Type = "string", DefaultValue = "10" };
            comp.Parameters["ShowGlow"] = new ComponentParameter { Name = "ShowGlow", Type = "string", DefaultValue = "true" };

            _components["StatusIndicator"] = comp;
        }

        private void RegisterIconButton()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "IconButton";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "⚙️" };
            comp.Parameters["Size"] = new ComponentParameter { Name = "Size", Type = "string", DefaultValue = "42" };
            comp.Parameters["IconSize"] = new ComponentParameter { Name = "IconSize", Type = "string", DefaultValue = "18" };
            comp.Parameters["Background"] = new ComponentParameter { Name = "Background", Type = "string", DefaultValue = "#FFFFFF08" };
            comp.Parameters["CornerRadius"] = new ComponentParameter { Name = "CornerRadius", Type = "string", DefaultValue = "12" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["IconButton"] = comp;
        }

        private void RegisterStatCard()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "StatCard";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "📊" };
            comp.Parameters["Value"] = new ComponentParameter { Name = "Value", Type = "string", DefaultValue = "0" };
            comp.Parameters["Label"] = new ComponentParameter { Name = "Label", Type = "string", DefaultValue = "Stat" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#3B82F6" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["StatCard"] = comp;
        }

        private void RegisterActionCard()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "ActionCard";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "" };
            comp.Parameters["Title"] = new ComponentParameter { Name = "Title", Type = "string", DefaultValue = "" };
            comp.Parameters["Subtitle"] = new ComponentParameter { Name = "Subtitle", Type = "string", DefaultValue = "" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#3B82F6" };
            comp.Parameters["ShowArrow"] = new ComponentParameter { Name = "ShowArrow", Type = "string", DefaultValue = "true" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["ActionCard"] = comp;
        }

        private void RegisterNavItem()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "NavItem";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "🏠" };
            comp.Parameters["Label"] = new ComponentParameter { Name = "Label", Type = "string", DefaultValue = "" };
            comp.Parameters["Active"] = new ComponentParameter { Name = "Active", Type = "string", DefaultValue = "false" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#8B5CF6" };
            comp.Parameters["OnClick"] = new ComponentParameter { Name = "OnClick", Type = "string", DefaultValue = "" };

            _components["NavItem"] = comp;
        }

        private void RegisterSearchBar()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "SearchBar";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Placeholder"] = new ComponentParameter { Name = "Placeholder", Type = "string", DefaultValue = "Search..." };
            comp.Parameters["Icon"] = new ComponentParameter { Name = "Icon", Type = "string", DefaultValue = "🔍" };
            comp.Parameters["ShowButton"] = new ComponentParameter { Name = "ShowButton", Type = "string", DefaultValue = "true" };
            comp.Parameters["ButtonIcon"] = new ComponentParameter { Name = "ButtonIcon", Type = "string", DefaultValue = "➤" };
            comp.Parameters["AccentColor"] = new ComponentParameter { Name = "AccentColor", Type = "string", DefaultValue = "#8B5CF6" };
            comp.Parameters["OnSearch"] = new ComponentParameter { Name = "OnSearch", Type = "string", DefaultValue = "" };

            _components["SearchBar"] = comp;
        }

        private void RegisterDivider()
        {
            ComponentDefinition comp = new ComponentDefinition();
            comp.Name = "Divider";
            comp.Parameters = new Dictionary<string, ComponentParameter>();
            
            comp.Parameters["Color"] = new ComponentParameter { Name = "Color", Type = "string", DefaultValue = "#FFFFFF10" };
            comp.Parameters["Height"] = new ComponentParameter { Name = "Height", Type = "string", DefaultValue = "1" };
            comp.Parameters["Margin"] = new ComponentParameter { Name = "Margin", Type = "string", DefaultValue = "0,8" };

            _components["Divider"] = comp;
        }

        #endregion
    }

    #region Data Classes

    

    public class ComponentDefinition
    {
        public string Name { get; set; }
        public Dictionary<string, ComponentParameter> Parameters { get; set; }
        public XmlNode Template { get; set; }
    }

    public class ComponentParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public bool Required { get; set; }
    }

    public class ThemeDefinition
{
    public string Name { get; set; }
    public Dictionary<string, string> Colors { get; set; }
}

    #endregion
}