using System;
using System.IO;
using System.Management.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell
{
    public abstract class ImageCmdlet : CritterCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Path to the .jpg file to read.")]
        [ValidateNotNullOrEmpty]
        public string Image { get; set; }

        protected void Initialize()
        {
            this.Image = this.CanonicalizePath(this.Image);
            if (File.Exists(this.Image) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(Image), "File indicated by -Image does not exist.");
            }
        }

        protected WriteableBitmap ReadImage(double scale)
        {
            using (FileStream imageStream = new FileStream(this.Image, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                JpegBitmapDecoder jpegDecoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                BitmapSource image = jpegDecoder.Frames[0];
                if (scale != 1.0)
                {
                    image = new TransformedBitmap(image, new ScaleTransform(scale, scale));
                }
                return new WriteableBitmap(image);
            }
        }
    }
}
