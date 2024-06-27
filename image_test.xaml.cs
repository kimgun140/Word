using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// image_test.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class image_test : Page
    {
             NetworkStream stream = Home.clients.GetStream();

        public image_test()
        {
            InitializeComponent();
        }


       

    
}
}
