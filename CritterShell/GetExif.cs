using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using MetadataDirectory = MetadataExtractor.Directory;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Exif")]
    public class GetExif : ImageCmdlet
    {
        private const int MaxMetadataExtractorIssue35Offset = 12;

        [Parameter(HelpMessage = "Optional path to extract an embedded thumbnail to, if one is present.")]
        public string Thumbnail { get; set; }

        protected override void ProcessRecord()
        {
            this.Initialize();
            using (FileStream imageStream = new FileStream(this.Image, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IReadOnlyList<MetadataDirectory> metadataDirectories = ImageMetadataReader.ReadMetadata(this.Image);
                this.WriteObject(metadataDirectories);

                if (this.Thumbnail != null)
                {
                    ExifThumbnailDirectory thumbnailDirectory = metadataDirectories.OfType<ExifThumbnailDirectory>().FirstOrDefault();
                    if (thumbnailDirectory == null)
                    {
                        throw new ImageProcessingException(String.Format("{0} does not contain an embedded thumbnail.", this.Image));
                    }
                    string compression = thumbnailDirectory.GetDescription(ExifThumbnailDirectory.TagCompression);
                    if (compression.StartsWith("JPEG") == false)
                    {
                        throw new NotSupportedException(String.Format("Unhandled thumbnail compression method '{0}'.", compression));
                    }

                    long thumbnailOffset = thumbnailDirectory.GetInt64(ExifThumbnailDirectory.TagThumbnailOffset);
                    int thumbnailLength = thumbnailDirectory.GetInt32(ExifThumbnailDirectory.TagThumbnailLength) + GetExif.MaxMetadataExtractorIssue35Offset;
                    if ((thumbnailOffset + thumbnailLength) > imageStream.Length)
                    {
                        throw new ImageProcessingException(String.Format("End position of thumbnail (byte {0}) is beyond the end of '{1}'.", thumbnailOffset + thumbnailLength, this.Image));
                    }
                    byte[] thumbnail = new byte[thumbnailLength];
                    imageStream.Seek(thumbnailOffset, SeekOrigin.Begin);
                    imageStream.Read(thumbnail, 0, thumbnailLength);

                    // work around Metadata Extractor issue #35
                    // https://github.com/drewnoakes/metadata-extractor-dotnet/issues/35
                    if (thumbnailLength <= MaxMetadataExtractorIssue35Offset + 1)
                    {
                        throw new NotSupportedException(String.Format("Unhandled thumbnail length {0}.", thumbnailLength));
                    }
                    int issue35Offset = 0;
                    for (int offset = 0; offset <= MaxMetadataExtractorIssue35Offset; ++offset)
                    {
                        // 0xffd8 is the JFIF start of image segment indicator
                        // https://en.wikipedia.org/wiki/JPEG_File_Interchange_Format#File_format_structure
                        if ((thumbnail[offset] == 0xff) && (thumbnail[offset + 1] == 0xd8))
                        {
                            issue35Offset = offset;
                            break;
                        }
                    }

                    this.Thumbnail = this.CanonicalizePath(this.Thumbnail);
                    using (FileStream thumbnailStream = new FileStream(this.Thumbnail, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        thumbnailStream.Write(thumbnail, issue35Offset, thumbnailLength - issue35Offset);
                    }
                }
            }
        }
    }
}
