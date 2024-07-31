using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using System.Data;
using System.IO;
namespace DAL
{
    public class SqlConnectionData
    {
        // Tạo chuỗi kết nối cơ sở dữ liệu
        public static SqlConnection Connect()
        {
            string strcon = @"Data Source=ADMIN\SQL_SERVER;Initial Catalog=QuanLyAmThanh;Integrated Security=True";
            SqlConnection conn = new SqlConnection(strcon);
            return conn;
        }
    }

    public class database
    {
        public void InsertAudioFile(int id, string filename, string time, string filePath)
        {
            byte[] audioData = File.ReadAllBytes(filePath);

            using (SqlConnection conn = SqlConnectionData.Connect())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO AudioTable (id, filename, time, fileamthanh) VALUES (@id, @filename, @time, @fileamthanh)", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@filename", filename);
                    cmd.Parameters.AddWithValue("@time", time);
                    cmd.Parameters.AddWithValue("@fileamthanh", audioData);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Phương thức để lấy file âm thanh từ cơ sở dữ liệu
        public byte[] GetAudioFile(string filename)
        {
            byte[] audioData = null;

            using (SqlConnection conn = SqlConnectionData.Connect())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT fileamthanh FROM AudioTable WHERE filename = @filename", conn))
                {
                    cmd.Parameters.AddWithValue("@filename", filename);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            audioData = (byte[])reader["fileamthanh"];
                        }
                    }
                }
            }

            return audioData;
        }


        // Phương thức để phát file âm thanh
        /*public void PlayAudioFile(byte[] audioData)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "tempPlayAudio.wav");
            File.WriteAllBytes(tempFilePath, audioData);

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(tempFilePath);
            player.Play();
        }*/

        public int GetNextAvailableId()
        {
            int maxId = 0;

            using (SqlConnection conn = SqlConnectionData.Connect())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(id), 0) FROM AudioTable", conn))
                {
                    maxId = (int)cmd.ExecuteScalar();
                }
            }

            // ID tiếp theo sẽ là ID tối đa hiện có + 1
            return maxId + 1;
        }
        public DataTable GetAudioFiles()
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection conn = SqlConnectionData.Connect())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, filename, time FROM AudioTable", conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }
        /*public string GetTimeByFilename(string filename)
        {
            string timeString = null;

            using (SqlConnection conn = SqlConnectionData.Connect())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT time FROM AudioTable WHERE filename = @filename", conn))
                {
                    cmd.Parameters.AddWithValue("@filename", filename);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        timeString = result.ToString(); // Trả về giá trị dưới dạng chuỗi
                    }
                }
            }
            return timeString;
        }*/
    }
}

