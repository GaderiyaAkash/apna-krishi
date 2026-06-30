// ── Apna Krishi – Site JS ──

// Auto-dismiss alerts after 4 seconds
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert.alert-success, .alert.alert-info').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 4000);
    });

    // Active nav link highlighting
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        const href = link.getAttribute('href')?.toLowerCase();
        if (href && href !== '/' && currentPath.startsWith(href)) {
            link.classList.add('active', 'fw-semibold');
        }
    });
});

// Confirm delete helper (used as onclick)
function confirmDelete(msg) {
    return confirm(msg || 'Are you sure you want to delete this?');
}

// Scroll to top
window.addEventListener('scroll', function () {
    const btn = document.getElementById('scrollTopBtn');
    if (btn) btn.style.display = window.scrollY > 300 ? 'block' : 'none';
});
