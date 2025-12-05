function Get-ClAISupportedModels {
    [CmdletBinding()]
    param()
    @(
        [ordered]@{ Name = 'phi3-mini'; Display = 'Phi-3 Mini'; Notes = 'Lightweight, good for constrained hardware'; Backend = 'LocalOllama' },
        [ordered]@{ Name = 'llama3.1'; Display = 'Llama 3.1 8B'; Notes = 'Better quality, needs more RAM/GPU'; Backend = 'LocalOllama' }
    )
}

function Get-ClAIModelRecommendation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [hashtable] $Hardware
    )

    $supported = Get-ClAISupportedModels
    $default = $supported | Where-Object { $_.Name -eq 'phi3-mini' }
    $reason = 'Defaulting to Phi-3 Mini for compatibility.'

    if ($Hardware.TotalMemoryGB -ge 16) {
        $discrete = $Hardware.GPU | Where-Object { $_.AdapterRAMGB -ge 6 }
        if ($discrete) {
            $default = $supported | Where-Object { $_.Name -eq 'llama3.1' }
            $reason = '>=16GB RAM and GPU >=6GB VRAM detected; recommending Llama 3.1 8B.'
        }
    }

    return [ordered]@{
        Recommendation = $default.Name
        Reason         = $reason
        Supported      = $supported
    }
}
