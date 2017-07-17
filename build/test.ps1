# usages:
#    ./test.ps1
#    ./test.ps1 -buildcfg debug -nojs

param (
    [ValidateSet("Release", "Debug")]
    [string]$buildcfg = "Release",
    [switch]$nocs = $false,
    [switch]$nojs = $false
 )

$rootdir  = (Get-Item -Path ".." -Verbose).FullName

# redefine above variables for appveyor
if ($env:APPVEYOR -eq $true) {
    $rootdir  = $env:APPVEYOR_BUILD_FOLDER
    $buildcfg = $env:CONFIGURATION
}

Write-Host "Root directory: $rootdir" -foregroundcolor "yellow"
Write-Host "Configuration: $buildcfg" -foregroundcolor "yellow"

# minify the script file, which is required in release configuration of UI testing
& uglifyjs --compress --mangle --comments /Copyright/ --output $rootdir\src\expressive.annotations.validate.min.js -- $rootdir\src\expressive.annotations.validate.js
cp $rootdir\src\expressive.annotations.validate.min.js $rootdir\src\ExpressiveAnnotations.MvcWebSample\Scripts\expressive.annotations.validate.min.js

# collect tools
$xunitdir     = Get-ChildItem $rootdir xunit.console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory
$opencoverdir = Get-ChildItem $rootdir opencover.console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory
$chutzpahdir  = Get-ChildItem $rootdir chutzpah.console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory

$xunit     = "$xunitdir\xunit.console.exe"
$opencover = "$opencoverdir\opencover.console.exe"
$chutzpah  = "$chutzpahdir\chutzpah.console.exe"

# collect C# tests
$eatestdll = "$rootdir\src\ExpressiveAnnotations.Tests\bin\$buildcfg\ExpressiveAnnotations.Tests.dll"
$vatestdll = "$rootdir\src\ExpressiveAnnotations.MvcUnobtrusive.Tests\bin\$buildcfg\ExpressiveAnnotations.MvcUnobtrusive.Tests.dll"
$uitestdll = "$rootdir\src\ExpressiveAnnotations.MvcWebSample.UITests\bin\$buildcfg\ExpressiveAnnotations.MvcWebSample.UITests.dll"
$testdlls  = "`"`"$eatestdll`"`" `"`"$vatestdll`"`" `"`"$uitestdll`"`""
$webmvcbin = "$rootdir\src\ExpressiveAnnotations.MvcWebSample\bin"

# collect JS tests
$maintest    = "$rootdir\src\expressive.annotations.validate.test.js"
$formtest    = "$rootdir\src\form.tests.harness.html"
$formtestnew = "$rootdir\src\form.tests.harness.latestdeps.html"

# run tests and analyze code coverage

if ($nocs -eq $false) {
    Write-Host "`nC# tests started..." -foregroundcolor "yellow"
    & $opencover -register:user "-target:$xunit" "-targetargs:$testdlls -nologo -noshadow -appveyor" "-targetdir:$webmvcbin" "-filter:+[ExpressiveAnnotations(.MvcUnobtrusive)?]*" -output:csharp-coverage.xml -hideskipped:All -mergebyhash -returntargetcode

    if ($LastExitCode -ne 0) {
        if ($env:APPVEYOR -eq $true) {
            $host.SetShouldExit($LastExitCode)
        }
        throw "C# tests failed"
    }
}

if ($nojs -eq $false) {
    Write-Host "`nJS tests started..." -foregroundcolor "yellow"
    & $chutzpah /nologo /silent /path $formtest /path $formtestnew /path $maintest /junit chutzpah-tests.xml /coverage /coverageIgnores "*test*, *jquery*" /coveragehtml javascript-coverage.html /lcov javascript-coverage.lcov

    if ($LastExitCode -ne 0) {
        if ($env:APPVEYOR -eq $true) {
            $host.SetShouldExit($LastExitCode)
        }
        throw "JS tests failed"
    }

    # manually submit chutzpah test results to appveyor
    if ($env:APPVEYOR -eq $true) {
        $results = [xml](Get-Content chutzpah-tests.xml)

        foreach ($testsuite in $results.testsuites.testsuite) {
            foreach ($testcase in $testsuite.testcase) {
                $module,$test = $testcase.name.split(':')

                if ($testcase.failure) {
                    Add-AppveyorTest $test -Outcome Failed -FileName $testsuite.name -ErrorMessage $testcase.failure.message -Duration $testcase.time
                }
                else {
                    Add-AppveyorTest $test -Outcome Passed -FileName $testsuite.name -Duration $testcase.time
                }
            }
        }
    }
}

# reportgenerator.exe -reports:csharp-coverage.xml -targetdir:csharp-coverage
# start csharp-coverage\index.htm
# start javascript-coverage.htm

# vsinstr.exe WebProj\bin\AnalyzeMe.dll /coverage #notice: x64 vs x86 tool version
# vsperfcmd.exe /start:coverage /output:results.coverage
# xunit.console.exe WebProj.UITests\bin\UITests.dll
# vsperfcmd.exe /shutdown
