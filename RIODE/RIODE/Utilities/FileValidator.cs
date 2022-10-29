using System;
using RIODE.Models;

namespace RIODE.Utilities
{
    public static class FileValidator
    {
        public static async Task<string> FileCreate(this IFormFile file,string root, string folder)
        {
            string fileName = string.Concat(Guid.NewGuid(), file.FileName);
            string path = Path.Combine(root, folder);
            string filePath = Path.Combine(path, fileName);

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception)
            {
                throw new FileLoadException();
            }

            return fileName;
        }


        public static void FileDelete(string root, string folder, string image)
        {
            string filePath = Path.Combine(root, folder, image);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static bool ImageIsOkay(this IFormFile file, int mb)
        {
            
            return file.Length / 1024 / 1024 < mb && file.ContentType.Contains("image/");
        }


    }
}

