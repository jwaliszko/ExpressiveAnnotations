$RootDir = "..\"
if($env:APPVEYOR -eq $True) {
    $RootDir = $env:APPVEYOR_BUILD_FOLDER
}

$XUnitDir = get-childitem $RootDir xunit.console.exe -recurse | select-object -first 1 | select -expand Directory
$XUnitCmd = "$XUnitDir\xunit.console.exe $RootDir\src\ExpressiveAnnotations.Tests\bin\Debug\ExpressiveAnnotations.Tests.dll $RootDir\src\ExpressiveAnnotations.MvcUnobtrusive.Tests\bin\Debug\ExpressiveAnnotations.MvcUnobtrusive.Tests.dll $RootDir\src\ExpressiveAnnotations.MvcWebSample.UITests\bin\Debug\ExpressiveAnnotations.MvcWebSample.UITests.dll -xml .\xunit-results.xml -appveyor"
Invoke-Expression $XUnitCmd

$ChutzpahDir = get-childitem $RootDir chutzpah.console.exe -recurse | select-object -first 1 | select -expand Directory
$ChutzpahCmd = "$ChutzpahDir\chutzpah.console.exe /path $RootDir\src\expressive.annotations.validate.test.js /path $RootDir\src\tests.html /junit .\chutzpah-results.xml"
Invoke-Expression $ChutzpahCmd

if($env:APPVEYOR -eq $True) {
    $success = $True
    $results = [xml](get-content .\chutzpah-results.xml)

    foreach ($testsuite in $results.testsuites.testsuite) {
        foreach ($testcase in $testsuite.testcase) {
            if ($testcase.failure) {
                Add-AppveyorTest $testcase.name -Outcome Failed -FileName $testsuite.name -ErrorMessage $testcase.failure.message -Duration $testcase.time
                $success = $False
            }
            else {
                Add-AppveyorTest $testcase.name -Outcome Passed -FileName $testsuite.name -Duration $testcase.time
            }
        }
    }

    if ($success -eq $False) {
        $host.SetShouldExit(1)
    }
}
