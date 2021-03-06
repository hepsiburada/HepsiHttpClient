Properties {
    ### Directories
    $base_directory = resolve-path .
	$src_directory = "$base_directory\src"
	$output_directory = "$base_directory\build"
	$dist_directory = "$base_directory\distribution"
    $tests_dir = "$base_directory\src\tests"
    $package_dir = "$base_directory\packages"

    ### Tools
    $nuget = "$base_directory\.nuget\nuget.exe"
    $nunit = "$package_dir\nunit.runners*\tools\nunit-console.exe"

    ### AppVeyor-related
    $appVeyorConfig = "$base_directory\appveyor.yml"
    $appVeyor = $env:APPVEYOR

    ### Project information
	$solution = "Hepsi.Http.Client.sln"
    $sln_file = "$base_directory\$solution"
    $target_config = "Release"

	### Build information
	$buildNumber = 0;
	$version = "1.0.1.0"
	$preRelease = $null
}

## Tasks

Task default -Depends Clean, RunUnitTests, RunApiTests, CreateNuGetPackage
Task appVeyor -Depends Clean, CreateNuGetPackage

Task Restore {
    "Restoring NuGet packages for '$solution'..."

    Exec { .$nuget restore $solution }
}

Task Clean {
	rmdir $output_directory -ea SilentlyContinue -recurse
	rmdir $dist_directory -ea SilentlyContinue -recurse
	exec { msbuild /nologo /verbosity:quiet $sln_file /p:Configuration=$target_config /t:Clean /p:Outdir="$output_directory" }
}

task UpdateVersion {
	"Updating version number..."

	$vSplit = $version.Split('.')
	if($vSplit.Length -ne 4)
	{
		throw "Version number is invalid. Must be in the form of 0.0.0.0"
	}
	$major = $vSplit[0]
	$minor = $vSplit[1]
	$patch = $vSplit[2]
	$assemblyFileVersion =  "$major.$minor.$patch.$buildNumber"
	$assemblyVersion = "$major.$minor.$patch.$buildNumber"
	$versionAssemblyInfoFile = "$src_directory/Shared/VersionAssemblyInfo.cs"
	"using System.Reflection;" > $versionAssemblyInfoFile
	"" >> $versionAssemblyInfoFile
	"[assembly: AssemblyVersion(""$assemblyVersion"")]" >> $versionAssemblyInfoFile
	"[assembly: AssemblyFileVersion(""$assemblyFileVersion"")]" >> $versionAssemblyInfoFile
}

Task Compile -Depends UpdateVersion {
    "Compiling '$solution'..."

	exec { msbuild /nologo /verbosity:q $sln_file /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.5 /p:Outdir="$output_directory" }
}

Task RunUnitTests -Depends Compile {
	"Runing Unit Tests for '$solution'..."

	$project = "Hepsi.Http.Client.UnitTests"
	.$nunit "$output_directory\$project.dll" /noxml
	if (!$?) {
	    exit 1
	}
}

Task RunApiTests -Depends Compile {
	"Runing Unit Tests for '$solution'..."

	$project = "Hepsi.Http.Client.ApiTests"
	.$nunit "$output_directory\$project.dll" /noxml
	if (!$?) {
	    exit 1
	}
}

Task CreateNuGetPackage -Depends Compile {
	"Creating nuget package..."

	$vSplit = $version.Split('.')
	if($vSplit.Length -ne 4)
	{
		throw "Version number is invalid. Must be in the form of 0.0.0.0"
	}
	$major = $vSplit[0]
	$minor = $vSplit[1]
	$patch = $vSplit[2]
	$packageVersion =  "$major.$minor.$patch"
	if($preRelease){
		$packageVersion = "$packageVersion-$preRelease"
	}

	if ($buildNumber -ne 0){
		$packageVersion = $packageVersion + "-build" + $buildNumber.ToString().PadLeft(5,'0')
	}

	mkdir $dist_directory -ea SilentlyContinue
	copy-item $src_directory\Hepsi.Http.Client\Hepsi.Http.Client.nuspec $dist_directory
	copy-item $src_directory\Hepsi.Http.Client.Testing\Hepsi.Http.Client.Testing.nuspec $dist_directory
	exec { . $nuget pack $dist_directory\Hepsi.Http.Client.nuspec -BasePath $dist_directory -o $dist_directory -version $packageVersion }
	exec { . $nuget pack $dist_directory\Hepsi.Http.Client.Testing.nuspec -BasePath $dist_directory -o $dist_directory -version $packageVersion }
}
