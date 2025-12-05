function Get-ClAILlmBackendConfig {
    $config = Read-ClAIConfig
    return [ordered]@{
        Backend      = $config.Backend
        Endpoint     = $config.Endpoint
        DefaultModel = $config.DefaultModel
    }
}

function Invoke-ClAILlm {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [array] $Messages,
        [string] $Model,
        [double] $Temperature = 0.2
    )

    $backend = Get-ClAILlmBackendConfig
    $modelToUse = if ($Model) { $Model } else { $backend.DefaultModel }

    switch ($backend.Backend) {
        'LocalOllama' {
            $uri = "$($backend.Endpoint.TrimEnd('/'))/api/chat"
            $body = @{ model = $modelToUse; messages = $Messages; options = @{ temperature = $Temperature } }
            try {
                $response = Invoke-RestMethod -Method Post -Uri $uri -Body ($body | ConvertTo-Json -Depth 6) -ContentType 'application/json' -ErrorAction Stop
                # Expecting { message = { content = '...' } } or similar
                if ($response.message.content) { return $response.message.content }
                if ($response.content) { return $response.content }
                return ($response | ConvertTo-Json -Depth 4)
            }
            catch {
                throw "LLM request failed: $($_.Exception.Message)"
            }
        }
        default {
            throw "Backend '$($backend.Backend)' is not implemented."
        }
    }
}
