using System;
using System.Data.SqlClient;
using System.IO;

namespace NewsApp.Repository.Models
{
    public class ImageUpload
    {
        public static void InsertImageToDatabase(string connectionString, int newsId, string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                string sql = "INSERT INTO MEDIA (news_id, image) VALUES (@newsId, @image)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@newsId", newsId);
                    cmd.Parameters.AddWithValue("@image", imageBytes);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Inserted: news_id={newsId}");
        }

        public static void UploadAllImages(string connectionString)
        {
            string folderPath = @"C:\Images";

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder not found: " + folderPath);
                return;
            }

            string[] imageFiles = Directory.GetFiles(folderPath);

            int newsId = 1; 

            foreach (string imagePath in imageFiles)
            {
                InsertImageToDatabase(connectionString, newsId, imagePath);
                newsId++;
            }

            Console.WriteLine("All images uploaded.");
        }
    }
}
