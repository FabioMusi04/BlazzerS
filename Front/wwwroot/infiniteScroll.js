window.initInfiniteScroll = (dotnetHelper, sessionRef/*, postRef*/) => {
    console.log(dotnetHelper);
    console.log(sessionRef)
    const options = { threshold: 1.0 };

    if (sessionRef) {
        const observer1 = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting) {
                dotnetHelper.invokeMethodAsync('OnSessionObserverIntersect');
            }
        }, options);
        observer1.observe(sessionRef);
    }

    //if (postRef) {
    //    const observer2 = new IntersectionObserver(entries => {
    //        if (entries[0].isIntersecting) {
    //            dotnetHelper.invokeMethodAsync('OnPostObserverIntersect');
    //        }
    //    }, options);
    //    observer2.observe(postRef);
    //}
};
