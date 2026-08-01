using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Uvel.WPF
{
    /// <summary>
    /// Custom Extensions System
    /// Allows developers to create custom "engines" on top of Uvel
    /// Similar to Next.js for React or Nuxt for Vue
    /// </summary>
    public class UvelExtensionManager
    {
        private WpfEngine _engine;
        private Dictionary<string, IUvelExtension> _extensions;
        private Dictionary<string, Func<XmlNode, FrameworkElement>> _customControls;
        private Dictionary<string, Action<XmlNode, object>> _customCommands;
        
        public UvelExtensionManager(WpfEngine engine)
        {
            _engine = engine;
            _extensions = new Dictionary<string, IUvelExtension>();
            _customControls = new Dictionary<string, Func<XmlNode, FrameworkElement>>();
            _customCommands = new Dictionary<string, Action<XmlNode, object>>();
        }
        
        #region Extension Registration
        
        /// <summary>
        /// Register a custom extension
        /// </summary>
        public void RegisterExtension(IUvelExtension extension)
        {
            string name = extension.Name;
            _extensions[name] = extension;
            
            // Register extension's controls
            foreach (var control in extension.GetCustomControls())
            {
                _customControls[control.Key] = control.Value;
            }
            
            // Register extension's commands
            foreach (var command in extension.GetCustomCommands())
            {
                _customCommands[command.Key] = command.Value;
            }
            
            // Initialize extension
            extension.Initialize(_engine);
            
            Console.WriteLine("[Extension] Loaded: " + name + " v" + extension.Version);
        }
        
        /// <summary>
        /// Load extension from DLL
        /// </summary>
        public void LoadExtensionFromFile(string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("Extension not found: " + dllPath);
            }
            
            Assembly assembly = Assembly.LoadFrom(dllPath);
            
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IUvelExtension).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    IUvelExtension extension = (IUvelExtension)Activator.CreateInstance(type);
                    RegisterExtension(extension);
                }
            }
        }
        
        /// <summary>
        /// Load all extensions from directory
        /// </summary>
        public void LoadExtensionsFromDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;
            
            foreach (string file in Directory.GetFiles(directory, "*.dll"))
            {
                try
                {
                    LoadExtensionFromFile(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Extension] Failed to load " + Path.GetFileName(file) + ": " + ex.Message);
                }
            }
        }
        
        #endregion
        
        #region Custom Controls
        
        /// <summary>
        /// Register a custom control type
        /// </summary>
        public void RegisterControl(string tagName, Func<XmlNode, FrameworkElement> creator)
        {
            _customControls[tagName.ToLower()] = creator;
        }
        
        /// <summary>
        /// Check if custom control exists
        /// </summary>
        public bool HasCustomControl(string tagName)
        {
            return _customControls.ContainsKey(tagName.ToLower());
        }
        
        /// <summary>
        /// Create custom control
        /// </summary>
        public FrameworkElement CreateCustomControl(string tagName, XmlNode node)
        {
            string key = tagName.ToLower();
            if (_customControls.ContainsKey(key))
            {
                return _customControls[key](node);
            }
            return null;
        }
        
        #endregion
        
        #region Custom Commands
        
        /// <summary>
        /// Register a custom command
        /// </summary>
        public void RegisterCommand(string commandName, Action<XmlNode, object> handler)
        {
            _customCommands[commandName.ToLower()] = handler;
        }
        
        /// <summary>
        /// Check if custom command exists
        /// </summary>
        public bool HasCustomCommand(string commandName)
        {
            return _customCommands.ContainsKey(commandName.ToLower());
        }
        
        /// <summary>
        /// Execute custom command
        /// </summary>
        public void ExecuteCustomCommand(string commandName, XmlNode node, object sender)
        {
            string key = commandName.ToLower();
            if (_customCommands.ContainsKey(key))
            {
                _customCommands[key](node, sender);
            }
        }
        
        #endregion
        
        #region Built-in Extensions
        
        /// <summary>
        /// Load built-in extensions
        /// </summary>
        public void LoadBuiltInExtensions()
        {
            // Charts extension
            RegisterControl("chart", CreateChartControl);
            RegisterControl("piechart", CreatePieChartControl);
            RegisterControl("barchart", CreateBarChartControl);
            
            // Data Grid
            RegisterControl("datagrid", CreateDataGridControl);
            RegisterControl("table", CreateDataGridControl);
            
            // Date/Time Picker
            RegisterControl("datepicker", CreateDatePickerControl);
            RegisterControl("timepicker", CreateTimePickerControl);
            
            // Color Picker
            RegisterControl("colorpicker", CreateColorPickerControl);
            
            // Tabs
            RegisterControl("tabcontrol", CreateTabControl);
            RegisterControl("tabs", CreateTabControl);
            
            // TreeView
            RegisterControl("treeview", CreateTreeViewControl);
            
            // Menu
            RegisterControl("menu", CreateMenuControl);
            RegisterControl("menubar", CreateMenuControl);
            
            // Expander
            RegisterControl("expander", CreateExpanderControl);
            RegisterControl("accordion", CreateAccordionControl);
            
            // Rating
            RegisterControl("rating", CreateRatingControl);
            RegisterControl("stars", CreateRatingControl);
            
            // Avatar
            RegisterControl("avatar", CreateAvatarControl);
            
            // Badge
            RegisterControl("badge", CreateBadgeControl);
            
            // Chip/Tag
            RegisterControl("chip", CreateChipControl);
            RegisterControl("tag", CreateChipControl);
            
            // Tooltip (enhanced)
            RegisterControl("infocard", CreateInfoCardControl);
            
            // Loading/Spinner
            RegisterControl("spinner", CreateSpinnerControl);
            RegisterControl("loading", CreateSpinnerControl);
        }
        
        #endregion
        
        #region Control Creators
        
        private FrameworkElement CreateChartControl(XmlNode node)
        {
            // Simple chart placeholder - would need full implementation
            Border chart = new Border();
            chart.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            chart.CornerRadius = new CornerRadius(10);
            chart.Padding = new Thickness(20);
            chart.MinHeight = 200;
            chart.MinWidth = 300;
            
            TextBlock text = new TextBlock();
            text.Text = "📊 Chart Component";
            text.Foreground = System.Windows.Media.Brushes.White;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            
            chart.Child = text;
            return chart;
        }
        
        private FrameworkElement CreatePieChartControl(XmlNode node)
        {
            Border chart = new Border();
            chart.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            chart.CornerRadius = new CornerRadius(10);
            chart.MinHeight = 200;
            chart.MinWidth = 200;
            
            TextBlock text = new TextBlock();
            text.Text = "🥧 Pie Chart";
            text.Foreground = System.Windows.Media.Brushes.White;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            
            chart.Child = text;
            return chart;
        }
        
        private FrameworkElement CreateBarChartControl(XmlNode node)
        {
            Border chart = new Border();
            chart.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            chart.CornerRadius = new CornerRadius(10);
            chart.MinHeight = 200;
            chart.MinWidth = 300;
            
            TextBlock text = new TextBlock();
            text.Text = "📊 Bar Chart";
            text.Foreground = System.Windows.Media.Brushes.White;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            
            chart.Child = text;
            return chart;
        }
        
        private FrameworkElement CreateDataGridControl(XmlNode node)
        {
            DataGrid grid = new DataGrid();
            grid.AutoGenerateColumns = true;
            grid.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(30, 30, 30));
            grid.Foreground = System.Windows.Media.Brushes.White;
            grid.BorderThickness = new Thickness(0);
            grid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            grid.HorizontalGridLinesBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(60, 60, 60));
            
            return grid;
        }
        
        private FrameworkElement CreateDatePickerControl(XmlNode node)
        {
            DatePicker picker = new DatePicker();
            picker.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            picker.Foreground = System.Windows.Media.Brushes.White;
            picker.SelectedDate = DateTime.Today;
            
            return picker;
        }
        
        private FrameworkElement CreateTimePickerControl(XmlNode node)
        {
            // Custom time picker
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            
            ComboBox hours = new ComboBox();
            hours.Width = 60;
            hours.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            for (int i = 0; i < 24; i++)
            {
                hours.Items.Add(i.ToString("D2"));
            }
            hours.SelectedIndex = DateTime.Now.Hour;
            
            TextBlock sep = new TextBlock();
            sep.Text = ":";
            sep.Foreground = System.Windows.Media.Brushes.White;
            sep.VerticalAlignment = VerticalAlignment.Center;
            sep.Margin = new Thickness(5, 0, 5, 0);
            
            ComboBox minutes = new ComboBox();
            minutes.Width = 60;
            minutes.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            for (int i = 0; i < 60; i++)
            {
                minutes.Items.Add(i.ToString("D2"));
            }
            minutes.SelectedIndex = DateTime.Now.Minute;
            
            panel.Children.Add(hours);
            panel.Children.Add(sep);
            panel.Children.Add(minutes);
            
            return panel;
        }
        
        private FrameworkElement CreateColorPickerControl(XmlNode node)
        {
            // Simple color picker with preset colors
            WrapPanel panel = new WrapPanel();
            panel.Width = 200;
            
            string[] colors = { "#F44336", "#E91E63", "#9C27B0", "#673AB7", 
                               "#3F51B5", "#2196F3", "#03A9F4", "#00BCD4",
                               "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
                               "#FFEB3B", "#FFC107", "#FF9800", "#FF5722" };
            
            foreach (string color in colors)
            {
                Border colorBox = new Border();
                colorBox.Width = 30;
                colorBox.Height = 30;
                colorBox.Margin = new Thickness(2);
                colorBox.CornerRadius = new CornerRadius(5);
                colorBox.Cursor = System.Windows.Input.Cursors.Hand;
                
                try
                {
                    colorBox.Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
                }
                catch { }
                
                panel.Children.Add(colorBox);
            }
            
            return panel;
        }
        
        private FrameworkElement CreateTabControl(XmlNode node)
        {
            TabControl tabs = new TabControl();
            tabs.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(37, 37, 38));
            
            foreach (XmlNode tabNode in node.ChildNodes)
            {
                if (tabNode.NodeType != XmlNodeType.Element) continue;
                if (tabNode.Name == "Tab" || tabNode.Name == "TabItem")
                {
                    TabItem item = new TabItem();
                    item.Header = GetAttribute(tabNode, "Header", GetAttribute(tabNode, "Title", "Tab"));
                    
                    // Content would be parsed from child nodes
                    TextBlock content = new TextBlock();
                    content.Text = tabNode.InnerText;
                    content.Foreground = System.Windows.Media.Brushes.White;
                    content.Margin = new Thickness(10);
                    item.Content = content;
                    
                    tabs.Items.Add(item);
                }
            }
            
            return tabs;
        }
        
        private FrameworkElement CreateTreeViewControl(XmlNode node)
        {
            TreeView tree = new TreeView();
            tree.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(30, 30, 30));
            tree.Foreground = System.Windows.Media.Brushes.White;
            
            return tree;
        }
        
        private FrameworkElement CreateMenuControl(XmlNode node)
        {
            Menu menu = new Menu();
            menu.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            
            return menu;
        }
        
        private FrameworkElement CreateExpanderControl(XmlNode node)
        {
            Expander expander = new Expander();
            expander.Header = GetAttribute(node, "Header", GetAttribute(node, "Title", "Expander"));
            expander.Foreground = System.Windows.Media.Brushes.White;
            expander.IsExpanded = GetAttribute(node, "IsExpanded", "false").ToLower() == "true";
            
            return expander;
        }
        
        private FrameworkElement CreateAccordionControl(XmlNode node)
        {
            StackPanel accordion = new StackPanel();
            
            foreach (XmlNode itemNode in node.ChildNodes)
            {
                if (itemNode.NodeType != XmlNodeType.Element) continue;
                
                Expander exp = new Expander();
                exp.Header = GetAttribute(itemNode, "Header", GetAttribute(itemNode, "Title", "Section"));
                exp.Foreground = System.Windows.Media.Brushes.White;
                exp.Margin = new Thickness(0, 0, 0, 5);
                
                TextBlock content = new TextBlock();
                content.Text = itemNode.InnerText;
                content.Foreground = System.Windows.Media.Brushes.White;
                content.Padding = new Thickness(10);
                exp.Content = content;
                
                accordion.Children.Add(exp);
            }
            
            return accordion;
        }
        
        private FrameworkElement CreateRatingControl(XmlNode node)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            
            int maxStars = int.Parse(GetAttribute(node, "Max", "5"));
            int currentValue = int.Parse(GetAttribute(node, "Value", "0"));
            
            for (int i = 1; i <= maxStars; i++)
            {
                TextBlock star = new TextBlock();
                star.Text = i <= currentValue ? "★" : "☆";
                star.FontSize = 24;
                star.Foreground = i <= currentValue ? 
                    System.Windows.Media.Brushes.Gold : 
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(100, 100, 100));
                star.Cursor = System.Windows.Input.Cursors.Hand;
                star.Margin = new Thickness(2);
                
                panel.Children.Add(star);
            }
            
            return panel;
        }
        
        private FrameworkElement CreateAvatarControl(XmlNode node)
        {
            Border avatar = new Border();
            int size = int.Parse(GetAttribute(node, "Size", "40"));
            string initials = GetAttribute(node, "Initials", GetAttribute(node, "Text", "?"));
            string bgColor = GetAttribute(node, "Background", "#0078D4");
            
            avatar.Width = size;
            avatar.Height = size;
            avatar.CornerRadius = new CornerRadius(size / 2);
            
            try
            {
                avatar.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor));
            }
            catch
            {
                avatar.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 120, 212));
            }
            
            TextBlock text = new TextBlock();
            text.Text = initials.Substring(0, Math.Min(2, initials.Length)).ToUpper();
            text.Foreground = System.Windows.Media.Brushes.White;
            text.FontWeight = FontWeights.Bold;
            text.FontSize = size / 2.5;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            
            avatar.Child = text;
            
            return avatar;
        }
        
        private FrameworkElement CreateBadgeControl(XmlNode node)
        {
            Border badge = new Border();
            string text = GetAttribute(node, "Text", GetAttribute(node, "Content", "0"));
            string bgColor = GetAttribute(node, "Background", "#F44336");
            
            badge.CornerRadius = new CornerRadius(10);
            badge.Padding = new Thickness(8, 4, 8, 4);
            
            try
            {
                badge.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor));
            }
            catch
            {
                badge.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(244, 67, 54));
            }
            
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = System.Windows.Media.Brushes.White;
            textBlock.FontSize = 12;
            textBlock.FontWeight = FontWeights.Bold;
            
            badge.Child = textBlock;
            
            return badge;
        }
        
        private FrameworkElement CreateChipControl(XmlNode node)
        {
            Border chip = new Border();
            string text = GetAttribute(node, "Text", GetAttribute(node, "Content", "Chip"));
            string bgColor = GetAttribute(node, "Background", "#3C3C3C");
            bool closable = GetAttribute(node, "Closable", "false").ToLower() == "true";
            
            chip.CornerRadius = new CornerRadius(15);
            chip.Padding = new Thickness(12, 6, closable ? 6 : 12, 6);
            
            try
            {
                chip.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor));
            }
            catch
            {
                chip.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(60, 60, 60));
            }
            
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = System.Windows.Media.Brushes.White;
            textBlock.FontSize = 13;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            panel.Children.Add(textBlock);
            
            if (closable)
            {
                TextBlock closeBtn = new TextBlock();
                closeBtn.Text = "✕";
                closeBtn.Foreground = System.Windows.Media.Brushes.White;
                closeBtn.FontSize = 12;
                closeBtn.Margin = new Thickness(8, 0, 0, 0);
                closeBtn.VerticalAlignment = VerticalAlignment.Center;
                closeBtn.Cursor = System.Windows.Input.Cursors.Hand;
                closeBtn.Opacity = 0.7;
                panel.Children.Add(closeBtn);
            }
            
            chip.Child = panel;
            
            return chip;
        }
        
        private FrameworkElement CreateInfoCardControl(XmlNode node)
        {
            Border card = new Border();
            string title = GetAttribute(node, "Title", "");
            string content = GetAttribute(node, "Content", node.InnerText);
            string icon = GetAttribute(node, "Icon", "ℹ️");
            
            card.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 48));
            card.CornerRadius = new CornerRadius(8);
            card.Padding = new Thickness(15);
            card.Margin = new Thickness(5);
            
            StackPanel panel = new StackPanel();
            
            if (!string.IsNullOrEmpty(title))
            {
                StackPanel header = new StackPanel();
                header.Orientation = Orientation.Horizontal;
                header.Margin = new Thickness(0, 0, 0, 10);
                
                TextBlock iconBlock = new TextBlock();
                iconBlock.Text = icon;
                iconBlock.FontSize = 18;
                iconBlock.Margin = new Thickness(0, 0, 8, 0);
                header.Children.Add(iconBlock);
                
                TextBlock titleBlock = new TextBlock();
                titleBlock.Text = title;
                titleBlock.FontSize = 14;
                titleBlock.FontWeight = FontWeights.Bold;
                titleBlock.Foreground = System.Windows.Media.Brushes.White;
                header.Children.Add(titleBlock);
                
                panel.Children.Add(header);
            }
            
            TextBlock contentBlock = new TextBlock();
            contentBlock.Text = content;
            contentBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(180, 180, 180));
            contentBlock.TextWrapping = TextWrapping.Wrap;
            panel.Children.Add(contentBlock);
            
            card.Child = panel;
            
            return card;
        }
        
        private FrameworkElement CreateSpinnerControl(XmlNode node)
        {
            Border spinner = new Border();
            int size = int.Parse(GetAttribute(node, "Size", "40"));
            string color = GetAttribute(node, "Color", "#0078D4");
            
            spinner.Width = size;
            spinner.Height = size;
            
            TextBlock loading = new TextBlock();
            loading.Text = "⟳";
            loading.FontSize = size * 0.8;
            loading.HorizontalAlignment = HorizontalAlignment.Center;
            loading.VerticalAlignment = VerticalAlignment.Center;
            
            try
            {
                loading.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            }
            catch
            {
                loading.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 120, 212));
            }
            
            // Add rotation animation
            System.Windows.Media.RotateTransform rotate = new System.Windows.Media.RotateTransform();
            loading.RenderTransform = rotate;
            loading.RenderTransformOrigin = new Point(0.5, 0.5);
            
            System.Windows.Media.Animation.DoubleAnimation rotateAnim = new System.Windows.Media.Animation.DoubleAnimation();
            rotateAnim.From = 0;
            rotateAnim.To = 360;
            rotateAnim.Duration = TimeSpan.FromSeconds(1);
            rotateAnim.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
            rotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotateAnim);
            
            spinner.Child = loading;
            
            return spinner;
        }
        
        #endregion
        
        #region Helpers
        
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
        
        #endregion
    }
    
    #region Extension Interface
    
    /// <summary>
    /// Interface for Uvel extensions
    /// Implement this to create custom "engines" on top of Uvel
    /// </summary>
    public interface IUvelExtension
    {
        /// <summary>
        /// Extension name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Extension version
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Extension description
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Initialize extension with engine reference
        /// </summary>
        void Initialize(WpfEngine engine);
        
        /// <summary>
        /// Get custom controls provided by this extension
        /// </summary>
        Dictionary<string, Func<XmlNode, FrameworkElement>> GetCustomControls();
        
        /// <summary>
        /// Get custom commands provided by this extension
        /// </summary>
        Dictionary<string, Action<XmlNode, object>> GetCustomCommands();
    }
    
    #endregion
    
    #region Example Extension
    
    /// <summary>
    /// Example extension - shows how to create custom extensions
    /// </summary>
    public class ExampleExtension : IUvelExtension
    {
        private WpfEngine _engine;
        
        public string Name { get { return "ExampleExtension"; } }
        public string Version { get { return "1.0.0"; } }
        public string Description { get { return "Example extension demonstrating custom controls and commands"; } }
        
        public void Initialize(WpfEngine engine)
        {
            _engine = engine;
        }
        
        public Dictionary<string, Func<XmlNode, FrameworkElement>> GetCustomControls()
        {
            Dictionary<string, Func<XmlNode, FrameworkElement>> controls = 
                new Dictionary<string, Func<XmlNode, FrameworkElement>>();
            
            // Register custom control
            controls["custombox"] = CreateCustomBox;
            
            return controls;
        }
        
        public Dictionary<string, Action<XmlNode, object>> GetCustomCommands()
        {
            Dictionary<string, Action<XmlNode, object>> commands = 
                new Dictionary<string, Action<XmlNode, object>>();
            
            // Register custom command
            commands["customaction"] = ExecuteCustomAction;
            
            return commands;
        }
        
        private FrameworkElement CreateCustomBox(XmlNode node)
        {
            Border box = new Border();
            box.Background = System.Windows.Media.Brushes.Purple;
            box.CornerRadius = new CornerRadius(10);
            box.Padding = new Thickness(20);
            
            TextBlock text = new TextBlock();
            text.Text = "Custom Control from Extension!";
            text.Foreground = System.Windows.Media.Brushes.White;
            box.Child = text;
            
            return box;
        }
        
        private void ExecuteCustomAction(XmlNode node, object sender)
        {
            MessageBox.Show("Custom action executed from extension!");
        }
    }
    
    #endregion
}
