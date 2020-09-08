using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.TypeFinding;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        private CompositePluginCatalog _allPlugins = new CompositePluginCatalog();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Demonstrates how tags can be used with Plugin Framework.
            // Single _allPlugins catalog is created from multiple different catalogs. All the plugins are tagged.
            await CreateCatalogs();
            
            // 1. Adds calculation operators using tag 'operator'
            AddCalculationOperators();

            // 2. Adds dialogs using 'dialog' tag
            AddDialogs();

            // 3. Adds buttons using 'button' tag
            AddButtons();
        }

        private async Task CreateCatalogs()
        {
            // 1. Uses folder catalog to add calculation operations inside the app. Mimics the WPF-sample.
            var folderPluginCatalog = new FolderPluginCatalog(@"..\..\..\..\Shared\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1",
                type => { type.Implements<IOperator>().Tag("operator"); });
            
            _allPlugins.AddCatalog(folderPluginCatalog);
            
            var assemblyPluginCatalog = new AssemblyPluginCatalog(typeof(Form1).Assembly, builder =>
            {
                builder.Implements<IOperator>()
                    .Tag("operator");
            });
            
            _allPlugins.AddCatalog(assemblyPluginCatalog);
            
            // 2. Another folder catalog is used to add other forms inside the app. For each form a button is added into the toolstrip
            // Note: WinFormsPluginsLibrary must be manually built as it isn't referenced by this sample app
            var folderCatalog = new FolderPluginCatalog(@"..\..\..\..\WinFormsPluginsLibrary\bin\debug\netcoreapp3.1",
                type =>
                {
                    type.Implements<IDialog>()
                        .Tag("dialog");
                });
            
            _allPlugins.AddCatalog(folderCatalog);
            
            // 3. Lastly, DelegateCatalog is used for creating an exit-button
            var exitAction = new Action(() =>
            {
                var result = MessageBox.Show("This is also a plugin. Do you want to exit this sample app?", "Exit App?", MessageBoxButtons.YesNo);
            
                if (result == DialogResult.No)
                {
                    return;
                }
            
                Application.Exit();
            });
            
            var exitCatalog = new DelegatePluginCatalog(exitAction, options: new DelegatePluginCatalogOptions(){Tags = new List<string> { "buttons" }} , pluginName: "Exit" );
            _allPlugins.AddCatalog(exitCatalog);

            // Finally the plugin catalog is initialized
            await _allPlugins.Initialize();
        }
        
        private void AddCalculationOperators()
        {
            var allPlugins = _allPlugins.GetByTag("operator");
            foreach (var plugin in allPlugins)
            {
                listBox1.Items.Add(plugin);
            }
        }

        private void AddDialogs()
        {
            var dialogPlugins = _allPlugins.GetByTag("dialog");
            
            foreach (var dialogPlugin in dialogPlugins)
            {
                var menuItem = new ToolStripButton(dialogPlugin.Name, null, (o, args) =>
                {
                    var instance = (IDialog) Activator.CreateInstance(dialogPlugin);
                    instance.Show();
                });

                dialogPluginsToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void AddButtons()
        {
            var buttonPlugins = _allPlugins.GetByTag("buttons");

            foreach (var buttonPlugin in buttonPlugins)
            {
                menuStrip1.Items.Add(new ToolStripButton(buttonPlugin.Name, null, (sender, args) =>
                {
                    dynamic instance = Activator.CreateInstance(buttonPlugin);
                    instance.Run();
                }));
            }
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
