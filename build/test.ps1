$rootdir = (Get-Item -Path "..\" -Verbose).FullName
$buildcfg = "Release"

# redefine above variables for appveyor
if($env:APPVEYOR -eq $true) {
    $rootdir = $env:APPVEYOR_BUILD_FOLDER
    $buildcfg = $env:CONFIGURATION
}

# collect tools
$xunitdir = Get-ChildItem $rootdir xunit.console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory
$opencoverdir = Get-ChildItem $rootdir OpenCover.Console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory
$chutzpahdir = Get-ChildItem $rootdir chutzpah.console.exe -Recurse | Select-Object -First 1 | Select -Expand Directory

$xunit = "$xunitdir\xunit.console.exe"
$opencover = "$opencoverdir\OpenCover.Console.exe"
$chutzpah = "$chutzpahdir\chutzpah.console.exe"

# collect C# tests
$eatestdll = "$rootdir\src\ExpressiveAnnotations.Tests\bin\$buildcfg\ExpressiveAnnotations.Tests.dll"
$vatestdll = "$rootdir\src\ExpressiveAnnotations.MvcUnobtrusive.Tests\bin\$buildcfg\ExpressiveAnnotations.MvcUnobtrusive.Tests.dll"
$uitestdll = "$rootdir\src\ExpressiveAnnotations.MvcWebSample.UITests\bin\$buildcfg\ExpressiveAnnotations.MvcWebSample.UITests.dll"
$webmvcbin = "$rootdir\src\ExpressiveAnnotations.MvcWebSample\bin"

# collect JS tests
$maintest = "$rootdir\src\expressive.annotations.validate.test.js"
$formtest = "$rootdir\src\tests.html"

# run tests and analyze code coverage
$opencovercmd = "$opencover -register:user -hideskipped:All -mergebyhash '-target:$xunit' '-targetargs:$eatestdll $vatestdll $uitestdll -noshadow -appveyor' '-targetdir:$webmvcbin' '-filter:+[ExpressiveAnnotations(.MvcUnobtrusive)?]*' '-output:.\csharp-coverage.xml' -returntargetcode"
$chutzpahcmd = "$chutzpah /path $maintest /path $formtest /coverage /coverageIgnores '*test*, *jquery*' /junit .\chutzpah-results.xml /lcov .\chutzpah-results.lcov"

Invoke-Expression $opencovercmd
if($LastExitCode -ne 0) {
    if($env:APPVEYOR -eq $true) {
        $host.SetShouldExit($LastExitCode)
    }
    throw "C# tests failed"
}

Invoke-Expression $chutzpahcmd
if($LastExitCode -ne 0) {
    if($env:APPVEYOR -eq $true) {
        $host.SetShouldExit($LastExitCode)
    }
    throw "JS tests failed"
}

# manually submit chutzpah test results to appveyor
if($env:APPVEYOR -eq $true) {
    $results = [xml](Get-Content .\chutzpah-results.xml)

    foreach ($testsuite in $results.testsuites.testsuite) {
        foreach ($testcase in $testsuite.testcase) {
            if ($testcase.failure) {
                Add-AppveyorTest $testcase.name -Outcome Failed -FileName $testsuite.name -ErrorMessage $testcase.failure.message -Duration $testcase.time
            }
            else {
                Add-AppveyorTest $testcase.name -Outcome Passed -FileName $testsuite.name -Duration $testcase.time
            }
        }
    }
}

# Invoke-Expression "reportgenerator.exe -reports:.\csharp-coverage.xml -targetdir:.\csharp-report"

# vsinstr.exe WebProj\bin\AnalyzeMe.dll /coverage #notice: x64 vs x86 tool version
# vsperfcmd.exe /start:coverage /output:results.coverage
# xunit.console.exe WebProj.UITests\bin\UITests.dll
# vsperfcmd.exe /shutdown
