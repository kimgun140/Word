using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace Word
{
    /// <summary>
    /// History.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class History : Page
    {
        NetworkStream stream = Home.clients.GetStream();
        byte[] data = new byte[256];
        public History()
        {
            data = null;
            data = Encoding.UTF8.GetBytes("문제풀이불러오기");
            stream.Write(data, 0, data.Length);

            InitializeComponent();
        }
    }
}
