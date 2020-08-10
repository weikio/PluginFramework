using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;
using Weikio.PluginFramework.Samples.Shared;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // 1. Uses folder catalog to add calculation operations inside the app. Mimics the WPF-sample.
            await AddCalculationOperators();

            // 2. Another folder catalog is used to add other forms inside the app. For each form a button is added into the toolstrip
            // Note: WinFormsPluginsLibrary must be manually built as it isn't referenced by this sample app
            // Note: Program.cs contains an initialization code which is required. Without it, we will get an exception about a missing reference. This is something we hope to get around in the future versions of Plugin Framework.
            await AddDialogs();

            // 3. Lastly, DelegateCatalog is used for creating an exit-button
            await AddExitButton();
        }

        private async Task AddCalculationOperators()
        {
            var folderPluginCatalog = new FolderPluginCatalog(@"..\..\..\..\Shared\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1",
                type => { type.Implements<IOperator>(); });

            var assemblyPluginCatalog = new AssemblyPluginCatalog(typeof(Form1).Assembly, type => typeof(IOperator).IsAssignableFrom(type));

            var pluginCatalog = new CompositePluginCatalog(folderPluginCatalog, assemblyPluginCatalog);
            await pluginCatalog.Initialize();

            var allPlugins = pluginCatalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                listBox1.Items.Add(plugin);
            }
        }
        
        private async Task AddDialogs()
        {
            var folderCatalog = new FolderPluginCatalog(@"..\..\..\..\WinFormsPluginsLibrary\bin\debug\netcoreapp3.1",
                type => { type.Implements<IDialog>(); });

            await folderCatalog.Initialize();

            foreach (var dialogPlugin in folderCatalog.GetPlugins())
            {
                var menuItem = new ToolStripButton(dialogPlugin.Name, null, (o, args) =>
                {
                    var instance = (IDialog) Activator.CreateInstance(dialogPlugin);
                    instance.Show();
                });

                dialogPluginsToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

       
        private async Task AddExitButton()
        {
            var exitAction = new Action(() =>
            {
                var result = MessageBox.Show("This is also a plugin. Do you want to exit this sample app?", "Exit App?", MessageBoxButtons.YesNo);

                if (result == DialogResult.No)
                {
                    return;
                }
                
                Application.Exit();
            });
            
            var exitCatalog = new DelegatePluginCatalog(exitAction, "Exit");
            await exitCatalog.Initialize();

            var exitPlugin = exitCatalog.Single();

            menuStrip1.Items.Add(new ToolStripButton(exitPlugin.Name, null, (sender, args) =>
            {
                dynamic instance = Activator.CreateInstance(exitPlugin);
                instance.Run();
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var selectedPlugin = listBox1.SelectedItem as Plugin;

            if (selectedPlugin == null)
            {
                return;
            }

            var instance = (IOperator) Activator.CreateInstance(selectedPlugin);

            var result = instance.Calculate(20, 10);

            textBox1.Text = result.ToString();
        }
    }
}
