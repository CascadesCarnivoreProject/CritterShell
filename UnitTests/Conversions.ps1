Import-Module -Name ".\CritterShell.dll"
Get-Detections -ImageFile ".\CarnivoreImages.csv"
Get-Detections -ImageFile ".\CarnivoreImages.csv" -BySite -Window 7.00:00:00
Convert-GpxToCsv -GpxFile ".\CarnivoreSign.gpx"