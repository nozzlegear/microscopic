$dir = "src/tests";
$config = "Release";

# Run the tests
$testOutput = dotnet test -c $config "$dir/tests.csproj";
$testExitCode = $LastExitCode;
$totalTestsLine = $testOutput -match "^Total tests:";

write-host $totalTestsLine;

if ($testExitCode -ne 0 -or $output -contains "Test Run Failed.") {
    Write-Host "Tests failed:" -ForegroundColor "red";
    Write-Output $testOutput;
    $message = "Tests failed with exit code $testExitCode.";

    throw $message;
}
else {
    Write-Host "All tests passed." -ForegroundColor "Green";
}