using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            var proxy = ProxyGetTest(outBuf);

            MessageBox.Show(outBuf.ToString());


            var cli = WebRequest.Create(m_TestUri);
            cli.Proxy = proxy;
            cli.Method = "GET";
            using (var res = cli.GetResponse())
            using (var resStream = new StreamReader(res.GetResponseStream()))
            {
                MessageBox.Show($"{resStream.ReadToEnd().Substring(0, 300)}");
            }
        }

        static IWebProxy ProxyGetTest(StringBuilder outBuf)
        {
            return ProxyGet(outBuf);
        }
        static IWebProxy ProxyGet(StringBuilder outBuf)
        {

            var proxy = WebRequest.GetSystemWebProxy();
            outBuf.AppendLine($"Address={proxy.GetProxy(new Uri(m_TestUri))}");
            return proxy;
        }


        private static WebProxy ProxyGetAndSerialize(StringBuilder outBuf)
        {
            //この関数のようにバイナリシリアライズする方式では、プロキシのアドレスが直接指定されている場合は設定を保存できるが、WPADの自動検出やPACファイルでの指定の場合は設定を保存できなかった。

            var proxy = WebRequest.GetSystemWebProxy();

            WebProxy webProxy = proxy as WebProxy;
            if (webProxy == null)
            {
                //.NET 4.7.2実装依存
                var field = proxy.GetType().GetField("webProxy",
                    BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    webProxy = (WebProxy) field.GetValue(proxy);
                }
            }

            if (webProxy == null)
            {
                return null;
            }

            outBuf.AppendLine($"Address={webProxy.Address}");

            IFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, webProxy);

                var data = stream.ToArray();

                using (var stream2 = new MemoryStream(data))
                {
                    var proxy2 = (WebProxy) formatter.Deserialize(stream2);

                    outBuf.AppendLine($"Address={proxy2.Address}");

                    return proxy2;
                }
            }

        }

        private async void OnOldTypeBtn(object sender, RoutedEventArgs e)
        {

            var outBuf = new StringBuilder();

            var proxy = ProxyGetTest(outBuf);

            MessageBox.Show(outBuf.ToString());

            var handler = new HttpClientHandler()
            {
                Proxy = proxy,
            };

            using (var cli = new HttpClient(handler))
            {

                using (var res = await cli.GetAsync(m_TestUri))
                {
                    MessageBox.Show($"{(await res.Content.ReadAsStringAsync()).Substring(0, 300)}");
                }
            }
        }

        private static string m_TestUri = "https://www.google.co.jp/";

        private void OnMainBtnDirectProxy(object sender, RoutedEventArgs e)
        {
            var outBuf = new StringBuilder();

            var proxy = ProxyGetTest(outBuf);

            MessageBox.Show(outBuf.ToString());


            var cli = WebRequest.Create(m_TestUri);
            if (proxy.IsBypassed(new Uri(m_TestUri)))
            {
                cli.Proxy = null;
            }
            else
            {
                cli.Proxy = new WebProxy(proxy.GetProxy(new Uri(m_TestUri)));
            }

            cli.Method = "GET";
            using (var res = cli.GetResponse())
            using (var resStream = new StreamReader(res.GetResponseStream()))
            {
                MessageBox.Show($"{resStream.ReadToEnd().Substring(0, 300)}");
            }
            
        }

        private async void OnOldTypeBtnDirectProxy(object sender, RoutedEventArgs e)
        {

            var outBuf = new StringBuilder();

            var proxy = ProxyGetTest(outBuf);

            MessageBox.Show(outBuf.ToString());

            var handler = new HttpClientHandler();

            if (proxy.IsBypassed(new Uri(m_TestUri)))
            {
                handler.Proxy = null;
                handler.UseProxy = false;
            }
            else
            {
                handler.Proxy = new WebProxy(proxy.GetProxy(new Uri(m_TestUri)));
                handler.UseProxy = true;
            }

            using (var cli = new HttpClient(handler))
            {
                using (var res = await cli.GetAsync(m_TestUri))
                {
                    MessageBox.Show($"{(await res.Content.ReadAsStringAsync()).Substring(0, 300)}");
                }
            }
        }
    }
}
