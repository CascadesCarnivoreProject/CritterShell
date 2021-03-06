﻿using CritterShell.Images;
using System.Management.Automation;
using System.Windows.Media.Imaging;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Histogram")]
    public class GetHistogram : ImageCmdlet
    {
        [Parameter(HelpMessage = "The number of bins in the histogram.  Defaults to 256.  Must be a power of two between 2 and 256, inclusive.")]
        public int Bins { get; set;  }

        // projecting logos, such as Bushnell and Reconyx's, require pixel masks and aren't currently supported
        //            MP         rows to skip
        // Bushnell   5, 8, 14   100
        // Reconyx    3.1        32
        [Parameter(HelpMessage = "The number of rows of pixels to exclude from the histogram at the bottom of the image.  Default is zero.  This parameter allows a majority of the information bar generated by trail cameras to be excluded.")]
        [ValidateRange(0, 1000000)]
        public int BottomRowsToSkip { get; set; }

        [Parameter(HelpMessage = "The ratio by which to scale the image before computing its histogram.  Can be 0.0 to 1.0, default is 1.0.  BottomRowsToSkip is also scaled.")]
        [ValidateRange(0.0, 1.0)]
        public double Scale { get; set; }

        public GetHistogram()
        {
            this.BottomRowsToSkip = 0;
            this.Bins = 256;
            this.Scale = 1.0;
        }

        protected override void ProcessRecord()
        {
            this.Initialize();

            WriteableBitmap image = this.ReadImage(this.Scale);
            image.Freeze();
            this.WriteObject(new ImageHistogram(image, this.Bins, (int)(this.Scale * this.BottomRowsToSkip)));
        }
    }
}
