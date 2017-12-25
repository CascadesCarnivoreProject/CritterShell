# cd ..\CritterShell\UnitTests\bin\Debug
Import-Module -Name ".\CritterShell.dll";

# critters
$images = ".\CarnivoreImages.csv";
$sign = ".\CarnivoreSign.gpx";

# csv flow
$signs = Convert-Gpx -DataType Critter -GpxFile $sign -OutputFile ".\PowerShell sign.csv";

$detectionsByStation = ".\PowerShell detections by station.csv";
$stationDetections = Get-Detections -ImageFile $images -OutputFile $detectionsByStation;
$detectionsBySite = ".\PowerShell detections by site.csv";
$siteDetections = Get-Detections -ImageFile $images -OutputFile $detectionsBySite -BySite -Window 7.00:00:00;

$groups = Add-Group -Name "DS02 group" -Stations @("DS02", "DS02G");
$dielActivity = Get-DielActivity -DetectionFile $detectionsByStation -Groups $groups -OutputFile ".\PowerShell diel activity.csv";
$monthlyActivity = Get-MonthlyActivity -DetectionFile $detectionsByStation -Groups $groups -StationFile ".\StationData.csv" -OutputFile ".\PowerShell monthly activity.csv";

# xlsx flow
$xlsx = ".\PowerShell.xlsx";
$signs = Convert-Gpx -DataType Critter -GpxFile $sign -OutputFile $xlsx -OutputWorksheet "critter sign";

$stationWorksheet = "station detections";
$stationDetections = Get-Detections -ImageFile $images -OutputFile $xlsx -OutputWorksheet $stationWorksheet;
$siteWorksheet = "site detections";
$siteDetections = Get-Detections -ImageFile $images -OutputFile $xlsx -OutputWorksheet $siteWorksheet -BySite -Window 7.00:00:00;

$dielActivity = Get-DielActivity -DetectionFile $xlsx -DetectionWorksheet $stationWorksheet -Groups $groups -OutputFile $xlsx;
$monthlyActivity = Get-MonthlyActivity -DetectionFile $xlsx -DetectionWorksheet $stationWorksheet -Groups $groups -StationFile ".\StationData.csv" -OutputFile $xlsx;

# default waypoint conversion
$gpx = ".\CAMA5.gpx";
$waypoints = Convert-Gpx -GpxFile $gpx -OutputFile ".\PowerShell CAMA5.csv";
$waypoints = Convert-Gpx -GpxFile $gpx -OutputFile ".\PowerShell CAMA5.xlsx";

# exif
$image = ".\TADO-20170718-766.jpg";
$exif = Get-Exif -Image $image -Thumbnail ([System.IO.Path]::GetFileNameWithoutExtension($image) + '-thumbnail.jpg');
foreach ($directory in $exif)
{
	foreach ($tag in $directory.Tags)
	{
		Write-Host $tag.ToString();
	}
}

# extraction
$rectangle = Export-Rectangle -Image $image -Rectangle ([System.Windows.Int32Rect]::new(1458, 2373, 144, 41)) -Out ([System.IO.Path]::GetFileNameWithoutExtension($image) + '.png') -OutFormat ([System.Windows.Media.PixelFormats]::Gray8) -BinaryThreshold 30;
Write-Host $rectangle[0].Format
Write-Host $rectangle[1].Path

# histograms
# $image = ".\TADO-20170707-919.jpg";
# $image = ".\TADO-20170718-766-thumbnail.jpg"; # -BottomRowsToSkip 5
foreach ($bins in @(256, 128, 64, 32, 16, 8, 4, 2))
{
    $histogram = Get-Histogram -Image $image -Bins $bins -BottomRowsToSkip 100
    $histogram.WriteXlsx([System.IO.Path]::GetFileNameWithoutExtension($image) + ".xlsx", $bins);
}
