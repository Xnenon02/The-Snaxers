// ===================================================
// POST-LOGIN ACTION PERSISTENCE
// Saves intended favorite/cart action before login redirect,
// then replays it automatically after returning to the gallery.
// ===================================================

document.addEventListener('DOMContentLoaded', function () {

    const isAuthenticated = document.getElementById('isAuthenticated')?.value === 'true';
    const antiForgeryToken = document.getElementById('antiForgeryToken')?.value;

    // Replay pending action after login redirect
    if (isAuthenticated && antiForgeryToken) {
        const pendingActionJson = sessionStorage.getItem('pendingAction');

        if (pendingActionJson) {
            const action = JSON.parse(pendingActionJson);
            sessionStorage.removeItem('pendingAction');

            fetch(action.url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: `productId=${encodeURIComponent(action.productId)}&returnUrl=Chocolate`
            }).then(() => {
                window.location.reload();
            }).catch(err => {
                console.error('Failed to replay pending action:', err);
            });
        }
    }

    // Intercept favorite form submissions for unauthenticated users
    if (!isAuthenticated) {
        document.querySelectorAll('.favorite-zone form').forEach(form => {
            form.addEventListener('submit', function (e) {
                e.preventDefault();

                const productId = form.querySelector('input[name="productId"]')?.value;
                const url = form.getAttribute('action');

                if (productId && url) {
                    // Save intended action before login redirect
                    sessionStorage.setItem('pendingAction', JSON.stringify({
                        productId: productId,
                        url: url
                    }));
                }

                // Submit form naturally to trigger login redirect
                form.submit();
            });
        });
    }
});