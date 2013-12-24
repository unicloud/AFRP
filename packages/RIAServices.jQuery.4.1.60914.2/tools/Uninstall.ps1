param($installPath, $toolsPath, $package, $project) 

$rootElement = [Microsoft.Build.Construction.ProjectRootElement]::Open($project.FileName)

$targetsImport = $rootElement.Imports | Where-Object {$_.Project -eq "Microsoft.Ria.Validation.targets"}
while ($targetsImport.Parent.Count -eq 1) {$targetsImport = $targetsImport.Parent}
$targetsImport.Parent.RemoveChild($targetsImport)
