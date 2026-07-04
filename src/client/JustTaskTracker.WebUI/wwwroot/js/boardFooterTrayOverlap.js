export function attach(scrollBody, footerPanel, dotNetRef) {
    if (!scrollBody || !footerPanel) {
        return { dispose: () => {}, refresh: () => {} };
    }

    const notify = () => {
        const ids = getOverlappingColumnIds(scrollBody, footerPanel);
        void dotNetRef.invokeMethodAsync('SetTrayOverlappingColumnIds', ids);
    };

    const onScroll = () => notify();

    scrollBody.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('resize', onScroll, { passive: true });

    const resizeObserver = new ResizeObserver(onScroll);
    resizeObserver.observe(footerPanel);
    resizeObserver.observe(scrollBody);

    let animationFrameId = null;

    const stopAnimationTracking = () => {
        if (animationFrameId !== null) {
            cancelAnimationFrame(animationFrameId);
            animationFrameId = null;
        }
    };

    const trackDuringAnimation = (durationMs = 300) => {
        stopAnimationTracking();
        const start = performance.now();
        const tick = (now) => {
            notify();
            if (now - start < durationMs) {
                animationFrameId = requestAnimationFrame(tick);
            } else {
                animationFrameId = null;
            }
        };
        animationFrameId = requestAnimationFrame(tick);
    };

    notify();
    trackDuringAnimation();

    return {
        dispose: () => {
            scrollBody.removeEventListener('scroll', onScroll);
            window.removeEventListener('resize', onScroll);
            resizeObserver.disconnect();
            stopAnimationTracking();
        },
        refresh: () => {
            notify();
            trackDuringAnimation();
        },
    };
}

export function getOverlappingColumnIds(scrollBody, footerPanel) {
    const tray = footerPanel.getBoundingClientRect();
    if (tray.width <= 0 || tray.height <= 0) {
        return [];
    }

    const columns = scrollBody.querySelectorAll('.board-column[data-column-id]');
    const ids = [];

    for (const column of columns) {
        const rect = column.getBoundingClientRect();
        const overlapsHorizontally = rect.left < tray.right && rect.right > tray.left;
        if (!overlapsHorizontally) {
            continue;
        }

        const id = column.getAttribute('data-column-id');
        if (id) {
            ids.push(id);
        }
    }

    return ids;
}
