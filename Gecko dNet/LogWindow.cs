using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeckoApp
{
    public partial class LogWindow : Form
    {
        public RichTextBox RichTextBox;
        public LogWindow()
        {
            InitializeComponent();
            RichTextBox = LogRichTextBox;
        }
    }
}
