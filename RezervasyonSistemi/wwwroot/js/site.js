// Immediate Theme Init (Prevent Flash of Unstyled Content)
(function () {
    var savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
        document.documentElement.setAttribute('data-theme', 'dark');
    } else {
        document.documentElement.setAttribute('data-theme', 'light');
    }
})();

document.addEventListener('DOMContentLoaded', function () {
    // Theme Toggle Functionality
    var themeToggleBtn = document.getElementById('theme-toggle-btn');
    var themeIcon = document.getElementById('theme-icon');

    function updateThemeIcon(theme) {
        if (themeIcon) {
            if (theme === 'dark') {
                themeIcon.className = 'bi bi-sun-fill text-warning';
            } else {
                themeIcon.className = 'bi bi-moon-stars-fill text-dark';
            }
        }
    }

    var currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    updateThemeIcon(currentTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            var activeTheme = document.documentElement.getAttribute('data-theme');
            var newTheme = activeTheme === 'dark' ? 'light' : 'dark';

            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeIcon(newTheme);
        });
    }

    // Sidebar Toggle Functionality
    var sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    var appWrapper = document.getElementById('app-wrapper');

    if (sidebarToggleBtn && appWrapper) {
        sidebarToggleBtn.addEventListener('click', function (e) {
            e.preventDefault();
            appWrapper.classList.toggle('toggled');
        });
    }
});
