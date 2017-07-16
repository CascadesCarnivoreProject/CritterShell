using System;

namespace CritterShell
{
    internal static class Constant
    {
        public static readonly TimeSpan DefaultDetectionMergeWindow = TimeSpan.FromMinutes(5);

        public static class ActivityColumn
        {
            public const string Identification = "Identification";
            public const string N = "N";
            public const string Station = "Station";
            public const string Survey = "Survey";
        }

        public static class CritterSignColumn
        {
            public const string Identification = "Identification";
            public const string SecondIdentification = "SecondIdentification";
            public const string Type = "Type";
        }

        public static class Csv
        {
            public const string Extension = ".csv";
        }

        public static class DetectionColumn
        {
            public const string Station = "Station";
            public const string File = "File";
            public const string RelativePath = "RelativePath";
            public const string StartDateTime = "StartDateTime";
            public const string EndDateTime = "EndDateTime";
            public const string UtcOffset = "UtcOffset";
            public const string Duration = "Duration";
            public const string TriggerSource = "TriggerSource";
            public const string Identification = "Identification";
            public const string Confidence = "Confidence";
            public const string GroupType = "GroupType";
            public const string Age = "Age";
            public const string Pelage = "Pelage";
            public const string Activity = "Activity";
            public const string Comments = "Comments";
            public const string Survey = "Survey";
        }

        public static class Excel
        {
            public const string DefaultDetectionsWorksheetName = "detections";
            public const string Extension = ".xlsx";
            public const double MinimumColumnWidth = 5.0;
            public const double MaximumColumnWidth = 40.0;
        }

        public static class File
        {
            public const string JpgExtension = ".jpg";
            public const int MaximumImportWarnings = 1;
        }

        public static class GarminExtension
        {
            public const string Category = "Category";
            public const string CreationTime = "CreationTime";
            public const string DisplayMode = "DisplayMode";
        }

        public static class GarminNamespace
        {
            public const string Acceleration1 = "http://www.garmin.com/xmlschemas/AccelerationExtension/v1";
            public const string Adventure1 = "http://www.garmin.com/xmlschemas/AdventuresExtensions/v1";
            public const string CreationTime1 = "http://www.garmin.com/xmlschemas/CreationTimeExtension/v1";
            public const string Gpx3 = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";
            public const string Power1 = "http://www.garmin.com/xmlschemas/PowerExtension/v1";
            public const string Pressure1 = "http://www.garmin.com/xmlschemas/PressureExtension/v1";
            public const string TrackPoint1 = "http://www.garmin.com/xmlschemas/TrackPointExtension/v1";
            public const string Trip1 = "http://www.garmin.com/xmlschemas/TripExtensions/v1";
            public const string TripMetadata1 = "http://www.garmin.com/xmlschemas/TripMetaDataExtensions/v1";
            public const string ViaPointTransportationMode1 = "http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensions/v1";
            public const string Video1 = "http://www.garmin.com/xmlschemas/VideoExtension/v1";
            public const string Waypoint1 = "http://www.garmin.com/xmlschemas/WaypointExtension/v1";
        }

        public static class Gpx
        {
            public const string Bounds = "bounds";
            public const string Comment = "cmt";
            public const string Description = "desc";
            public const string Creator = "creator";
            public const string Elevation = "ele";
            public const string Extensions = "extensions";
            public const string GpxElement = "gpx";
            public const string Href = "href";
            public const string Latitude = "lat";
            public const string Link = "link";
            public const string Longitude = "lon";
            public const string MaximumLatitude = "maxlat";
            public const string MaximumLongitude = "maxlon";
            public const string Metadata = "metadata";
            public const string MinimumLatitude = "minlat";
            public const string MinimumLongitude = "minlon";
            public const string Name = "name";
            public const string Namespace = "http://www.topografix.com/GPX/1/1";
            public const string Symbol = "sym";
            public const string Text = "text";
            public const string Time = "time";
            public const string Type = "type";
            public const string Version = "version";
            public const string Waypoint = "wpt";
        }

        public static class GpxColumn
        {
            public const string Comment = "Comment";
            public const string Description = "Description";
            public const string Elevation = "Elevation";
            public const string Latitude = "Latitude";
            public const string Longitude = "Longitude";
            public const string Name = "Name";
            public const string Time = "Time";
        }

        public static class ImageColumn
        {
            public const string Activity = "Activity";
            public const string Age = "Age";
            public const string Comments = "Comments";
            public const string Confidence = "Confidence";
            public const string DateTime = "DateTime";
            public const string DeleteFlag = "DeleteFlag";
            public const string File = "File";
            public const string GroupType = "GroupType";
            public const string Identification = "Identification";
            public const string ImageQuality = "ImageQuality";
            public const string Pelage = "Pelage";
            public const string RelativePath = "RelativePath";
            public const string Station = "Station";
            public const string Survey = "Survey";
            public const string TriggerSource = "TriggerSource";
            public const string UtcOffset = "UtcOffset";
        }

        public static class WaypointSignType
        {
            public const char ForageSite = 'F';
            public const char SubniviumAccess = 'N';
            public const char Photo = 'P';
            public const char Scat = 'S';
            public const char Track = 'T';
            public const char Urine = 'U';
        }

        public static class Time
        {
            public const string HourOfDayFormatWithoutSign = "hh\\:mm";
            public const int HoursInDay = 24;
            public const int MonthsInYear = 12;
            public const string MonthShortFormat = "MMM";
            public const string UtcDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            public const string UtcOffsetFormat = "0.00";
        }
    }
}
