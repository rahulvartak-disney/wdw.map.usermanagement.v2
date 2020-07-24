try
{
	Write-Host "*** Starting Deployment ***" -foregroundcolor green -BackgroundColor Yellow
	
	$solution = "Wdw.UserManagement.v2.wsp"
	
	$slnFlag = Get-SPSolution -Identity $solution
	if ($slnFlag -ne $null) {
	Write-Host "`nDisabling User Management feature…"
	
	$ftr = Disable-SPFeature –Identity "b919c298-0d7b-443f-aa00-6f1cedf40e24" –url http://spdev.mbia.disney.com/sites/Workplace/ –Confirm:$false
	Start-Sleep -Seconds 10
	
	Write-Host "`nUser Management feature Disabled Sucessfully"
	
	$slnStat = Uninstall-SPSolution -Identity $solution -WebApplication http://spdev.mbia.disney.com/ –Confirm:$false
	if ($slnStat.Deployed -eq $null) {
	Write-Host "`nUnInstalling $solution solution…"
	$flgWSPSln = $false
	do {
	$wspID = Get-SPSolution -Identity $solution
	$tmJob = Get-SPSolution -Identity $solution
	if ($wspID.Deployed -eq $null) {
	$flgWSPSln = $false
	}
	else {
	if (!($wspID.Deployed -eq $False -And $tmJob.JobExists -eq – $False)) {
	$flgWSPSln = $true
	Start-Sleep -Seconds 10
	}
	else {
	$flgWSPSln = $false
	}
	}
	}
	while ($flgWSPSln -eq $true)
	Write-Host "`n$solution solution Uninstalled Sucessfully…"
	
	Remove-SPSolution -Identity $solution –Confirm:$false
	Start-Sleep -Seconds 2
	Write-Host "`n$solution solution Removed Sucessfully…"
	}
	}
	
	$slnFlag = Get-SPSolution -Identity $solution
	if ($slnFlag -eq $null) {
	Add-SPSolution "C:\Users\rvartak\Source\Repos\rahulvartak-disney\wdw.map.usermanagement.v2\Deployment\Wdw.UserManagement.v2.wsp"
	Start-Sleep -Seconds 2
	Write-Host "`n$solution Added Sucessfully…"
	Install-SPSolution -Identity $solution -WebApplication http://spdev.mbia.disney.com/ -GACDeployment
	Write-Host "`nInstalling $solution …"
	$flgWSPSln = $false
	do {
	$wspID = Get-SPSolution -Identity $solution
	$tmJob = Get-SPSolution -Identity $solution
	if ($wspID.Deployed -eq $True -And $tmJob.JobExists -eq – $False) {
	$flgWSPSln = $true
	}
	else {
	Start-Sleep -Seconds 10
	}
	}
	while ($flgWSPSln -eq $false)
	Write-Host "`nInstalled $solution solution Sucessfully…"
	Write-Host "`nEnabling the User Management feature …."
	Enable-SPFeature –Identity "b919c298-0d7b-443f-aa00-6f1cedf40e24" –url http://spdev.mbia.disney.com/sites/Workplace/ –PassThru
	Write-Host "`nUser Management feature Enabled Sucessfully"
	}
	
	Write-Host "*** Deployment successfully completely ***" -foregroundcolor green -BackgroundColor Yellow
}
catch
{
	Write-Host "`nError:: $($_.Exception.Message)" -foregroundcolor red -BackgroundColor Yellow
}




