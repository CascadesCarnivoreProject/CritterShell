﻿using System;
using System.IO;
using System.Management.Automation;
using System.Windows.Media.Imaging;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Histogram")]
    public class GetHistogram : Cmdlet
    {
        [Parameter(HelpMessage = "The number of bins in the histogram.  Defaults to 256.  Must be a power of two between 2 and 256, inclusive.")]
        public int Bins { get; set;  }

        // projecting logos, such as Bushnell and Reconyx's, require pixel masks and aren't currently supported
        //            MP         rows to skip
        // Bushnell   5, 8, 14   100
        // Reconyx    3.1        32
        [Parameter(HelpMessage = "The number of rows of pixels to exclude from the histogram at the bottom of the image.  Default is zero.  This parameter allows a majority of the information bar generated by trail cameras to be excluded.")]
        public int BottomRowsToSkip { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Path to the .jpg file to calculate the histogram of.")]
        public string Image { get; set; }

        public GetHistogram()
        {
            this.BottomRowsToSkip = 0;
            this.Bins = 256;
        }

        protected override void ProcessRecord()
        {
            if (String.IsNullOrWhiteSpace(this.Image) || (File.Exists(this.Image) == false))
            {
                this.WriteError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(Image), "-Image must indicate an image file in JPEG format."), "image file", ErrorCategory.ReadError, this.Image));
                return;
            }

            WriteableBitmap writeableBitmap;
            using (FileStream imageStream = new FileStream(this.Image, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                JpegBitmapDecoder jpegBitmapDecoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                writeableBitmap = new WriteableBitmap(jpegBitmapDecoder.Frames[0]);
            }

            this.WriteObject(new ImageHistogram(writeableBitmap, this.Bins, this.BottomRowsToSkip));
        }
    }
}
