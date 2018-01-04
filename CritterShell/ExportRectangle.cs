using CritterShell.Images;
using System;
using System.IO;
using System.Management.Automation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell
{
    [Cmdlet(VerbsData.Export, "Rectangle")]
    public class ExportRectangle : ImageCmdlet
    {
        [Parameter(HelpMessage = "If specified, pixels with luminosity greater than the threshold are converted to white and pixels equal or below threshold are converted to black.  Default is disabled.")]
        [ValidateRange(0, 255)]
        public int BinaryThreshold { get; set; }

        [Parameter(HelpMessage = "Type of rectangle to find within the image.")]
        public ExportTarget Find { get; set; }

        [Parameter(HelpMessage = "Path to the file to write the extracted rectangle to.  If not set, no output file is created.")]
        public string Out { get; set; }

        [Parameter(HelpMessage = "Format of the output file.  Default is the same format as used in the file indicated by -Image.")]
        public PixelFormat OutFormat { get; set; }

        [Parameter(HelpMessage = "Location and size of rectangle to export in pixels.")]
        public Int32Rect Rectangle { get; set; }

        public ExportRectangle()
        {
            this.BinaryThreshold = -1;
            this.Find = ExportTarget.None;
            this.Out = null;
            this.OutFormat = PixelFormats.Default;
            this.Rectangle = Int32Rect.Empty;
        }

        protected override void ProcessRecord()
        {
            this.Initialize();
            if ((this.Find != ExportTarget.None) && (this.Rectangle != Int32Rect.Empty))
            {
                throw new ArgumentException("Both -Find and -Rectangle are specified.  Specify one or the other.");
            }
            this.Out = this.CanonicalizePath(this.Out);

            WriteableBitmap image = this.ReadImage(1.0);
            image.Freeze();

            if (this.Find != ExportTarget.None)
            {
                switch (this.Find)
                {
                    case ExportTarget.BushnellTemperature:
                        this.Rectangle = Bushnell.FindTemperature(image);
                        break;
                    case ExportTarget.BushnellTrigger:
                        this.Rectangle = Bushnell.FindTrigger(image);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Unhandled export target type {0}.", this.Find));
                }
            }

            WriteableBitmap area = image.ExtractRectangle(this.Rectangle);
            if (this.BinaryThreshold >= 0)
            {
                area.Threshold(this.BinaryThreshold);
            }
            area.Freeze();

            WriteableBitmap output = area;
            if (this.OutFormat != PixelFormats.Default)
            {
                output = area.Convert(this.OutFormat);
                output.Freeze();
            }

            ImageProperties properties = output.GetProperties();
            if (String.IsNullOrWhiteSpace(this.Out) == false)
            {
                output.Save(this.Out);
                properties.Path = this.Out;
            }

            this.WriteObject(output);
            this.WriteObject(properties);
        }
    }
}
