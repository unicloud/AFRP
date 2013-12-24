using System;
using System.Windows;
using System.Windows.Controls;

namespace UniCloud.Infrastructure
{
    public class RtbExtension
    {
        public static readonly DependencyProperty RtbContentProperty =
            DependencyProperty.RegisterAttached("RtbContent", typeof(string), typeof(RtbExtension), new PropertyMetadata(Changed));
        public static string GetRtbContent(DependencyObject obj)
        {
            RichTextBox rtb = obj as RichTextBox;
            if (rtb == null)
                throw new ArgumentException("RtbContent is only supported on RichTextBox Control");
            // missing check whether contains any UIElement 

            return (string)rtb.Xaml;
        }
        public static void SetRtbContent(DependencyObject obj, string value)
        {
            RichTextBox rtb = obj as RichTextBox;
            if (rtb == null)
                throw new ArgumentException("RtbContent is only supported on RichTextBox Control");
            rtb.Xaml = value;
        }
        private static void Changed(object sender, DependencyPropertyChangedEventArgs args)
        {
            RichTextBox c = sender as RichTextBox;
            if (c == null)
                throw new ArgumentException("Must be RichTextBox");
            if (string.IsNullOrWhiteSpace((string)args.NewValue))
                c.Blocks.Clear();
            else
                c.Xaml = (string)args.NewValue;
        }
    }
}
