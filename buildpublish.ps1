$solution = "CommandTools.sln"
$projects = Get-ChildItem -Path "." -Filter "*.csproj" -Recurse


foreach ($proj in $projects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj.FullName)
    dotnet publish $proj.FullName -c Release -o "Publish\$projName" --sc
}