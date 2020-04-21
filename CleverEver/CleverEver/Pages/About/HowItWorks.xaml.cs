using CleverEver.Analytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.About
{
    public partial class HowItWorks : ContentPage, IIdentifiable
    {
        public HowItWorks()
        {
            InitializeComponent();

            var assembly = typeof(HowItWorks).GetTypeInfo().Assembly;
            var content = assembly.GetManifestResourceNames().First(li => li.EndsWith("HowItWorks.html"));

            using (var reader = new StreamReader(assembly.GetManifestResourceStream(content)))
            {
                var source = new HtmlWebViewSource() { Html = reader.ReadToEnd() };
                web.Source = source;
            }
        }

        string IIdentifiable.Id
        {
            get { return "About"; }
        }
    }
}
