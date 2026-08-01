using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Xml;

namespace Uvel.WPF
{
    /// <summary>
    /// Advanced Logic Runner with Python-like syntax support
    /// Supports: +=, -=, *=, /=, for, while, foreach, def, print, input
    /// </summary>
    public partial class LogicRunner
    {
        #region Advanced Variable Operations
        
        /// <summary>
        /// Increment: += operator
        /// </summary>
        private void ExecuteIncrement(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseValue(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.IncrementVariable(variable, value);
            }
        }
        
        /// <summary>
        /// Decrement: -= operator
        /// </summary>
        private void ExecuteDecrement(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseValue(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.DecrementVariable(variable, value);
            }
        }
        
        /// <summary>
        /// Multiply: *= operator
        /// </summary>
        private void ExecuteMultiply(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseValue(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.MultiplyVariable(variable, value);
            }
        }
        
        /// <summary>
        /// Divide: /= operator
        /// </summary>
        private void ExecuteDivide(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseValue(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.DivideVariable(variable, value);
            }
        }
        
        /// <summary>
        /// Modulo: %= operator
        /// </summary>
        private void ExecuteModulo(XmlNode node)
        {
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", ""));
            string value = GetAttribute(node, "Value", "1");
            
            if (!string.IsNullOrEmpty(variable) && _engine.State.ContainsKey(variable))
            {
                double current, modVal;
                double.TryParse(_engine.State[variable].ToString(), out current);
                double.TryParse(ParseValue(value), out modVal);
                
                if (modVal != 0)
                {
                    _engine.State[variable] = current % modVal;
                }
            }
        }
        
        #endregion
        
        #region Loops (For, While, ForEach)
        
        /// <summary>
        /// For loop: for i in range(start, end)
        /// </summary>
        private void ExecuteFor(XmlNode node, object sender)
        {
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", "i"));
            int start = int.Parse(GetAttribute(node, "Start", "0"));
            int end = int.Parse(GetAttribute(node, "End", "10"));
            int step = int.Parse(GetAttribute(node, "Step", "1"));
            
            // Range syntax: Range="0,10" or Range="10"
            string range = GetAttribute(node, "Range", "");
            if (!string.IsNullOrEmpty(range))
            {
                string[] parts = range.Split(',');
                if (parts.Length == 1)
                {
                    start = 0;
                    int.TryParse(parts[0].Trim(), out end);
                }
                else if (parts.Length >= 2)
                {
                    int.TryParse(parts[0].Trim(), out start);
                    int.TryParse(parts[1].Trim(), out end);
                    if (parts.Length >= 3)
                    {
                        int.TryParse(parts[2].Trim(), out step);
                    }
                }
            }
            
            for (int i = start; i < end; i += step)
            {
                _engine.State[variable] = i;
                Execute(node, sender);
                
                // Check for break
                if (_engine.State.ContainsKey("_break") && (bool)_engine.State["_break"])
                {
                    _engine.State["_break"] = false;
                    break;
                }
                
                // Check for continue
                if (_engine.State.ContainsKey("_continue") && (bool)_engine.State["_continue"])
                {
                    _engine.State["_continue"] = false;
                    continue;
                }
            }
        }
        
        /// <summary>
        /// While loop
        /// </summary>
        private void ExecuteWhile(XmlNode node, object sender)
        {
            string condition = GetAttribute(node, "Condition", "");
            int maxIterations = int.Parse(GetAttribute(node, "MaxIterations", "1000"));
            int iterations = 0;
            
            while (EvaluateCondition(condition) && iterations < maxIterations)
            {
                Execute(node, sender);
                iterations++;
                
                if (_engine.State.ContainsKey("_break") && (bool)_engine.State["_break"])
                {
                    _engine.State["_break"] = false;
                    break;
                }
            }
        }
        
        /// <summary>
        /// ForEach loop
        /// </summary>
        private void ExecuteForEach(XmlNode node, object sender)
        {
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", "item"));
            string listName = GetAttribute(node, "In", GetAttribute(node, "List", ""));
            
            if (_engine.State.ContainsKey(listName))
            {
                object list = _engine.State[listName];
                
                if (list is List<object>)
                {
                    foreach (object item in (List<object>)list)
                    {
                        _engine.State[variable] = item;
                        Execute(node, sender);
                        
                        if (_engine.State.ContainsKey("_break") && (bool)_engine.State["_break"])
                        {
                            _engine.State["_break"] = false;
                            break;
                        }
                    }
                }
                else if (list is string)
                {
                    foreach (char c in (string)list)
                    {
                        _engine.State[variable] = c.ToString();
                        Execute(node, sender);
                    }
                }
            }
        }
        
