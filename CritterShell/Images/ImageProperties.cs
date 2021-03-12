using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.Images
{
    public class ImageProperties
    {
        public int BlackPixels { get; private set; }
        public UInt32 Hash32 { get; private set; }
        public UInt64 Hash64 { get; private set; }
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
                this.Hash32 = XXHash.Hash32(backBuffer, backBufferSize);
                this.Hash64 = XXHash.Hash64(backBuffer, backBufferSize);
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
            else if (image.Format == PixelFormats.BlackWhite)
            {
                unsafe
                {
                    byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                    int bytesPerRow = (image.PixelWidth + 8 - 1) / 8;
                    int bitsInLastByteOfRow = image.PixelWidth - 8 * (image.PixelWidth / 8);
                    for (int row = 0; row < image.PixelHeight; ++row)
                    {
                        for (int offset = 0; offset < bytesPerRow; ++offset)
                        {
                            // if needed, this can be made faster by taking more than one byte at a time
                            int pixelValue = *(backBuffer + offset);
                            pixelValue -= ((pixelValue >> 1) & 0x55555555);
                            pixelValue = (pixelValue & 0x33333333) + ((pixelValue >> 2) & 0x33333333);
                            int whitePixels = ((pixelValue + (pixelValue >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;

                            int blackPixels = 8 - whitePixels;
                            if (offset == (bytesPerRow - 1))
                            {
                                // Min() isn't needed for images produced by WriteableBitmapExtensions.Convert() as unused bits
                                // are zeroed but is required in the general case.
                                whitePixels = Math.Min(whitePixels, bitsInLastByteOfRow);
                                blackPixels = bitsInLastByteOfRow - whitePixels;
                            }

                            Debug.Assert((0 <= whitePixels) && (whitePixels <= 8), "White pixel count out of range.");
                            Debug.Assert((0 <= blackPixels) && (blackPixels <= 8), "Black pixel count out of range.");

                            this.BlackPixels += blackPixels;
                            this.WhitePixels += whitePixels;
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
                            byte pixelValue = *(backBuffer + pixel);
                            if (pixelValue == 0)
                            {
                                ++this.BlackPixels;
                            }
                            else if (pixelValue == 255)
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

            Debug.Assert((this.BlackPixels + this.WhitePixels) <= this.Pixels, "Pixel counts out of range.");
        }

        public int Pixels
        {
            get { return this.PixelWidth * this.PixelHeight; }
        }

        public bool Equals(ImageProperties other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.Hash32 != other.Hash32)
            {
                return false;
            }
            if (this.Hash64 != other.Hash64)
            {
                return false;
            }

            if (this.BlackPixels != other.BlackPixels)
            {
                return false;
            }
            if (this.PixelHeight != other.PixelHeight)
            {
                return false;
            }
            if (this.PixelWidth != other.PixelWidth)
            {
                return false;
            }
            if (this.WhitePixels != other.WhitePixels)
            {
                return false;
            }

            if (String.Equals(this.Path, other.Path, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageProperties other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = (int)this.Hash32;
            hash = (hash << 5) + hash ^ this.Hash64.GetHashCode();

            hash = (hash << 5) + hash ^ this.BlackPixels;
            hash = (hash << 5) + hash ^ this.PixelHeight;
            hash = (hash << 5) + hash ^ this.PixelWidth;
            hash = (hash << 5) + hash ^ this.WhitePixels;

            if (this.Path != null)
            {
                hash = (hash << 5) + hash ^ this.Path.GetHashCode();
            }
            return hash;
        }

        public static bool operator !=(ImageProperties properties1, ImageProperties properties2)
        {
            return !(properties1 == properties2);
        }

        public static bool operator ==(ImageProperties properties1, ImageProperties properties2)
        {
            if (Object.ReferenceEquals(properties1, properties2))
            {
                return true;
            }
            if (Object.ReferenceEquals(properties1, null))
            {
                return false;
            }

            return properties1.Equals(properties2);
        }
    }
}
