using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace AventasApi.Services.CloudinaryService
{
    public class CloudinaryFunciones
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryFunciones(string cloud, string apiKey, string secret)
        {
            var account = new Account(
                cloud,
                apiKey,
                secret);

            _cloudinary = new Cloudinary(account);
        }


        //public async Task<string> Upload(string imagen)
        //{
        //    byte[] imageBytes = Convert.FromBase64String(imagen);
        //    using (var ms = new MemoryStream(imageBytes))
        //    {
        //        var uploadParams = new ImageUploadParams()
        //        {
        //            File = new FileDescription("image", ms),
        //            Folder = "Incidencias"
        //        };

        //        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        //        return uploadResult.Url.AbsoluteUri;

        //    }
        //}

        //public async Task<string> Upload(string imagen)
        //{
        //    byte[] imageBytes = Convert.FromBase64String(imagen);
        //    long originalSize = imageBytes.Length;

        //    using (var ms = new MemoryStream(imageBytes))
        //    {
        //        using (var originalImage = Image.FromStream(ms))
        //        {
        //            // Correct orientation based on EXIF data
        //            using (var correctedImage = CorrectImageOrientation(originalImage))
        //            {
        //                int newWidth = 800; // Ajusta el ancho según sea necesario
        //                int newHeight = (int)(correctedImage.Height * (800.0 / correctedImage.Width)); // Mantener la proporción

        //                using (var resizedImage = new Bitmap(newWidth, newHeight))
        //                {
        //                    using (var graphics = Graphics.FromImage(resizedImage))
        //                    {
        //                        graphics.CompositingQuality = CompositingQuality.HighQuality;
        //                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //                        graphics.SmoothingMode = SmoothingMode.HighQuality;

        //                        graphics.DrawImage(correctedImage, 0, 0, newWidth, newHeight);

        //                        using (var resizedStream = new MemoryStream())
        //                        {
        //                            long quality = 80L; // Calidad inicial
        //                            EncoderParameters encoderParameters;
        //                            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
        //                                .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

        //                            do
        //                            {
        //                                resizedStream.SetLength(0); // Resetear el stream
        //                                encoderParameters = new EncoderParameters(1);
        //                                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

        //                                resizedImage.Save(resizedStream, codec, encoderParameters);
        //                                quality -= 5; // Reducir calidad gradualmente
        //                            }
        //                            while (resizedStream.Length > originalSize && quality > 10); // Continuar si el tamaño es mayor que el original y la calidad es aceptable

        //                            resizedStream.Seek(0, SeekOrigin.Begin);

        //                            var uploadParams = new ImageUploadParams()
        //                            {
        //                                File = new FileDescription("image", resizedStream),
        //                                Folder = "Incidencias"
        //                            };

        //                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        //                            return uploadResult.Url.AbsoluteUri;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        public async Task<string> Upload(string imagen)
        {
            byte[] imageBytes = Convert.FromBase64String(imagen);

            using (var ms = new MemoryStream(imageBytes))
            {
                using (var originalImage = Image.FromStream(ms))
                {
                    // Corrige la orientación de la imagen basada en los datos EXIF
                    using (var correctedImage = CorrectImageOrientation(originalImage))
                    {
                        // Eliminar los datos EXIF sin recortar la imagen
                        using (var imageNoExif = RemoveExifData(correctedImage))
                        {
                            using (var compressedStream = new MemoryStream())
                            {
                                long quality = 80L; // Calidad inicial
                                EncoderParameters encoderParameters;
                                ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                                    .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

                                // Guarda la imagen en el MemoryStream con compresión
                                do
                                {
                                    compressedStream.SetLength(0); // Resetear el stream
                                    encoderParameters = new EncoderParameters(1);
                                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                                    imageNoExif.Save(compressedStream, codec, encoderParameters);
                                    quality -= 5; // Reducir calidad gradualmente
                                }
                                while (compressedStream.Length > imageBytes.Length && quality > 10); // Continuar si el tamaño es mayor que el original y la calidad es aceptable

                                compressedStream.Seek(0, SeekOrigin.Begin);

                                var uploadParams = new ImageUploadParams()
                                {
                                    File = new FileDescription("image", compressedStream),
                                    Folder = "Incidencias"
                                };

                                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                                return uploadResult.Url.AbsoluteUri;
                            }
                        }
                    }
                }
            }
        }

        private static Image CorrectImageOrientation(Image image)
        {
            if (image.PropertyIdList.Contains(0x112)) // 0x112 es el identificador de la propiedad de orientación
            {
                var orientation = (int)image.GetPropertyItem(0x112).Value[0];
                switch (orientation)
                {
                    case 1:
                        // Normal
                        break;
                    case 2:
                        // Flip horizontal
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        // Rotación 180 grados
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        // Flip vertical
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        // Rotación 90 grados + Flip horizontal
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        // Rotación 90 grados
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        // Rotación 270 grados + Flip horizontal
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        // Rotación 270 grados
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
            }
            return image;
        }

        private static Image RemoveExifData(Image image)
        {
            // Crear una nueva imagen sin los datos EXIF
            var newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.Clear(Color.White); // Opcional: establece el fondo en blanco o el color deseado
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            return newImage;
        }
        public async Task<bool> DeleteImageByUrl(string imageUrl)
        {
            string publicId = GetPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            var deletionParams = new DeletionParams(publicId)
            {
                PublicId = publicId,
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;

        }

        static string GetPublicIdFromUrl(string url)
        {
            // Expresión regular ajustada para manejar carpetas y extraer public_id sin la extensión
            var regex = new Regex(@"upload/v\d+/(?<publicId>.+?)\.[a-z]+$", RegexOptions.IgnoreCase);
            var match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups["publicId"].Value;
            }

            return null;
        }        
    }
}