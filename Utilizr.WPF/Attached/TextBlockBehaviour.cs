using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Utilizr.WPF.Attached
{
    public class TextBlockBehaviour
    {
        public static readonly DependencyProperty UseNarrationProperty = DependencyProperty.RegisterAttached(
            "UseNarration",
            typeof(bool),
            typeof(TextBlockBehaviour),
            new FrameworkPropertyMetadata(false, UseNarrationPropertyChanged)
            );

        private static void UseNarrationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
                return;

            AutomationPeer peer = UIElementAutomationPeer.FromElement(textBlock)
                ?? UIElementAutomationPeer.CreatePeerForElement(textBlock);

            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }

        public static void SetUseNarration(DependencyObject textBlock, bool value)
        {
            textBlock.SetValue(UseNarrationProperty, value);
        }

        public static bool GetUseNarration(DependencyObject textBlock)
        {
            return (bool)textBlock.GetValue(UseNarrationProperty);
        }

        public static readonly DependencyProperty InlineTextTextProperty = DependencyProperty.RegisterAttached(
            "InlineText",
            typeof(string),
            typeof(TextBlockBehaviour),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                InlineTextPropertyChanged
            )
        );

        private static void InlineTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
                return;

            var formattedText = (string)e.NewValue ?? string.Empty;

            // sanitize input to ensure valid xaml, e.g. '&' => '&amp;'
            if (formattedText.Length > 0)
            {
                var sb = new StringBuilder();
                var charsArray = formattedText.ToCharArray();
                foreach (var c in charsArray)
                {
                    if (_invalidXamlCharacters.TryGetValue(c, out string? safeEquivalent))
                    {
                        sb.Append(safeEquivalent);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                formattedText = sb.ToString();
            }

            formattedText = string.Format(INLINE_FMT, _assemblyName, formattedText);

            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(formattedText)))
                {
                    if (textBlock != null)
                    {
                        textBlock.Inlines.Clear();
                        var result = (Span)XamlReader.Load(stream);
                        textBlock.Inlines.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                var original = (string?)e.NewValue ?? string.Empty;
                throw new Exception($"Failed to process inlines for '{original}'", ex);
            }
        }

        public static void SetInlineText(DependencyObject textBlock, string value)
        {
            textBlock.SetValue(InlineTextTextProperty, value);
        }

        public static string GetInlineText(DependencyObject textBlock)
        {
            return (string)textBlock.GetValue(InlineTextTextProperty);
        }

        const string INLINE_FMT = "<Span xml:space=\"preserve\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:wpf=\"clr-namespace:Utilizr.WPF.Controls;assembly={0}\">{1}</Span>";
        private static string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        static readonly Dictionary<char, string> _invalidXamlCharacters;

        static TextBlockBehaviour()
        {
            // all invalid xaml entries
            _invalidXamlCharacters = new Dictionary<char, string>
            {
                { '&', "&amp;" },
                // todo: better method to sanitize text only rather than text and formatting
                // Following entries will mess with formatting elements, e.g. <Run Property="value"/>
                //{ '<', "&lt;" },
                //{ '>', "&gt;" },
                //{ '"', "&quot;" },
                //{ '\'', "&apos;" },
            };
        }
    }
}
