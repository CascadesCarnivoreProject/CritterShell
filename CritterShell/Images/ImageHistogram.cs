﻿using CritterShell.Critters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.Images
{
    public class ImageHistogram : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        public int[] B { get; private set; }
        public int[] Cb { get; private set; }
        public int[] Coloration { get; private set; }
        public int[] Cr { get; private set; }
        public int Bins { get; private set; }
        public int BottomRowsToSkip { get; private set; }
        public int[] G { get; private set; }
        public int[] H { get; private set; }
        public int[] R { get; private set; }
        public int[] S { get; private set; }
        public int[] V { get; private set; }
        public int[] Y { get; private set; }

        static ImageHistogram()
        {
            ImageHistogram.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("bin", true),
                new ColumnDefinition("R", true),
                new ColumnDefinition("G", true),
                new ColumnDefinition("B", true),
                new ColumnDefinition("Y", true),
                new ColumnDefinition("Cb", true),
                new ColumnDefinition("Cr", true),
                new ColumnDefinition("H", true),
                new ColumnDefinition("S", true),
                new ColumnDefinition("V", true),
                new ColumnDefinition("coloration", true)
            }.AsReadOnly();
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:CodeMustNotContainMultipleWhitespaceInARow", Justification = "Readability.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Readability.")]
        public ImageHistogram(WriteableBitmap image, int bins, int bottomRowsToSkip)
        {
            if ((bins < 2) || (bins > 256) || ((bins & (bins - 1)) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bins), "Bins must be 2, 4, 8, 16, 32, 64, 128, or 256.");
            }
            if (bottomRowsToSkip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bottomRowsToSkip), "Number of rows to skip must be either zero or positive.");
            }
            if ((image.Format != PixelFormats.Bgr24) &&
                (image.Format != PixelFormats.Bgr32) &&
                (image.Format != PixelFormats.Bgra32) &&
                (image.Format != PixelFormats.Pbgra32))
            {
                throw new ArgumentOutOfRangeException(nameof(image), String.Format("Unhandled pixel format {0}.", image.Format));
            }

            this.Bins = bins;
            this.B = new int[bins];
            this.G = new int[bins];
            this.R = new int[bins];
            this.Y = new int[bins];
            this.Cb = new int[bins];
            this.Cr = new int[bins];
            this.H = new int[bins];
            this.S = new int[bins];
            this.V = new int[bins];
            this.Coloration = new int[bins];

            int bytesPerPixel = image.GetBytesPerPixel();
            int imageBytes = image.GetSizeInBytes(bottomRowsToSkip);
            Debug.Assert(image.BackBufferStride == bytesPerPixel * image.PixelWidth, "Unhandled back buffer stride.");
            int shiftToBinIndex = 8 - (int)Math.Log(bins, 2.0);
            Debug.Assert((0 <= shiftToBinIndex) && (shiftToBinIndex < 8), "Expected shift for 8 bit pixels to be between 0 and 7, inclusive.");

            unsafe
            {
                byte* backBuffer = (byte*)image.BackBuffer.ToPointer();
                for (int pixel = 0; pixel < imageBytes; pixel += bytesPerPixel)
                {
                    int r = *(backBuffer + pixel + 2);
                    int g = *(backBuffer + pixel + 1);
                    int b = *(backBuffer + pixel);

                    ++this.B[b >> shiftToBinIndex];
                    ++this.G[g >> shiftToBinIndex];
                    ++this.R[r >> shiftToBinIndex];

                    // https://en.wikipedia.org/wiki/YCbCr
                    // https://www.itu.int/dms_pubrec/itu-r/rec/bt/R-REC-BT.2020-2-201510-I!!PDF-E.pdf
                    // JFIF JPEG and SDTV BT.601 (NTSC) full swing: Y = 0.299R + 0.587G + 0.114B = (77R + 150G + 29B + 128) >> 8
                    // HDTV BT.709: Y = 0.2126R + 0.7152 + 0.0722B = (54R + 182G + 19B + 128) >> 8
                    // UHDTV BT.2020: Y = 0.2627R + 0.6780 + 0.0593B = (67R + 172G + 15B + 128) >> 8
                    int y = (77 * r + 150 * g + 29 * b + 128) >> 8;
                    // JFIF JPEG
                    //   Cb = 128 - 0.168736R - 0.331264G + 0.5B = (-43R + -84G + -128B + 256) >> 8
                    //   Cr = 128 + 0.5R - 0.418688G - 0.081312B = (127R - 107G - 21B + 256) >> 8
                    int cb = (32640 -  43 * r -  84 * g + 128 * b + 128) >> 8;
                    int cr = (32640 + 127 * r - 107 * g -  21 * b + 128) >> 8;

                    ++this.Y[y >> shiftToBinIndex];
                    ++this.Cb[cb >> shiftToBinIndex];
                    ++this.Cr[cr >> shiftToBinIndex];

                    int max = r > g ? r : g;
                        max = max > b ? max : b;
                    int min = r < g ? r : g;
                        min = min < b ? min : b;
                    int h = 0;
                    int s = 0;
                    int v = max;
                    if (v != 0)
                    {
                        int delta = max - min;
                        s = (int)(255.0f * (v > 127 ? (float)delta / (float)(512 - max - min) : (float)delta / (float)(max + min)));
                        if (s != 0)
                        {
                            if (max == r)
                            {
                                h = (byte)(43 * (g - b) / delta);
                            }
                            else if (max == g)
                            {
                                h = (byte)(85 + 43 * (b - r) / delta);
                            }
                            else
                            {
                                h = (byte)(171 + 43 * (r - g) / delta);
                            }
                        }
                    }

                    ++this.H[h >> shiftToBinIndex];
                    ++this.S[s >> shiftToBinIndex];
                    ++this.V[v >> shiftToBinIndex];

                    int coloration = (Math.Abs(r - g) + Math.Abs(r - g) + Math.Abs(b - r)) / 3;
                    ++this.Coloration[coloration >> shiftToBinIndex];
                }
            }
        }

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            throw new NotImplementedException();
        }

        public override void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (ColumnDefinition column in ImageHistogram.Columns)
                {
                    header.Append(this.AddCsvValue(column.Name));
                }
                fileWriter.WriteLine(header.ToString());

                int pixelValuesPerBin = 256 / this.Bins;
                for (int binIndex = 0; binIndex < this.Bins; ++binIndex)
                {
                    int r = this.R[binIndex] / pixelValuesPerBin;
                    int g = this.G[binIndex] / pixelValuesPerBin;
                    int b = this.B[binIndex] / pixelValuesPerBin;
                    int y = this.Y[binIndex] / pixelValuesPerBin;
                    int cb = this.Cb[binIndex] / pixelValuesPerBin;
                    int cr = this.Cr[binIndex] / pixelValuesPerBin;
                    int h = this.H[binIndex] / pixelValuesPerBin;
                    int s = this.S[binIndex] / pixelValuesPerBin;
                    int v = this.V[binIndex] / pixelValuesPerBin;
                    int coloration = this.Coloration[binIndex] / pixelValuesPerBin;

                    int startPixelValue = binIndex * pixelValuesPerBin;
                    int endPixelValue = startPixelValue + pixelValuesPerBin;
                    for (int pixelValue = startPixelValue; pixelValue < endPixelValue; ++pixelValue)
                    {
                        StringBuilder row = new StringBuilder();
                        row.Append(this.AddCsvValue(pixelValue));
                        row.Append(this.AddCsvValue(r));
                        row.Append(this.AddCsvValue(g));
                        row.Append(this.AddCsvValue(b));
                        row.Append(this.AddCsvValue(y));
                        row.Append(this.AddCsvValue(cb));
                        row.Append(this.AddCsvValue(cr));
                        row.Append(this.AddCsvValue(h));
                        row.Append(this.AddCsvValue(s));
                        row.Append(this.AddCsvValue(v));
                        row.Append(this.AddCsvValue(coloration));
                        fileWriter.WriteLine(row.ToString());
                    }
                }
            }
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = this.GetOrCreateBlankWorksheet(xlsxFile, worksheetName, ImageHistogram.Columns);

                int pixelValuesPerBin = 256 / this.Bins;
                for (int binIndex = 0; binIndex < this.Bins; ++binIndex)
                {
                    int r = this.R[binIndex] / pixelValuesPerBin;
                    int g = this.G[binIndex] / pixelValuesPerBin;
                    int b = this.B[binIndex] / pixelValuesPerBin;
                    int y = this.Y[binIndex] / pixelValuesPerBin;
                    int cb = this.Cb[binIndex] / pixelValuesPerBin;
                    int cr = this.Cr[binIndex] / pixelValuesPerBin;
                    int h = this.H[binIndex] / pixelValuesPerBin;
                    int s = this.S[binIndex] / pixelValuesPerBin;
                    int v = this.V[binIndex] / pixelValuesPerBin;
                    int coloration = this.Coloration[binIndex] / pixelValuesPerBin;

                    int startPixelValue = binIndex * pixelValuesPerBin;
                    int endPixelValue = startPixelValue + pixelValuesPerBin;
                    for (int pixelValue = startPixelValue; pixelValue < endPixelValue; ++pixelValue)
                    {
                        int row = pixelValue + 2;
                        worksheet.Cells[row, 1].Value = pixelValue;
                        worksheet.Cells[row, 2].Value = r;
                        worksheet.Cells[row, 3].Value = g;
                        worksheet.Cells[row, 4].Value = b;
                        worksheet.Cells[row, 5].Value = y;
                        worksheet.Cells[row, 6].Value = cb;
                        worksheet.Cells[row, 7].Value = cr;
                        worksheet.Cells[row, 8].Value = h;
                        worksheet.Cells[row, 9].Value = s;
                        worksheet.Cells[row, 10].Value = v;
                        worksheet.Cells[row, 11].Value = coloration;
                    }
                }

                worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns(Constant.Excel.MinimumColumnWidth, Constant.Excel.MaximumColumnWidth);
                xlsxFile.Save();
            }
        }
    }
}
