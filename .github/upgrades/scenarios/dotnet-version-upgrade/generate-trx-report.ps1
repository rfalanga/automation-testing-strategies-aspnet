# PowerShell script: generate-trx-report.ps1
# Finds TRX files under the repo tests/*/TestResults and writes a combined HTML report.

param(
    [string]$Root = (Resolve-Path -Path "$PSScriptRoot/..\..\.." | Select-Object -ExpandProperty Path)
)

Set-Location -Path $Root

$trxFiles = Get-ChildItem -Path "tests" -Recurse -Filter "*.trx" | Where-Object { $_.FullName -notmatch "TestResults-filtered" }
if (-not $trxFiles) {
    Write-Error "No TRX files found under tests/*/TestResults"
    exit 2
}

$reportPath = Join-Path -Path "$PSScriptRoot" -ChildPath "test-report.html"

$allResults = @()

# default namespace for TRX XML
$ns = @{t='http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

foreach ($f in $trxFiles) {
    try {
        $xml = [xml](Get-Content $f.FullName -Raw)
    } catch {
        Write-Warning "Failed to read $($f.FullName): $_"
        continue
    }

    # Select UnitTestResult nodes (handle namespace)
    $nodes = $xml.SelectNodes('//t:UnitTestResult', $ns)
    foreach ($n in $nodes) {
        $entry = [PSCustomObject]@{
            File = $f.FullName
            TestName = $n.testName
            Outcome = $n.outcome
            Duration = $n.duration
            StartTime = $n.startTime
            EndTime = $n.endTime
        }
        $allResults += $entry
    }
}

$total = $allResults.Count
$passed = ($allResults | Where-Object { $_.Outcome -eq 'Passed' }).Count
$failed = ($allResults | Where-Object { $_.Outcome -ne 'Passed' }).Count

# Build HTML
$htmlBuilder = New-Object System.Text.StringBuilder
$htmlBuilder.AppendLine('<!doctype html>') | Out-Null
$htmlBuilder.AppendLine('<html lang="en">') | Out-Null
$htmlBuilder.AppendLine('<head>') | Out-Null
$htmlBuilder.AppendLine('<meta charset="utf-8"/>') | Out-Null
$htmlBuilder.AppendLine('<meta name="viewport" content="width=device-width,initial-scale=1"/>') | Out-Null
$htmlBuilder.AppendLine('<title>TRX Test Report</title>') | Out-Null
$htmlBuilder.AppendLine('<style>body{font-family:Segoe UI,Arial,sans-serif;margin:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:8px}th{background:#f3f3f3;text-align:left}tr.pass{background:#e6ffed}tr.fail{background:#ffecec} .meta{margin-bottom:16px}</style>') | Out-Null
$htmlBuilder.AppendLine('</head><body>') | Out-Null
$htmlBuilder.AppendLine("<h1>TRX Test Report</h1>") | Out-Null
$htmlBuilder.AppendLine("<div class='meta'><strong>Generated:</strong> $(Get-Date -Format u)<br/><strong>TRX files:</strong> $($trxFiles.Count) &nbsp; <strong>Total tests:</strong> $total &nbsp; <strong>Passed:</strong> $passed &nbsp; <strong>Other:</strong> $failed</div>") | Out-Null

# Group by file and show per-file summary
foreach ($group in $allResults | Group-Object File) {
    $file = $group.Name
    $count = $group.Count
    $p = ($group.Group | Where-Object { $_.Outcome -eq 'Passed' }).Count
    $other = $count - $p
    $htmlBuilder.AppendLine("<h2>File: $(Split-Path $file -Leaf) <small>($file)</small></h2>") | Out-Null
    $htmlBuilder.AppendLine("<div><strong>Tests:</strong> $count &nbsp; <strong>Passed:</strong> $p &nbsp; <strong>Other:</strong> $other</div>") | Out-Null
    $htmlBuilder.AppendLine('<table>') | Out-Null
    $htmlBuilder.AppendLine('<thead><tr><th>Test Name</th><th>Outcome</th><th>Duration</th><th>Start</th><th>End</th></tr></thead><tbody>') | Out-Null
    foreach ($r in ($group.Group | Sort-Object TestName)) {
        $rowClass = if ($r.Outcome -eq 'Passed') { 'pass' } else { 'fail' }
        $encodedName = [System.Web.HttpUtility]::HtmlEncode($r.TestName)
        $htmlBuilder.AppendLine("<tr class='$rowClass'><td>$encodedName</td><td>$($r.Outcome)</td><td>$($r.Duration)</td><td>$($r.StartTime)</td><td>$($r.EndTime)</td></tr>") | Out-Null
    }
    $htmlBuilder.AppendLine('</tbody></table><br/>') | Out-Null
}

$htmlBuilder.AppendLine('</body></html>') | Out-Null

# Write report
[System.IO.File]::WriteAllText($reportPath, $htmlBuilder.ToString())
Write-Output "Wrote HTML report to: $reportPath"

exit 0
