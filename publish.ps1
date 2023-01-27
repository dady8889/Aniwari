cd "Onova\Onova.Updater\"
dotnet publish -f "net7.0" -c Release -o "bin\Release\net7.0\publish\win-x64\" -r "win-x64" --no-self-contained -p:PublishSingleFile=true
cd "..\..\Aniwari.UI\"

dotnet publish -f "net7.0-windows10.0.19041.0" -c Release -p:RuntimeIdentifierOverride=win10-x64

$project = Get-Project Aniwari
$version = [Version]::new($project.Properties.Item("AssemblyVersion").Value)
$versionString = $version.ToString(3)

Onova.Publisher --in "bin\Release\net7.0-windows10.0.19041.0\win10-x64\publish" --out "..\OnovaPublish\" --no-releasenotes --version $versionString --name "Aniwari" --url "https://github.com/dady8889/Aniwari"
