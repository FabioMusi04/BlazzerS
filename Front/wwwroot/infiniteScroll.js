window.initInfiniteScroll = (dotnetHelper, elementRef, dotnetMethodName, container) => {
    const scrollContainer = container
        ? document.querySelector(container)
        : null;

    const options = {
        threshold: 0.5,
        root: scrollContainer
    };

    if (!elementRef) return;

    const observer = new IntersectionObserver(entries => {
        if (entries[0].isIntersecting) {
            dotnetHelper.invokeMethodAsync(dotnetMethodName);
        }
    }, options);

    observer.observe(elementRef);
};
