using System;

namespace CritterShell
{
    internal static class Constant
    {
        public const string DateTimeUtcFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly TimeSpan DefaultDetectionMergeWindow = TimeSpan.FromMinutes(5);

        public static class CritterSignColumn
        {
            public const string Elevation = "Elevation";
            public const string Identification = "Identification";
            public const string Latitude = "Latitude";
            public const string Longitude = "Longitude";
            public const string Name = "Name";
            public const string SecondIdentification = "SecondIdentification";
            public const string Time = "Time";
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
            public const string StartTime = "StartTime";
            public const string EndTime = "EndTime";
            public const string Duration = "Duration";
            public const string TriggerSource = "TriggerSource";
            public const string Identification = "Identification";
            public const string Confidence = "Confidence";
            public const string GroupType = "GroupType";
            public const string Age = "Age";
            public const string Pelage = "Pelage";
            public const string Activity = "Activity";
            public const string Comments = "Comments";
            public const string Folder = "Folder";
            public const string Survey = "Survey";
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

        public static class ImageColumn
        {
            public const string Activity = "Activity";
            public const string Age = "Age";
            public const string Comments = "Comments";
            public const string Confidence = "Confidence";
            public const string Date = "Date";
            public const string File = "File";
            public const string Folder = "Folder";
            public const string GroupType = "GroupType";
            public const string Identification = "Identification";
            public const string ImageQuality = "ImageQuality";
            public const string MarkForDeletion = "MarkForDeletion";
            public const string Pelage = "Pelage";
            public const string RelativePath = "RelativePath";
            public const string Station = "Station";
            public const string Survey = "Survey";
            public const string Time = "Time";
            public const string TriggerSource = "TriggerSource";
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
            // time formats                                                    Timelapse               .csv saved by Excel
            public static readonly string[] LocalTimeFormats = new string[2] { "dd-MMM-yyyy HH:mm:ss", "dd-MMM-yy H:mm:ss" };
        }
    }
}
