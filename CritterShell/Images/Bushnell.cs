using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.Images
{
    internal class Bushnell
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Readability.")]
        public static Int32Rect FindTemperature(WriteableBitmap image)
        {
            if ((image.Format != PixelFormats.Bgr24) &&
                (image.Format != PixelFormats.Bgr32) &&
                (image.Format != PixelFormats.Bgra32) &&
                (image.Format != PixelFormats.Pbgra32))
            {
                throw new ArgumentOutOfRangeException(nameof(image), String.Format("Unhandled pixel format {0}.", image.Format));
            }

            // temperature format is nn°Fnn°C<moon phase>
            // This algorithm locates the Fnn part by
            //   1) Scanning backwards along the centerline of the info bar at the bottom of the image to find the first black pixel.
            //      This locates the right hand edge of the lunar phase symbol.  A search is necesseary because the exact position of
            //      the temperature information block varies with the pixel length of the camera's name, which is a function of both
            //      the length of the name and the specific characters used.
            //   2) Subtracting a fixed offset from the lunar edge position to locate the left edge of the temperature rectangle.
            //   3) Returning a temperature rectangle with the resulting left edge, a fixed height from the bottom of the image,
            //      and a fixed size.
            int bytesPerPixel = image.GetBytesPerPixel();
            int minimumPixel = Constant.Bushnell.TriggerPixelX + Constant.Bushnell.TriggerPixelWidth + Constant.Bushnell.CelsiusToLunarEdgePixelDistance;
            int searchRow = image.PixelHeight - Constant.Bushnell.InfoBarCenterlinePixelOffset;
            int lunarEdge = -1;
            unsafe
            {
                byte* backBuffer = (byte*)image.BackBuffer.ToPointer() + image.BackBufferStride * searchRow;
                for (int pixel = 2 * image.PixelWidth / 3; pixel > 0; --pixel)
                {
                    int sourcePixelOffset = bytesPerPixel * pixel;
                    int r = *(backBuffer + sourcePixelOffset + 2);
                    int g = *(backBuffer + sourcePixelOffset + 1);
                    int b = *(backBuffer + sourcePixelOffset);

                    if ((r < Constant.Bushnell.BlackPixelThreshold) && (g < Constant.Bushnell.BlackPixelThreshold) && (b < Constant.Bushnell.BlackPixelThreshold))
                    {
                        lunarEdge = pixel;
                        break;
                    }
                }
            }

            if (lunarEdge <= minimumPixel)
            {
                throw new ArgumentException(nameof(image), "Lunar phase indicator not found ");
            }

            return new Int32Rect(lunarEdge - Constant.Bushnell.CelsiusToLunarEdgePixelDistance, image.PixelHeight - Constant.Bushnell.TemperaturePixelYOffset, Constant.Bushnell.TemperaturePixelWidth, Constant.Bushnell.TemperaturePixelHeight);
        }

        public static Int32Rect FindTrigger(WriteableBitmap image)
        {
            return new Int32Rect(Constant.Bushnell.TriggerPixelX, image.PixelHeight - Constant.Bushnell.TriggerPixelYOffset, Constant.Bushnell.TriggerPixelWidth, Constant.Bushnell.TriggerPixelHeight);
        }
    }
}
