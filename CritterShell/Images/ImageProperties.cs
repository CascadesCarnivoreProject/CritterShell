using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.Images
{
    public class ImageProperties
    {
        public int BlackPixels { get; private set; }
        public UInt64 Hash { get; private set; }
        public string Path { get; set; }
        public int PixelHeight { get; private set; }
        public int PixelWidth { get; private set; }
        public int WhitePixels { get; private set; }

        public ImageProperties(WriteableBitmap image)
        {
            this.BlackPixels = 0;
            unsafe
            {
                byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                int backBufferSize = image.GetSizeInBytes(0);
                this.Hash = XXHash64.Hash(backBuffer, backBufferSize);
            }
            this.Path = null;
            this.PixelHeight = image.PixelHeight;
            this.PixelWidth = image.PixelWidth;
            this.WhitePixels = 0;

            // count black and white pixels
            if ((image.Format == PixelFormats.Bgr24) ||
                (image.Format == PixelFormats.Bgr32) ||
                (image.Format == PixelFormats.Bgra32) ||
                (image.Format == PixelFormats.Pbgra32) ||
                (image.Format == PixelFormats.Rgb24))
            {
                int bytesPerPixel = image.GetBytesPerPixel();
                unsafe
                {
                    byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                    for (int row = 0; row < image.PixelHeight; ++row)
                    {
                        for (int pixel = 0; pixel < image.PixelWidth; ++pixel)
                        {
                            int pixelOffset = bytesPerPixel * pixel;
                            byte r = *(backBuffer + pixelOffset + 2);
                            byte g = *(backBuffer + pixelOffset + 1);
                            byte b = *(backBuffer + pixelOffset);

                            if ((r == 0) && (g == 0) && (b == 0))
                            {
                                ++this.BlackPixels;
                            }
                            else if ((r == 255) && (g == 255) && (b == 255))
                            {
                                ++this.WhitePixels;
                            }
                        }

                        backBuffer += image.BackBufferStride;
                    }
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                unsafe
                {
                    byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                    for (int row = 0; row < image.PixelHeight; ++row)
                    {
                        for (int pixel = 0; pixel < image.PixelWidth; ++pixel)
                        {
                            byte gray = *(backBuffer + pixel);
                            if (gray == 0)
                            {
                                ++this.BlackPixels;
                            }
                            else if (gray == 255)
                            {
                                ++this.WhitePixels;
                            }
                        }

                        backBuffer += image.BackBufferStride;
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(image), String.Format("Unhandled pixel format {0}.", image.Format));
            }
        }

        public int Pixels
        {
            get { return this.PixelWidth * this.PixelHeight; }
        }
    }
}
