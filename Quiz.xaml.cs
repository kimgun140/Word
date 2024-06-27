using Newtonsoft.Json;
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
using static System.Net.Mime.MediaTypeNames;

namespace Word
{
    /// <summary>
    /// Quiz.xaml에 대한 상호 작용 논리
    /// </summary>
    public class MyData2
    {
        public List<string> words { get; set; } = new List<string>();
        public List<string> means { get; set; } = new List<string>();

    }
    public partial class Quiz : Page
    {
        int cnt = 0;
        MyData2 MyData2 = new MyData2();
        Senddata quizdata = new Senddata();
        NetworkStream stream = Home.clients.GetStream();
        byte[] data = new byte[256];
        public Quiz()
        {
            data = null;
            data = Encoding.UTF8.GetBytes("문제풀기");
            stream.Write(data, 0, data.Length);

            InitializeComponent();
            txt_question.Text = "";
            Task.Run(() => Read_Quiz());
        }
        public async Task Read_Quiz()
        {
            int databyte = 0;
            while (true)
            {
                byte[] recv_data = new byte[2000];
                databyte = await stream.ReadAsync(recv_data, 0, recv_data.Length);
                string readdata = Encoding.UTF8.GetString(recv_data, 0, databyte);





                if (readdata == "데이터전송종료")
                {
                    break;
                }

                try
                {
                    Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(readdata);

                    if (dictionary != null)
                    {
                        foreach (var kvp in dictionary)
                        {
                            MyData2.words.Add(kvp.Key);
                            MyData2.means.Add(kvp.Value);

                        }
                        await Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            txt_question.Text = "";
                            txt_question.Text = MyData2.means[0];

                        }));

                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }));
                }
            }
        
        }
        private void txt_answer_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_answer.Clear();
        }

        private void LoadImageFromServer()
        {
            try
            {

                // 이미지 데이터 크기 수신
                byte[] sizeBuffer = new byte[sizeof(int)];
                //int bytes = stream.Read(data1, 0, data1.Length)
                stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                int imageSize = BitConverter.ToInt32(sizeBuffer.Reverse().ToArray(), 0);
                //sizeBuffer를 역순으로 변환한 후, BitConverter.ToInt32를 사용해 네트워크 바이트 순서에서 호스트 바이트 순서로 변환하여 imageSize에 저장
                // 역순으로 변환하는 이유는 바이트순서(엔디언 차이) 때문
                // 네트워크 바이트 순서는 빅 엔디언으로 인코딩 된다. 하지만 대부분의 pc는 리틀 엔딩언 방식이기에 변환해줘야 읽기 가능
                // 0x12345678을 리틀엔디언 - '78 56 34 12' 빅엔디언 '12 34 56 78' 저장 방식
                Debug.WriteLine("*******************");
                Debug.WriteLine(imageSize);

                byte[] buffer = new byte[4096];
                using (MemoryStream ms = new MemoryStream())
                // 메모리 스트림 ms 생성.
                // 이 스트림은 이미지 데이터를 메모리에 저장
                {
                    int bytesRead;
                    int totalBytesRead = 0;
                    while (totalBytesRead < imageSize && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    // 총 읽은 바이트수가 이미지 크기보다 작고, 바이트수를 계속 읽어올 수 있으면 루프
                    {
                        ms.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        //Debug.WriteLine("&&&&&&&&&&&&&&&&&&&&");
                        //Debug.WriteLine(bytesRead);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    // 스트림 포인터를 스트림 시작으로 이동
                    // 이전 코드에서 이미지를 수신하여 ms에 저장할떄 스크림 포인터는 스트림의 끝에 위치하게 됨.
                    // 이미지를 로드하려면 스트림의 시작부터 읽어야 하기에 스트림 포인터를 시작위치로 되돌림.
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    // BitmapImage 초기화
                    bitmap.StreamSource = ms;
                    // 메모리 스트림 ms를 BitmapImagedml 소스로 설정
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    // 이미지를 메모리에 로드하고, 스트림을 닫아도 이미지를 사용할 수 있도록 설정합니다.
                    bitmap.EndInit();
                    // 초기화 완료
                    img_read.Source = bitmap;
                    Debug.WriteLine("이미지 로드 완료");

                }
            }
            catch (IOException ex)
            {
              /*  MessageBox.Show($"이미지 로드 실패 - 네트워크 오류: {ex.Message}");*/
                Debug.WriteLine($"네트워크 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
             /*  MessageBox.Show($"이미지 로드 실패: {ex.Message}");*/
                Debug.WriteLine($"예외 발생: {ex.Message}");
            }
        }
        private async void txt_answer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                string answer = txt_answer.Text;
                string questiom = txt_question.Text;

                Dictionary<string, string> dict = new Dictionary<string, string>();

                dict.Add(answer, questiom);

                string sendquiz = JsonConvert.SerializeObject(dict);
                if (!string.IsNullOrEmpty(sendquiz))
                {
                    data = null;
                    data = Encoding.UTF8.GetBytes(sendquiz);
                    stream.Write(data, 0, data.Length);

                    txt_answer.Clear();

                    LoadImageFromServer();
                }
            }
        }

        private void btn_anwer_Click(object sender, RoutedEventArgs e)
        {
            data = null;
            data = Encoding.UTF8.GetBytes("답안제출");
            stream.Write(data, 0, data.Length);


        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (cnt < MyData2.words.Count - 1)
            {
                cnt++;
                txt_question.Text = "";
                txt_question.Text = MyData2.means[cnt];
            }
        }
    }
}
