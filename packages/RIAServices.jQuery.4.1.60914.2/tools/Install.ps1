param($installPath, $toolsPath, $package, $project)

$rootElement = [Microsoft.Build.Construction.ProjectRootElement]::Open($project.FileName)

$rootElement.AddImport("Microsoft.Ria.Validation.targets")