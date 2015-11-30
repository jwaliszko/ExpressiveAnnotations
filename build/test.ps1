$XUnitDir = get-childitem ..\src\ xunit.console.exe -recurse | select-object -first 1 | select -expand Directory
$XUnitCmd = "& '$($XUnitDir)\xunit.console.exe' ..\src\ExpressiveAnnotations.Tests\bin\Debug\ExpressiveAnnotations.Tests.dll ..\src\ExpressiveAnnotations.MvcUnobtrusive.Tests\bin\Debug\ExpressiveAnnotations.MvcUnobtrusive.Tests.dll ..\src\ExpressiveAnnotations.MvcWebSample.UITests\bin\Debug\ExpressiveAnnotations.MvcWebSample.UITests.dll -xml .\xunit-results.xml -appveyor"
Invoke-Expression $XUnitCmd

$ChutzpahDir = get-childitem ..\src\ chutzpah.console.exe -recurse | select-object -first 1 | select -expand Directory
$ChutzpahCmd = "& '$($ChutzpahDir)\chutzpah.console.exe' /path ..\src\expressive.annotations.validate.test.js /path ..\src\tests.html /junit .\chutzpah-results.xml"
Invoke-Expression $ChutzpahCmd

$results = [xml](get-content .\chutzpah-results.xml)

$anyFailures = $FALSE
foreach ($testsuite in $results.testsuites.testsuite) {
    foreach ($testcase in $testsuite.testcase) {
        if ($testcase.failure) {            
			if (Get-Command Add-AppveyorTest -errorAction SilentlyContinue) {
				Add-AppveyorTest $testcase.name -Outcome Failed -FileName $testsuite.name -ErrorMessage $testcase.failure.message -Duration $testcase.time
			}
            $anyFailures = $TRUE
        }
        else {
			if (Get-Command Add-AppveyorTest -errorAction SilentlyContinue) {
				Add-AppveyorTest $testcase.name -Outcome Passed -FileName $testsuite.name -Duration $testcase.time
			}
        }
    }
}

if ($anyFailures -eq $TRUE) {
    #$host.SetShouldExit(1)
}
