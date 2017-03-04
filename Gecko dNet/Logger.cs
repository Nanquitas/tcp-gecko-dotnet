using System;
using System.Drawing;
using System.Windows.Forms;

namespace GeckoApp
{
    public class VLogger
    {
        private readonly Color alertColor = Color.FromArgb(0xD3, 0x54, 0);
        private readonly Color debugColor = Color.FromArgb(0x7F, 0x8C, 0x8D);
        private readonly Color errorColor = Color.FromArgb(0xC0, 0x39, 0x2B);
        private readonly Color infoColor = Color.FromArgb(0x27, 0xAE, 0x60);

        public VLogger(RichTextBox richTextBox)
        {
            RichTextBox = richTextBox;
        }

        public RichTextBox RichTextBox { get; set; }
        public void Add(string text, LoggerEnum loggerEnum = LoggerEnum.Info)
        {
            if (RichTextBox == null)
                return;

            if (!RichTextBox.InvokeRequired)
            {


               

                string log = String.Empty;
                Color color = Color.Black;
                switch (loggerEnum)
                {
                    case LoggerEnum.Alert:
                        color = alertColor;
                        break;
                    case LoggerEnum.Debug:
                        color = debugColor;
                        break;
                    case LoggerEnum.Error:
                        color = errorColor;
                        break;
                    case LoggerEnum.Info:
                        color = infoColor;
                        break;
                }

                string time = DateTime.Now.ToShortTimeString();
                RichTextBox.AppendText($"{time} - " + text, color, true);
            }
            else
                RichTextBox.Invoke(new Action(() => Add(text, loggerEnum)));
        }

        public void Error(Exception ex)
        {
            string message = $"{ex.Message}{Environment.NewLine}{ex.ToString()}";
            Add(message, LoggerEnum.Error);
        }

        public void Clear()
        {
            if (!RichTextBox.InvokeRequired)
                RichTextBox.Clear();
            else
                RichTextBox.Invoke(new Action(Clear));
        }
    }
}