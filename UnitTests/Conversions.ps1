# cd ..\CritterShell\UnitTests\bin\Debug
Import-Module -Name ".\CritterShell.dll";

# critters
$images = ".\CarnivoreImages.csv";
$sign = ".\CarnivoreSign.gpx";

# csv flow
$signs = Convert-Gpx -DataType Critters -GpxFile $sign -OutputFile ".\PowerShell sign.csv";

$detectionsByStation = ".\PowerShell detections by station.csv";
$stationDetections = Get-Detections -ImageFile $images -OutputFile $detectionsByStation;
$detectionsBySite = ".\PowerShell detections by station.csv";
$siteDetections = Get-Detections -ImageFile $images -OutputFile $detectionsBySite -BySite -Window 7.00:00:00;

$groups = Add-Group -Name "DS02 group" -Stations @("DS02", "DS02G");
$dielActivity = Get-DielActivity -DetectionFile $detectionsByStation -Groups $groups -OutputFile ".\PowerShell diel activity.csv";
$monthlyActivity = Get-MonthlyActivity -DetectionFile $detectionsByStation -Groups $groups -OutputFile ".\PowerShell montly activity.csv";

# xlsx flow
$xlsx = ".\PowerShell.xlsx";
$signs = Convert-Gpx -DataType Critters -GpxFile $sign -OutputFile $xlsx -WorksheetName "critter sign";

$stationWorksheet = "station detections";
$stationDetections = Get-Detections -ImageFile $images -OutputFile $xlsx -OutputWorksheet $stationWorksheet;
$siteWorksheet = "site detections";
$siteDetections = Get-Detections -ImageFile $images -OutputFile $xlsx -OutputWorksheet $siteWorksheet -BySite -Window 7.00:00:00;

$dielActivity = Get-DielActivity -DetectionFile $xlsx -DetectionWorksheet $stationWorksheet -Groups $groups -OutputFile $xlsx;
$monthlyActivity = Get-MonthlyActivity -DetectionFile $xlsx -DetectionWorksheet $siteWorkSheet -Groups $groups -OutputFile $xlsx;

# default waypoint conversion
$gpx = ".\CAMA5.gpx";
$waypoints = Convert-Gpx -GpxFile $gpx -OutputFile ".\PowerShell CAMA5.csv";
$waypoints = Convert-Gpx -GpxFile $gpx -OutputFile ".\PowerShell CAMA5.xlsx";