        /// <summary>
        /// Break loop
        /// </summary>
        private void ExecuteBreak(XmlNode node)
        {
            _engine.State["_break"] = true;
        }
        
        /// <summary>
        /// Continue loop
        /// </summary>
        private void ExecuteContinue(XmlNode node)
        {
            _engine.State["_continue"] = true;
        }
        
        #endregion
        
        #region List Operations
        
        /// <summary>
        /// Create list
        /// </summary>
        private void ExecuteCreateList(XmlNode node)
        {
            string name = GetAttribute(node, "Name", "");
            string values = GetAttribute(node, "Values", "");
            
            if (!string.IsNullOrEmpty(name))
            {
                List<object> list = new List<object>();
                
                if (!string.IsNullOrEmpty(values))
                {
                    string[] items = values.Split(',');
                    foreach (string item in items)
                    {
                        list.Add(item.Trim());
                    }
                }
                
                _engine.State[name] = list;
            }
        }
        
        /// <summary>
        /// Add to list
        /// </summary>
        private void ExecuteListAdd(XmlNode node)
        {
            string listName = GetAttribute(node, "List", "");
            string value = GetAttribute(node, "Value", "");
            
            value = ParseValue(value);
            
            if (_engine.State.ContainsKey(listName) && _engine.State[listName] is List<object>)
            {
                ((List<object>)_engine.State[listName]).Add(value);
            }
        }
        
        /// <summary>
        /// Remove from list
        /// </summary>
        private void ExecuteListRemove(XmlNode node)
        {
            string listName = GetAttribute(node, "List", "");
            string value = GetAttribute(node, "Value", "");
            int index = int.Parse(GetAttribute(node, "Index", "-1"));
            
            if (_engine.State.ContainsKey(listName) && _engine.State[listName] is List<object>)
            {
                List<object> list = (List<object>)_engine.State[listName];
                
                if (index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    list.Remove(value);
                }
            }
        }
        
        /// <summary>
        /// Get list length
        /// </summary>
        private void ExecuteListLength(XmlNode node)
        {
            string listName = GetAttribute(node, "List", "");
            string toState = GetAttribute(node, "ToState", "");
            
            if (_engine.State.ContainsKey(listName) && _engine.State[listName] is List<object>)
            {
                int length = ((List<object>)_engine.State[listName]).Count;
                
                if (!string.IsNullOrEmpty(toState))
                {
                    _engine.State[toState] = length;
                }
            }
        }
        
        #endregion
        
        #region Print & Debug
        
        /// <summary>
        /// Print to console (Python-like)
        /// </summary>
        private void ExecutePrint(XmlNode node)
        {
            string text = GetAttribute(node, "Text", node.InnerText);
            string color = GetAttribute(node, "Color", "");
            
            text = ParseAllVariables(text);
            
            if (!string.IsNullOrEmpty(color))
            {
                try
                {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color, true);
                }
                catch { }
            }
            
            Console.WriteLine(text);
            Console.ResetColor();
        }
        
