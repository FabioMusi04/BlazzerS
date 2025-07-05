using Microsoft.JSInterop;

namespace Front.Services;

public class DeviceService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task<string> GetDeviceIdAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("getOrCreateDeviceId");
    }
}
