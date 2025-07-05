using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Front.Services;

public class CookieHandler(DeviceService deviceService) : DelegatingHandler
{
    private readonly DeviceService _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string deviceId = await _deviceService.GetDeviceIdAsync();
        request.Headers.Add("X-Device-ID", deviceId);

        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await base.SendAsync(request, cancellationToken);
    }
}