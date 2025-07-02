const applicationServerKey = "BFQRVbu5IEbEnEGvD9vlj2ZUNzjsqYA6gvXHoG-Q0LHSH6Z0U_XJfxvotAHZ1Yr_SfRUYMAaPbl5JXa2bppv6wA" //capiamo

window.registerPushNotifications = async (userEmail) => {
    if (!("serviceWorker" in navigator)) {
        console.warn("Service workers are not supported.");
        return;
    }

    try {
        const reg = await navigator.serviceWorker.register("/ServiceWorker.js", { scope: "/" });

        const permission = await Notification.requestPermission();
        if (permission !== "granted") {
            console.warn("Notification permission denied.");
            return;
        }

        const subscription = await reg.pushManager.getSubscription() ??
            await reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(applicationServerKey)
            });

        const endpoint = subscription.endpoint;
        const p256dh = arrayBufferToBase64(subscription.getKey('p256dh'));
        const auth = arrayBufferToBase64(subscription.getKey('auth'));

        await DotNet.invokeMethodAsync("ReportingPortal", "AutoSubscribe", userEmail, endpoint, p256dh, auth);
        console.log("Push subscription sent successfully.");
    } catch (err) {
        console.error("Push notification setup failed:", err);
    }
}

function arrayBufferToBase64(buffer) {
    return btoa(String.fromCharCode(...new Uint8Array(buffer)));
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    return Uint8Array.from([...rawData].map(char => char.charCodeAt(0)));
}
