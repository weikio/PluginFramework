using System;
using System.ComponentModel;
using System.Windows.Forms;
using Weikio.PluginFramework.Samples.Shared;

namespace WinFormsPluginsLibrary
{
    public partial class LabelPlugin : Form, IDialog
    {
        public LabelPlugin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Another simple example";
        }
    }
}

