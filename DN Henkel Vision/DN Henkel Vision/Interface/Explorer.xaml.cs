using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Explorer : Page
    {
        public string[] Header;
        public List<Exportite> Exportites;

        public Explorer()
        {
            Exportites = Exports();
            Header = Export.GetHeader(Manager.LaunchingFile);
            this.InitializeComponent();
        }

        public List<Exportite> Exports()
        {
            return Export.GetExport(Manager.LaunchingFile);
        }
    }

    /// <summary>
    /// Represents a class for storing information about an export.
    /// </summary>
    public class Exportite
    {
        public string Order;
        public string Placement;
        public string Description;
        public string Registrant;

        /// <summary>
        /// Initializes a new instance of the Exportite class using a raw string array.
        /// </summary>
        public Exportite(string[] raw)
        {
            Order = raw[0];
            Placement = raw[1];
            Description = raw[2];
            Registrant = raw[3];
        }
    }
}
