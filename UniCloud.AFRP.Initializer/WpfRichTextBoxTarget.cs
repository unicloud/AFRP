// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using NLog.Config;
    using NLog.Internal;
    using System.Windows.Documents;
    using System.Windows.Media;

    [Target("RichTextBox")]
    public sealed class WpfRichTextBoxTarget : TargetWithLayout
    {
        private int lineCount;
        private static TypeConverter colorConverter = new ColorConverter();

        static WpfRichTextBoxTarget()
        {
            var rules = new List<WpfRichTextBoxRowColoringRule>()
            {
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Fatal", "Red", "Empty", FontStyles.Normal, FontWeights.Bold),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Error", "Orange", "Empty", FontStyles.Normal, FontWeights.Bold),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Warn", "Yellow", "Empty"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty"),
            };

            DefaultRowColoringRules = rules.AsReadOnly();
        }

        public WpfRichTextBoxTarget()
        {
            this.WordColoringRules = new List<WpfRichTextBoxWordColoringRule>();
            this.RowColoringRules = new List<WpfRichTextBoxRowColoringRule>();
            this.ToolWindow = true;
        }

        private delegate void DelSendTheMessageToRichTextBox(string logMessage, WpfRichTextBoxRowColoringRule rule);

        private delegate void FormCloseDelegate();

        public static ReadOnlyCollection<WpfRichTextBoxRowColoringRule> DefaultRowColoringRules { get; private set; }

        public string ControlName { get; set; }

        public string FormName { get; set; }

        [DefaultValue(false)]
        public bool UseDefaultRowColoringRules { get; set; }

        [ArrayParameter(typeof(WpfRichTextBoxRowColoringRule), "row-coloring")]
        public IList<WpfRichTextBoxRowColoringRule> RowColoringRules { get; private set; }

        [ArrayParameter(typeof(WpfRichTextBoxWordColoringRule), "word-coloring")]
        public IList<WpfRichTextBoxWordColoringRule> WordColoringRules { get; private set; }

        [DefaultValue(true)]
        public bool ToolWindow { get; set; }

        public bool ShowMinimized { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AutoScroll { get; set; }

        public int MaxLines { get; set; }

        internal Form TargetForm { get; set; }

        internal System.Windows.Controls.RichTextBox TargetRichTextBox { get; set; }

        internal bool CreatedForm { get; set; }

        protected override void InitializeTarget()
        {
            this.TargetRichTextBox = (System.Windows.Controls.RichTextBox)System.Windows.Application.Current.MainWindow.FindName(this.ControlName);
            //this.TargetForm = FormHelper.CreateForm(this.FormName, this.Width, this.Height, false, this.ShowMinimized, this.ToolWindow);
            //this.CreatedForm = true;

            /*var openFormByName = Application.OpenForms[this.FormName];
            if (openFormByName != null)
            {
                this.TargetForm = openFormByName;
                if (string.IsNullOrEmpty(this.ControlName))
                {
                    throw new NLogConfigurationException("Rich text box control name must be specified for " + this.GetType().Name + ".");
                }

                this.CreatedForm = false;
                this.TargetRichTextBox = FormHelper.FindControl<RichTextBox>(this.ControlName, this.TargetForm);

                if (this.TargetRichTextBox == null)
                {
                    throw new NLogConfigurationException("Rich text box control '" + this.ControlName + "' cannot be found on form '" + this.FormName + "'.");
                }
            }
            else
            {
                this.TargetForm = FormHelper.CreateForm(this.FormName, this.Width, this.Height, true, this.ShowMinimized, this.ToolWindow);
                this.TargetRichTextBox = FormHelper.CreateRichTextBox(this.ControlName, this.TargetForm);
                this.CreatedForm = true;
            }*/
        }

        protected override void CloseTarget()
        {
            if (this.CreatedForm)
            {
                this.TargetForm.Invoke((FormCloseDelegate)this.TargetForm.Close);
                this.TargetForm = null;
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            WpfRichTextBoxRowColoringRule matchingRule = null;

            foreach (WpfRichTextBoxRowColoringRule rr in this.RowColoringRules)
            {
                if (rr.CheckCondition(logEvent))
                {
                    matchingRule = rr;
                    break;
                }
            }

            if (this.UseDefaultRowColoringRules && matchingRule == null)
            {
                foreach (WpfRichTextBoxRowColoringRule rr in DefaultRowColoringRules)
                {
                    if (rr.CheckCondition(logEvent))
                    {
                        matchingRule = rr;
                        break;
                    }
                }
            }

            if (matchingRule == null)
            {
                matchingRule = WpfRichTextBoxRowColoringRule.Default;
            }

            string logMessage = this.Layout.Render(logEvent);

            //this.TargetRichTextBox.Invoke(new DelSendTheMessageToRichTextBox(this.SendTheMessageToRichTextBox), new object[] { logMessage, matchingRule }); 
            if (System.Windows.Application.Current.Dispatcher.CheckAccess() == false)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SendTheMessageToRichTextBox(logMessage, matchingRule);
                }));
            }
            else
            {
                SendTheMessageToRichTextBox(logMessage, matchingRule);
            }
        }

        private static Color GetColorFromString(string color, Brush defaultColor)
        {
            if (defaultColor == null) return Color.FromRgb(255, 255, 255); // This will set default background colour to white. 
            if (color == "Empty")
            {
                return (Color)colorConverter.ConvertFrom(defaultColor);
            }

            return (Color)colorConverter.ConvertFromString(color);
        }


        private void SendTheMessageToRichTextBox(string logMessage, WpfRichTextBoxRowColoringRule rule)
        {
            System.Windows.Controls.RichTextBox rtbx = this.TargetRichTextBox;

            TextRange tr = new TextRange(rtbx.Document.ContentEnd, rtbx.Document.ContentEnd);
            tr.Text = logMessage + "\n";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                new SolidColorBrush(GetColorFromString(rule.FontColor, (Brush)tr.GetPropertyValue(TextElement.ForegroundProperty)))
            );
            tr.ApplyPropertyValue(TextElement.BackgroundProperty,
                new SolidColorBrush(GetColorFromString(rule.BackgroundColor, (Brush)tr.GetPropertyValue(TextElement.BackgroundProperty)))
            );
            tr.ApplyPropertyValue(TextElement.FontStyleProperty, rule.Style);
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, rule.Weight);

            //int startIndex = rtbx.Text.Length;
            //rtbx.SelectionStart = startIndex;
            //rtbx.SelectionBackColor = GetColorFromString(rule.BackgroundColor, rtbx.BackColor);
            //rtbx.SelectionColor = GetColorFromString(rule.FontColor, rtbx.ForeColor);
            //rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ rule.Style);
            //rtbx.AppendText(logMessage + "\n");
            //rtbx.SelectionLength = rtbx.Text.Length - rtbx.SelectionStart;

            //// find word to color
            //foreach (RichTextBoxWordColoringRule wordRule in this.WordColoringRules)
            //{
            //    MatchCollection mc = wordRule.CompiledRegex.Matches(rtbx.Text, startIndex);
            //    foreach (Match m in mc)
            //    {
            //        rtbx.SelectionStart = m.Index;
            //        rtbx.SelectionLength = m.Length;
            //        rtbx.SelectionBackColor = GetColorFromString(wordRule.BackgroundColor, rtbx.BackColor);
            //        rtbx.SelectionColor = GetColorFromString(wordRule.FontColor, rtbx.ForeColor);
            //        rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ wordRule.Style);
            //    }
            //}

            if (this.MaxLines > 0)
            {
                this.lineCount++;
                if (this.lineCount > this.MaxLines)
                {
                    tr = new TextRange(rtbx.Document.ContentStart, rtbx.Document.ContentEnd);
                    tr.Text.Remove(0, tr.Text.IndexOf('\n'));
                    this.lineCount--;
                }
            }

            if (this.AutoScroll)
            {
                //rtbx.Select(rtbx.TextLength, 0);
                rtbx.ScrollToEnd();
            }
        }
    }
}
#endif