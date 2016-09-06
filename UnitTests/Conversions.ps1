Import-Module -Name ".\CritterShell.dll";

Convert-GpxToCsv -GpxFile ".\CarnivoreSign.gpx" -OutputFile ".\CarnivoreSign-FromPowerShell.csv";

$detectionsByStation = ".\CarnivoreImages-DetectionsByStation-FromPowerShell.csv";
Get-Detections -ImageFile ".\CarnivoreImages.csv" -OutputFile $detectionsByStation;
$detectionsBySite = ".\CarnivoreImages-DetectionsBySite-FromPowerShell.csv";
Get-Detections -ImageFile ".\CarnivoreImages.csv" -OutputFile $detectionsBySite -BySite -Window 7.00:00:00;

Get-DielActivity -DetectionFile $detectionsByStation -OutputFile ".\CarnivoreImages-DielActivityByStation-FromPowerShell.csv";
Get-MonthlyActivity -DetectionFile $detectionsByStation -OutputFile ".\CarnivoreImages-MonthlyActivityByStation-FromPowerShell.csv";