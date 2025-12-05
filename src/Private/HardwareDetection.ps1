function Get-ClAIHardwareInfo {
    [CmdletBinding()]
    param()
    $hardware = [ordered]@{
        OSVersion        = [System.Environment]::OSVersion.VersionString
        PowerShell       = $PSVersionTable.PSVersion.ToString()
        TotalMemoryGB    = $null
        LogicalCores     = $null
        GPU              = @()
        NPU              = 'Unknown'
    }

    try {
        $mem = Get-CimInstance -ClassName Win32_ComputerSystem -ErrorAction Stop
        if ($mem.TotalPhysicalMemory) {
            $hardware.TotalMemoryGB = [math]::Round($mem.TotalPhysicalMemory / 1GB, 2)
        }
    }
    catch {
        Write-Warning "Unable to read memory info: $($_.Exception.Message)"
    }

    try {
        $cpu = Get-CimInstance -ClassName Win32_Processor -ErrorAction Stop | Select-Object -First 1
        if ($cpu.NumberOfLogicalProcessors) {
            $hardware.LogicalCores = $cpu.NumberOfLogicalProcessors
        }
    }
    catch {
        Write-Warning "Unable to read CPU info: $($_.Exception.Message)"
    }

    try {
        $gpus = Get-CimInstance -ClassName Win32_VideoController -ErrorAction Stop
        foreach ($gpu in $gpus) {
            $hardware.GPU += [ordered]@{
                Name = $gpu.Name
                AdapterRAMGB = if ($gpu.AdapterRAM) { [math]::Round($gpu.AdapterRAM / 1GB, 2) } else { $null }
                DriverVersion = $gpu.DriverVersion
            }
        }
    }
    catch {
        Write-Warning "Unable to read GPU info: $($_.Exception.Message)"
    }

    # NPU detection placeholder: future APIs like Windows ML/DirectML could be queried here
    $hardware
}
