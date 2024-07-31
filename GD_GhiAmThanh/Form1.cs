using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using NAudio.Wave;
using System.Diagnostics;
using DAL;


namespace GD_GhiAmThanh
{
    public partial class Form1 : Form
    {
        private database db = new database();
        private string tempFilePath = Path.Combine(Path.GetTempPath(), "tempAudio.wav");
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;
        private Stopwatch count = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            LoadAudioFiles();
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int record(string ipstrCommand, string ipstrReturnString, int uReturnLenth, int hwndCallback);
        
        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void recorderBox_Click(object sender, EventArgs e)
        {
            record("open new Type waveaudio Alias recSound", "", 0, 0);
            record("record recSound", "", 0, 0);
            timer1.Start();
            count.Start();
            lblRecord.Text = "Rec...";
        }

        private void stopBox_Click(object sender, EventArgs e)
        {
            // Lưu âm thanh vào file tạm thời
            record("save recSound " + tempFilePath, "", 0, 0);
            record("close recSound", "", 0, 0);
            timer1.Stop();
            count.Stop();
            lblRecord.Text = "Recorded";

            // Lấy ID tối đa hiện có trong cơ sở dữ liệu
            int nextId = db.GetNextAvailableId();
            // Tạo tên file là "audio.wav" với ID
            string filename = "audio" + nextId + ".wav";
            string time = labelTimer.Text;
            // Lưu âm thanh vào cơ sở dữ liệu với ID tiếp theo
            db.InsertAudioFile(nextId, filename, time, tempFilePath);
            count.Reset(); // Reset đồng hồ bấm giờ
            labelTimer.Text = "00:00:00"; // Đặt lại giá trị hiển thị thời gian
            LoadAudioFiles();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = count.Elapsed;
            labelTimer.Text = string.Format("{0:00}:{1:00}:{2:00}", Math.Floor(elapsed.TotalHours), elapsed.Minutes, elapsed.Seconds);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            // Kiểm tra nếu có mục được chọn trong ListBox
            if (listBox1.SelectedItem != null)
            {
                // Lấy tên file từ mục được chọn
                string filename = listBox1.SelectedItem.ToString();

                // Lấy dữ liệu âm thanh từ cơ sở dữ liệu dựa trên filename
                byte[] audioData = db.GetAudioFile(filename);

                if (audioData != null)
                {
                    // Phát âm thanh từ dữ liệu âm thanh lấy được
                    PlayAudioFile(audioData);
                    lblRecord.Text = "Playing...";
                    timer1.Start();
                    count.Start();
                }
                else
                {
                    // Hiển thị thông báo lỗi nếu không tìm thấy file âm thanh
                    MessageBox.Show("Không tìm thấy file âm thanh trong cơ sở dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadAudioFiles()
        {
            // Xóa tất cả các mục hiện có trong ListBox
            listBox1.Items.Clear();

            // Lấy danh sách các file âm thanh từ cơ sở dữ liệu
            DataTable audioFiles = db.GetAudioFiles();

            // Thêm tên file vào ListBox
            foreach (DataRow row in audioFiles.Rows)
            {
                string itemText = row["filename"].ToString(); // Lấy tên file từ cột "filename"
                listBox1.Items.Add(itemText);
            }
        }

        private void PlayAudioFile(byte[] audioData)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "tempPlayAudio.wav");

            // Đảm bảo wavePlayer và audioFileReader được giải phóng trước khi ghi tệp mới
            StopAndDisposePlayer();

            File.WriteAllBytes(tempFilePath, audioData);

            wavePlayer = new WaveOutEvent();
            audioFileReader = new AudioFileReader(tempFilePath);

            wavePlayer.Init(audioFileReader);
            wavePlayer.Play();
            wavePlayer.PlaybackStopped += OnPlaybackStopped;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Ngừng và reset timer khi âm thanh kết thúc
            timer1.Stop();
            count.Stop();
            count.Reset();
            labelTimer.Text = "00:00:00";
            lblRecord.Text = "Stopped";

            // Đảm bảo wavePlayer và audioFileReader được giải phóng
            StopAndDisposePlayer();
        }

        private void StopAndDisposePlayer()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Stop();
                wavePlayer.Dispose();
                wavePlayer = null;
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }
    }
}
