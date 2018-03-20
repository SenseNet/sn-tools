$srcPath = [System.IO.Path]::GetFullPath(($PSScriptRoot + '\..\..\src'))

# delete existing packages
Remove-Item $PSScriptRoot\*.nupkg

nuget pack $srcPath\SenseNet.Tools\SenseNet.Tools.csproj -properties Configuration=Release -OutputDirectory $PSScriptRoot