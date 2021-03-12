using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.Images
{
    internal static class WriteableBitmapExtensions
    {
        public static WriteableBitmap Convert(this WriteableBitmap image, PixelFormat format)
        {
            if ((image.Format != PixelFormats.Bgr24) &&
                (image.Format != PixelFormats.Bgr32) &&
                (image.Format != PixelFormats.Bgra32) &&
                (image.Format != PixelFormats.Pbgra32))
            {
                throw new ArgumentOutOfRangeException(nameof(image), String.Format("Unhandled pixel format {0}.", image.Format));
            }
            if ((format != PixelFormats.BlackWhite) &&
                (format != PixelFormats.Gray8))
            {
                throw new ArgumentOutOfRangeException(nameof(format), String.Format("Unhandled pixel format {0}.", format));
            }

            // convert to grey
            WriteableBitmap grey = new WriteableBitmap(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY, PixelFormats.Gray8, null);
            int bgrxBytesPerPixel = image.GetBytesPerPixel();
            grey.Lock();
            unsafe
            {
                byte* bgrxBuffer = (byte*)image.BackBuffer.ToPointer();
                byte* greyBuffer = (byte*)grey.BackBuffer.ToPointer();
                for (int row = 0; row < image.PixelHeight; ++row)
                {
                    for (int pixel = 0; pixel < image.PixelWidth; ++pixel)
                    {
                        int sourcePixelOffset = bgrxBytesPerPixel * pixel;
                        int r = *(bgrxBuffer + sourcePixelOffset + 2);
                        int g = *(bgrxBuffer + sourcePixelOffset + 1);
                        int b = *(bgrxBuffer + sourcePixelOffset);

                        byte luminosity = (byte)((77 * r + 150 * g + 29 * b + 128) >> 8);
                        *(greyBuffer + pixel) = luminosity;
                    }

                    bgrxBuffer += image.BackBufferStride;
                    greyBuffer += grey.BackBufferStride;
                }
            }
            grey.Unlock();

            if (format == PixelFormats.Gray8)
            {
                return grey;
            }
            grey.Freeze();

            // convert to monochrome
            WriteableBitmap monochrome = new WriteableBitmap(grey.PixelWidth, grey.PixelHeight, grey.DpiX, grey.DpiY, PixelFormats.BlackWhite, null);
            monochrome.Lock();
            unsafe
            {
                byte* greyBuffer = (byte*)grey.BackBuffer.ToPointer();
                byte* monochromeBuffer = (byte*)monochrome.BackBuffer.ToPointer();
                for (int row = 0; row < grey.PixelHeight; ++row)
                {
                    // if needed, the below loops can be made faster by taking more than one byte at a time
                    for (int offset = 0; offset < monochrome.BackBufferStride; ++offset)
                    {
                        // zero row for ORing in of pixels below
                        // Zeroing any unused bytes at the end of the stride is not strictly necessary but is done for simplicity.
                        *(monochromeBuffer + offset) = 0;
                    }

                    for (int pixel = 0; pixel < grey.PixelWidth; ++pixel)
                    {
                        int pixelValue = *(greyBuffer + pixel);
                        if ((pixelValue != 0) && (pixelValue != 255))
                        {
                            throw new ArgumentOutOfRangeException(nameof(image), "Image contains pixels with luminosities other than pure black (0x00) or pure white (0xff).");
                        }

                        // convert 0xff to 0x01
                        pixelValue = (pixelValue & 0x80) >> 7;
                        // move bit for pixel into position and or into output byte
                        // PngBitmapEncoder interprets bits little endian, so the first pixel in the byte is indicated by the most
                        // significant bit.
                        *(monochromeBuffer + pixel / 8) |= (byte)(pixelValue << (7 - (pixel % 8)));
                    }

                    greyBuffer += grey.BackBufferStride;
                    monochromeBuffer += monochrome.BackBufferStride;
                }
            }
            monochrome.Unlock();

            return monochrome;
        }

        public static WriteableBitmap ExtractRectangle(this WriteableBitmap image, Int32Rect rectangle)
        {
            if ((rectangle.X < 0) ||
                (rectangle.Width < 0) ||
                (rectangle.X + rectangle.Width > image.PixelWidth) ||
                (rectangle.Y < 0) ||
                (rectangle.Height < 0) ||
                (rectangle.Y + rectangle.Height > image.PixelHeight))
            {
                throw new ArgumentOutOfRangeException(nameof(rectangle));
            }

            WriteableBitmap extractedPixels = new WriteableBitmap(rectangle.Width, rectangle.Height, image.DpiX, image.DpiY, image.Format, image.Palette);
            extractedPixels.Lock();
            image.CopyPixels(rectangle, extractedPixels.BackBuffer, extractedPixels.BackBufferStride * extractedPixels.PixelHeight, extractedPixels.BackBufferStride);
            extractedPixels.Unlock();

            return extractedPixels;
        }

        public static int GetBytesPerPixel(this WriteableBitmap image)
        {
            return image.Format.BitsPerPixel / 8;
        }

        public static ImageProperties GetProperties(this WriteableBitmap image)
        {
            return new ImageProperties(image);
        }

        public static int GetSizeInBytes(this WriteableBitmap image, int bottomRowsToSkip)
        {
            if (image.PixelHeight <= bottomRowsToSkip)
            {
                throw new ArgumentOutOfRangeException(nameof(bottomRowsToSkip), "Number of rows to skip must be less than the image height.");
            }
            return image.BackBufferStride * (image.PixelHeight - bottomRowsToSkip);
        }

        public static void Save(this WriteableBitmap image, string filePath)
        {
            BitmapEncoder encoder;
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case Constant.File.JpgExtension:
                case Constant.File.JpegExtension:
                    encoder = new JpegBitmapEncoder();
                    break;
                case Constant.File.PngExtension:
                    encoder = new PngBitmapEncoder();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filePath), "Path must indicate an image file in JPEG (.jpg, .jpeg) or PNG (.png) format.");
            }

            encoder.Frames.Add(BitmapFrame.Create(image));
            using (FileStream outStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(outStream);
            }
        }

        public static void Threshold(this WriteableBitmap image, int threshold)
        {
            if ((image.Format != PixelFormats.Bgr24) &&
                (image.Format != PixelFormats.Bgr32) &&
                (image.Format != PixelFormats.Bgra32) &&
                (image.Format != PixelFormats.Pbgra32))
            {
                throw new ArgumentOutOfRangeException(nameof(image), String.Format("Unhandled pixel format {0}.", image.Format));
            }

            int bytesPerPixel = image.GetBytesPerPixel();
            image.Lock();
            unsafe
            {
                byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                for (int row = 0; row < image.PixelHeight; ++row)
                {
                    for (int pixel = 0; pixel < image.PixelWidth; ++pixel)
                    {
                        int pixelOffset = bytesPerPixel * pixel;
                        int r = *(backBuffer + pixelOffset + 2);
                        int g = *(backBuffer + pixelOffset + 1);
                        int b = *(backBuffer + pixelOffset);

                        // see remarks in ImageHistogram..ctor();
                        int luminosity = (77 * r + 150 * g + 29 * b + 128) >> 8;
                        if (luminosity > threshold)
                        {
                            *(backBuffer + pixelOffset + 2) = 255;
                            *(backBuffer + pixelOffset + 1) = 255;
                            *(backBuffer + pixelOffset) = 255;
                        }
                        else
                        {
                            *(backBuffer + pixelOffset + 2) = 0;
                            *(backBuffer + pixelOffset + 1) = 0;
                            *(backBuffer + pixelOffset) = 0;
                        }
                    }

                    backBuffer += image.BackBufferStride;
                }
            }
            image.Unlock();
        }
    }
}
