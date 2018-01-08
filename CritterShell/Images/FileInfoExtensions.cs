using System;
using System.IO;

namespace CritterShell.Images
{
    internal static class FileInfoExtensions
    {
        public static bool IsImage(this FileInfo fileInfo)
        {
            return String.Equals(fileInfo.Extension, Constant.File.JpegExtension, StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(fileInfo.Extension, Constant.File.JpgExtension, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVideo(this FileInfo fileInfo)
        {
            return String.Equals(fileInfo.Extension, Constant.File.AviExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
