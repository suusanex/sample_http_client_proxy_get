using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace sample_http_client_proxy
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMainBtn(object sender, RoutedEventArgs e)
        {
            var outBuf = new StringBuilder();

            var proxy = WebRequest.GetSystemWebProxy();

            WebProxy webProxy = proxy as WebProxy;
            if (webProxy == null)
            {
                //.NET 4.7.2実装依存
                var field = proxy.GetType().GetField("webProxy",
                    BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    webProxy = (WebProxy)field.GetValue(proxy);
                }
            }

            if (webProxy == null)
            {
                return;
            }

            outBuf.AppendLine($"Address={webProxy.Address}");

            IFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, webProxy);

                var data = stream.ToArray();

                using (var stream2 = new MemoryStream(data))
                {
                    var proxy2 = (WebProxy)formatter.Deserialize(stream2);

                    outBuf.AppendLine($"Address={proxy2.Address}");

                }

            }

            MessageBox.Show(outBuf.ToString());
        }
    }
}