        /// <summary>
        /// Debug log
        /// </summary>
        private void ExecuteDebug(XmlNode node)
        {
            string message = GetAttribute(node, "Message", node.InnerText);
            string level = GetAttribute(node, "Level", "Info");
            
            message = ParseAllVariables(message);
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            
            switch (level.ToLower())
            {
                case "error":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] ❌ ERROR: {1}", timestamp, message);
                    break;
                case "warning":
                case "warn":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[{0}] ⚠️  WARN: {1}", timestamp, message);
                    break;
                case "success":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[{0}] ✅ SUCCESS: {1}", timestamp, message);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[{0}] ℹ️  INFO: {1}", timestamp, message);
                    break;
            }
            Console.ResetColor();
        }
        
        #endregion
        
        #region Shapes (Rectangle, Ellipse, Line, Path)
        
        /// <summary>
        /// Add Rectangle shape
        /// </summary>
        private void ExecuteAddRectangle(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string name = GetAttribute(node, "Name", "");
            double width = double.Parse(GetAttribute(node, "Width", "100"));
            double height = double.Parse(GetAttribute(node, "Height", "100"));
            string fill = GetAttribute(node, "Fill", "#0078D4");
            string stroke = GetAttribute(node, "Stroke", "");
            double strokeThickness = double.Parse(GetAttribute(node, "StrokeThickness", "0"));
            double cornerRadius = double.Parse(GetAttribute(node, "CornerRadius", "0"));
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.RadiusX = cornerRadius;
            rect.RadiusY = cornerRadius;
            
            try { rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill)); }
            catch { rect.Fill = Brushes.Blue; }
            
            if (!string.IsNullOrEmpty(stroke))
            {
                try { rect.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stroke)); }
                catch { }
                rect.StrokeThickness = strokeThickness;
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                _engine.Controls[name] = rect;
            }
            
            AddToContainer(container, rect);
        }
        
        /// <summary>
        /// Add Ellipse shape
        /// </summary>
        private void ExecuteAddEllipse(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string name = GetAttribute(node, "Name", "");
            double width = double.Parse(GetAttribute(node, "Width", "100"));
            double height = double.Parse(GetAttribute(node, "Height", "100"));
            string fill = GetAttribute(node, "Fill", "#0078D4");
            string stroke = GetAttribute(node, "Stroke", "");
            double strokeThickness = double.Parse(GetAttribute(node, "StrokeThickness", "0"));
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Ellipse ellipse = new Ellipse();
            ellipse.Width = width;
            ellipse.Height = height;
            
            try { ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill)); }
            catch { ellipse.Fill = Brushes.Blue; }
            
            if (!string.IsNullOrEmpty(stroke))
            {
                try { ellipse.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stroke)); }
                catch { }
                ellipse.StrokeThickness = strokeThickness;
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                _engine.Controls[name] = ellipse;
            }
            
            AddToContainer(container, ellipse);
        }
        
        /// <summary>
        /// Add Line shape
        /// </summary>
        private void ExecuteAddLine(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string name = GetAttribute(node, "Name", "");
            double x1 = double.Parse(GetAttribute(node, "X1", "0"));
            double y1 = double.Parse(GetAttribute(node, "Y1", "0"));
            double x2 = double.Parse(GetAttribute(node, "X2", "100"));
            double y2 = double.Parse(GetAttribute(node, "Y2", "100"));
            string stroke = GetAttribute(node, "Stroke", "#FFFFFF");
            double strokeThickness = double.Parse(GetAttribute(node, "StrokeThickness", "2"));
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Line line = new Line();
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            line.StrokeThickness = strokeThickness;
            
            try { line.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stroke)); }
            catch { line.Stroke = Brushes.White; }
            
            if (!string.IsNullOrEmpty(name))
            {
                _engine.Controls[name] = line;
            }
            
            AddToContainer(container, line);
        }
        
        private void AddToContainer(FrameworkElement container, UIElement element)
        {
            if (container is Panel)
            {
                ((Panel)container).Children.Add(element);
            }
            else if (container is Canvas)
            {
                ((Canvas)container).Children.Add(element);
            }
            else if (container is ScrollViewer && ((ScrollViewer)container).Content is Panel)
            {
                ((Panel)((ScrollViewer)container).Content).Children.Add(element);
            }
        }
        
        #endregion
        
        #region Cards & Popups
        
        /// <summary>
        /// Create Card (styled border)
        /// </summary>
        private void ExecuteAddCard(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string name = GetAttribute(node, "Name", "");
            string title = GetAttribute(node, "Title", "");
            string content = GetAttribute(node, "Content", "");
            string background = GetAttribute(node, "Background", "#2D2D30");
            double width = double.Parse(GetAttribute(node, "Width", "300"));
            double cornerRadius = double.Parse(GetAttribute(node, "CornerRadius", "10"));
            bool hasShadow = GetAttribute(node, "Shadow", "true").ToLower() == "true";
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Border card = new Border();
            card.Width = width;
            card.CornerRadius = new CornerRadius(cornerRadius);
            card.Padding = new Thickness(20);
            card.Margin = new Thickness(10);
            
            try { card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(background)); }
            catch { card.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); }
            
            if (hasShadow)
            {
                card.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 15,
                    ShadowDepth = 3,
                    Opacity = 0.3
                };
            }
            
            StackPanel cardContent = new StackPanel();
            
            if (!string.IsNullOrEmpty(title))
            {
                TextBlock titleBlock = new TextBlock();
                titleBlock.Text = title;
                titleBlock.FontSize = 18;
                titleBlock.FontWeight = FontWeights.Bold;
                titleBlock.Foreground = Brushes.White;
                titleBlock.Margin = new Thickness(0, 0, 0, 10);
                cardContent.Children.Add(titleBlock);
            }
            
            if (!string.IsNullOrEmpty(content))
            {
                TextBlock contentBlock = new TextBlock();
                contentBlock.Text = content;
                contentBlock.FontSize = 14;
                contentBlock.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                contentBlock.TextWrapping = TextWrapping.Wrap;
                cardContent.Children.Add(contentBlock);
            }
            
            card.Child = cardContent;
            
            if (!string.IsNullOrEmpty(name))
            {
                _engine.Controls[name] = card;
            }
            
            AddToContainer(container, card);
        }
        
        /// <summary>
        /// Show Popup
        /// </summary>
        private void ExecuteShowPopup(XmlNode node, object sender)
        {
            string title = GetAttribute(node, "Title", "Popup");
            string message = GetAttribute(node, "Message", "");
            string width = GetAttribute(node, "Width", "400");
            string height = GetAttribute(node, "Height", "200");
            string background = GetAttribute(node, "Background", "#252526");
            string type = GetAttribute(node, "Type", "info"); // info, warning, error, success
            bool showCloseButton = GetAttribute(node, "CloseButton", "true").ToLower() == "true";
            
            message = ParseAllVariables(message);
            
            Window popup = new Window();
            popup.Title = title;
            popup.Width = double.Parse(width);
            popup.Height = double.Parse(height);
            popup.WindowStyle = WindowStyle.None;
            popup.AllowsTransparency = true;
            popup.Background = Brushes.Transparent;
            popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popup.Topmost = true;
            
            Border mainBorder = new Border();
            mainBorder.CornerRadius = new CornerRadius(10);
            mainBorder.Margin = new Thickness(20);
            
            try { mainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(background)); }
            catch { mainBorder.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)); }
            
            mainBorder.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 5,
                Opacity = 0.5
            };
            
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            
            // Header
            Border header = new Border();
            header.CornerRadius = new CornerRadius(10, 10, 0, 0);
            
            Color headerColor = Colors.Gray;
            switch (type.ToLower())
            {
                case "success": headerColor = Color.FromRgb(76, 175, 80); break;
                case "warning": headerColor = Color.FromRgb(255, 152, 0); break;
                case "error": headerColor = Color.FromRgb(244, 67, 54); break;
                default: headerColor = Color.FromRgb(33, 150, 243); break;
            }
            header.Background = new SolidColorBrush(headerColor);
            
            Grid headerGrid = new Grid();
            
            TextBlock titleBlock = new TextBlock();
            titleBlock.Text = title;
            titleBlock.FontSize = 16;
            titleBlock.FontWeight = FontWeights.Bold;
            titleBlock.Foreground = Brushes.White;
            titleBlock.VerticalAlignment = VerticalAlignment.Center;
            titleBlock.Margin = new Thickness(15, 0, 0, 0);
            headerGrid.Children.Add(titleBlock);
            
            if (showCloseButton)
            {
                Button closeBtn = new Button();
                closeBtn.Content = "✕";
                closeBtn.Width = 30;
                closeBtn.Height = 30;
                closeBtn.Background = Brushes.Transparent;
                closeBtn.Foreground = Brushes.White;
                closeBtn.BorderThickness = new Thickness(0);
                closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
                closeBtn.Margin = new Thickness(0, 0, 10, 0);
                closeBtn.Cursor = System.Windows.Input.Cursors.Hand;
                closeBtn.Click += (s, e) => popup.Close();
                headerGrid.Children.Add(closeBtn);
            }
            
            // Enable dragging
            header.MouseLeftButtonDown += (s, e) => popup.DragMove();
            
            header.Child = headerGrid;
            Grid.SetRow(header, 0);
            grid.Children.Add(header);
            
            // Content
            TextBlock contentBlock = new TextBlock();
            contentBlock.Text = message;
            contentBlock.FontSize = 14;
            contentBlock.Foreground = Brushes.White;
            contentBlock.TextWrapping = TextWrapping.Wrap;
            contentBlock.Margin = new Thickness(20);
            contentBlock.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(contentBlock, 1);
            grid.Children.Add(contentBlock);
            
            // Footer with OK button
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Width = 100;
            okBtn.Height = 35;
            okBtn.Background = new SolidColorBrush(headerColor);
            okBtn.Foreground = Brushes.White;
            okBtn.BorderThickness = new Thickness(0);
            okBtn.HorizontalAlignment = HorizontalAlignment.Center;
            okBtn.Cursor = System.Windows.Input.Cursors.Hand;
            okBtn.Click += (s, e) =>
            {
                // Execute OnClose handler if exists
                XmlNode onCloseNode = node.SelectSingleNode("OnClose");
                if (onCloseNode != null)
                {
                    Execute(onCloseNode, sender);
                }
                popup.Close();
            };
            Grid.SetRow(okBtn, 2);
            grid.Children.Add(okBtn);
            
            mainBorder.Child = grid;
            popup.Content = mainBorder;
            
            // Animate in
            popup.Opacity = 0;
            popup.Show();
            
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            popup.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        
        /// <summary>
        /// Show Input Dialog
        /// </summary>
        private void ExecuteInputDialog(XmlNode node, object sender)
        {
            string title = GetAttribute(node, "Title", "Input");
            string message = GetAttribute(node, "Message", "Enter value:");
            string defaultValue = GetAttribute(node, "Default", "");
            string toState = GetAttribute(node, "ToState", "");
            string toControl = GetAttribute(node, "ToControl", "");
            string toProperty = GetAttribute(node, "ToProperty", "Text");
            
            Window dialog = new Window();
            dialog.Title = title;
            dialog.Width = 400;
            dialog.Height = 200;
            dialog.WindowStyle = WindowStyle.None;
            dialog.AllowsTransparency = true;
            dialog.Background = Brushes.Transparent;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Topmost = true;
            
            Border mainBorder = new Border();
            mainBorder.CornerRadius = new CornerRadius(10);
            mainBorder.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            mainBorder.Margin = new Thickness(15);
            mainBorder.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 15,
                ShadowDepth = 3,
                Opacity = 0.4
            };
            
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(20);
            
            TextBlock msgBlock = new TextBlock();
            msgBlock.Text = message;
            msgBlock.FontSize = 14;
            msgBlock.Foreground = Brushes.White;
            msgBlock.Margin = new Thickness(0, 0, 0, 15);
            panel.Children.Add(msgBlock);
            
            TextBox inputBox = new TextBox();
            inputBox.Text = defaultValue;
            inputBox.FontSize = 14;
            inputBox.Padding = new Thickness(10, 8, 10, 8);
            inputBox.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            inputBox.Foreground = Brushes.White;
            inputBox.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            panel.Children.Add(inputBox);
            
            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;
            buttonPanel.Margin = new Thickness(0, 20, 0, 0);
            
            Button cancelBtn = new Button();
            cancelBtn.Content = "Cancel";
            cancelBtn.Width = 80;
            cancelBtn.Height = 32;
            cancelBtn.Margin = new Thickness(0, 0, 10, 0);
            cancelBtn.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            cancelBtn.Foreground = Brushes.White;
            cancelBtn.BorderThickness = new Thickness(0);
            cancelBtn.Click += (s, e) => dialog.Close();
            buttonPanel.Children.Add(cancelBtn);
            
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Width = 80;
            okBtn.Height = 32;
            okBtn.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            okBtn.Foreground = Brushes.White;
            okBtn.BorderThickness = new Thickness(0);
            okBtn.Click += (s, e) =>
            {
                string result = inputBox.Text;
                
                if (!string.IsNullOrEmpty(toState))
                {
                    _engine.State[toState] = result;
                }
                
                if (!string.IsNullOrEmpty(toControl))
                {
                    _engine.SetControlProperty(toControl, toProperty, result);
                }
                
                // Execute OnSubmit handler
                XmlNode onSubmitNode = node.SelectSingleNode("OnSubmit");
                if (onSubmitNode != null)
                {
                    Execute(onSubmitNode, sender);
                }
                
                dialog.Close();
            };
            buttonPanel.Children.Add(okBtn);
            
            panel.Children.Add(buttonPanel);
            mainBorder.Child = panel;
            dialog.Content = mainBorder;
            
            dialog.ShowDialog();
        }
        
        #endregion
        
        #region Filters
        
        /// <summary>
        /// Filter list items
        /// </summary>
        private void ExecuteFilter(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string filterText = GetAttribute(node, "Text", "");
            string fromControl = GetAttribute(node, "FromControl", "");
            
            if (!string.IsNullOrEmpty(fromControl))
            {
                object value = _engine.GetControlProperty(fromControl, "Text");
                filterText = value != null ? value.ToString() : "";
            }
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Panel panel = null;
            if (container is Panel)
            {
                panel = (Panel)container;
            }
            else if (container is ScrollViewer && ((ScrollViewer)container).Content is Panel)
            {
                panel = (Panel)((ScrollViewer)container).Content;
            }
            
            if (panel != null)
            {
                foreach (UIElement child in panel.Children)
                {
                    if (child is FrameworkElement)
                    {
                        FrameworkElement fe = (FrameworkElement)child;
                        string text = GetElementText(fe);
                        
                        if (string.IsNullOrEmpty(filterText) || 
                            text.ToLower().Contains(filterText.ToLower()))
                        {
                            fe.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            fe.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        
        private string GetElementText(FrameworkElement element)
        {
            if (element is TextBlock)
                return ((TextBlock)element).Text;
            if (element is TextBox)
                return ((TextBox)element).Text;
            if (element is Label)
                return ((Label)element).Content?.ToString() ?? "";
            if (element is Button)
                return ((Button)element).Content?.ToString() ?? "";
            if (element is Border && ((Border)element).Child != null)
                return GetElementText((FrameworkElement)((Border)element).Child);
            if (element is Panel)
            {
                foreach (UIElement child in ((Panel)element).Children)
                {
                    if (child is FrameworkElement)
                    {
                        string text = GetElementText((FrameworkElement)child);
                        if (!string.IsNullOrEmpty(text))
                            return text;
                    }
                }
            }
            return "";
        }
        
        #endregion
        
        #region Radio Button Groups
        
        /// <summary>
        /// Create Radio Button Group
        /// </summary>
        private void ExecuteCreateRadioGroup(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string groupName = GetAttribute(node, "GroupName", "radioGroup");
            string options = GetAttribute(node, "Options", "");
            string selectedValue = GetAttribute(node, "SelectedValue", "");
            string orientation = GetAttribute(node, "Orientation", "Vertical");
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            StackPanel radioPanel = new StackPanel();
            radioPanel.Orientation = orientation.ToLower() == "horizontal" ? 
                Orientation.Horizontal : Orientation.Vertical;
            
            string[] optionList = options.Split(',');
            
            foreach (string option in optionList)
            {
                string optionTrimmed = option.Trim();
                
                RadioButton radio = new RadioButton();
                radio.Content = optionTrimmed;
                radio.GroupName = groupName;
                radio.Foreground = Brushes.White;
                radio.Margin = new Thickness(5);
                radio.IsChecked = optionTrimmed == selectedValue;
                
                radio.Checked += (s, e) =>
                {
                    _engine.State[groupName + "_Selected"] = optionTrimmed;
                };
                
                string radioName = groupName + "_" + optionTrimmed.Replace(" ", "");
                _engine.Controls[radioName] = radio;
                
                radioPanel.Children.Add(radio);
            }
            
            _engine.Controls[groupName] = radioPanel;
            AddToContainer(container, radioPanel);
        }
        
        /// <summary>
        /// Get selected radio value
        /// </summary>
        private void ExecuteGetRadioValue(XmlNode node)
        {
            string groupName = GetAttribute(node, "GroupName", "");
            string toState = GetAttribute(node, "ToState", "");
            
            if (_engine.State.ContainsKey(groupName + "_Selected") && !string.IsNullOrEmpty(toState))
            {
                _engine.State[toState] = _engine.State[groupName + "_Selected"];
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string ParseValue(string value)
        {
            return ParseControlProperty(value);
        }
        
        private string ParseAllVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // Replace {varName} with actual values
            foreach (KeyValuePair<string, object> kvp in _engine.State)
            {
                string placeholder = "{" + kvp.Key + "}";
                if (text.Contains(placeholder))
                {
                    string value = kvp.Value != null ? kvp.Value.ToString() : "";
                    text = text.Replace(placeholder, value);
                }
            }
            
            return text;
        }
        
        #endregion
    }
}
