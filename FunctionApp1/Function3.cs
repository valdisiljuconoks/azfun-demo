using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Shared.Models;

namespace FunctionApp1
{
    public static class Function3
    {
        [FunctionName("Function3")]
        [return: Queue("done", Connection = "MyStorageConnection")]
        public static async Task<CloudQueueMessage> Run(
            [QueueTrigger("to-fun3", Connection = "MyStorageConnection")] AsciiArtRequest request,
            [Blob("container/{BlobRef}", Connection = "MyStorageConnection")] CloudBlockBlob inBlob,
            [Blob("container", Connection = "MyStorageConnection")] CloudBlobContainer outContainer,
            TraceWriter log)
        {
            log.Info("Making ASCII art...");

            using (var stream = new MemoryStream())
            {
                await inBlob.DownloadToStreamAsync(stream);
                var result = ConvertImageToAscii(stream);

                using (var upstream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    var resultBlob = $"{request.BlobRef}-done";
                    var blob = outContainer.GetBlockBlobReference(resultBlob);
                    await blob.UploadFromStreamAsync(upstream);

                    return new AsciiArtResult(resultBlob, request.BlobRef, request.Description, request.Tags).AsQueueItem();
                }
            }
        }

        // Copyright: Code for ASCII convert used from http://www.c-sharpcorner.com/article/generating-ascii-art-from-an-image-using-C-Sharp/
        private static readonly string[] _AsciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", "&nbsp;" };

        private static string ConvertImageToAscii(Stream image)
        {
            var bitmap = new Bitmap(image, true);
            bitmap = GetReSizedImage(bitmap, 100);
            return ConvertToAscii(bitmap);
        }

        private static Bitmap GetReSizedImage(Image inputBitmap, int asciiWidth)
        {
            var asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);
            var result = new Bitmap(asciiWidth, asciiHeight);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
                return result;
            }
        }

        private static string ConvertToAscii(Bitmap image)
        {
            var sb = new StringBuilder();
            var toggle = false;

            for (var h = 0; h < image.Height; h++)
            {
                for (var w = 0; w < image.Width; w++)
                {
                    var pixelColor = image.GetPixel(w, h);
                    var red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    var green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    var blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    var grayColor = Color.FromArgb(red, green, blue);

                    if(toggle)
                        continue;

                    var index = grayColor.R * 10 / 255;
                    sb.Append(_AsciiChars[index]);
                }

                if(!toggle)
                {
                    sb.Append("<BR>");
                    toggle = true;
                }
                else
                    toggle = false;
            }

            return sb.ToString();
        }
    }
}
