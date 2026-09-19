/**
 * TaskManager - Interactive UI Enhancements
 * Handles Dynamic Dark/Light Theming, Live Task Filtering, and View Switching
 */

(function () {
    'use strict';

    // =========================================================================
    // 1. Theme Management (Dark / Light Mode)
    // =========================================================================
    const THEME_STORAGE_KEY = 'taskmanager_theme_preference';

    function getStoredTheme() {
        return localStorage.getItem(THEME_STORAGE_KEY);
    }

    function getPreferredTheme() {
        const storedTheme = getStoredTheme();
        if (storedTheme) {
            return storedTheme;
        }
        // Default to dark mode for a modern developer-centric aesthetic
        return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        updateThemeToggleIcon(theme);
    }

    function updateThemeToggleIcon(theme) {
        const toggleButtons = document.querySelectorAll('.btn-theme-toggle');
        toggleButtons.forEach(btn => {
            const icon = btn.querySelector('i');
            if (icon) {
                if (theme === 'dark') {
                    icon.className = 'bi bi-sun-fill text-warning';
                    btn.setAttribute('title', 'Switch to Light Mode');
                } else {
                    icon.className = 'bi bi-moon-stars-fill text-primary';
                    btn.setAttribute('title', 'Switch to Dark Mode');
                }
            }
        });
    }

    function toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'dark';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        localStorage.setItem(THEME_STORAGE_KEY, newTheme);
        applyTheme(newTheme);
    }

    // Apply theme as early as possible
    applyTheme(getPreferredTheme());

    document.addEventListener('DOMContentLoaded', () => {
        // Initialize theme toggle buttons
        const toggleButtons = document.querySelectorAll('.btn-theme-toggle');
        toggleButtons.forEach(btn => {
            btn.addEventListener('click', toggleTheme);
        });
        updateThemeToggleIcon(document.documentElement.getAttribute('data-bs-theme') || 'dark');

        // =====================================================================
        // 2. Real-Time Task Search & Filter
        // =====================================================================
        const searchInput = document.getElementById('taskSearchInput');
        const taskCards = document.querySelectorAll('.task-item-card');
        const taskRows = document.querySelectorAll('.task-item-row');
        const noTasksFoundEl = document.getElementById('noSearchMatchAlert');
        const taskCountBadge = document.getElementById('visibleTaskCount');

        if (searchInput) {
            searchInput.addEventListener('input', function (e) {
                const query = e.target.value.toLowerCase().trim();
                let matchCount = 0;

                // Filter grid cards
                taskCards.forEach(card => {
                    const title = card.getAttribute('data-task-title') || '';
                    const desc = card.getAttribute('data-task-desc') || '';
                    const matches = title.includes(query) || desc.includes(query);

                    if (matches) {
                        card.style.display = '';
                        matchCount++;
                    } else {
                        card.style.display = 'none';
                    }
                });

                // Filter table rows
                taskRows.forEach(row => {
                    const title = row.getAttribute('data-task-title') || '';
                    const desc = row.getAttribute('data-task-desc') || '';
                    const matches = title.includes(query) || desc.includes(query);

                    if (matches) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                });

                // Update count badge if present
                if (taskCountBadge) {
                    taskCountBadge.textContent = matchCount;
                }

                // Show empty match state if needed
                if (noTasksFoundEl) {
                    if (matchCount === 0 && (taskCards.length > 0 || taskRows.length > 0)) {
                        noTasksFoundEl.classList.remove('d-none');
                    } else {
                        noTasksFoundEl.classList.add('d-none');
                    }
                }
            });
        }

        // =====================================================================
        // 3. View Switcher (Grid vs Table)
        // =====================================================================
        const VIEW_PREF_KEY = 'taskmanager_view_mode';
        const btnGridView = document.getElementById('btnViewGrid');
        const btnTableView = document.getElementById('btnViewTable');
        const containerGrid = document.getElementById('taskGridContainer');
        const containerTable = document.getElementById('taskTableContainer');

        function setViewMode(mode) {
            if (!containerGrid || !containerTable) return;

            if (mode === 'table') {
                containerGrid.classList.add('d-none');
                containerTable.classList.remove('d-none');
                if (btnTableView) btnTableView.classList.add('active');
                if (btnGridView) btnGridView.classList.remove('active');
            } else {
                containerGrid.classList.remove('d-none');
                containerTable.classList.add('d-none');
                if (btnGridView) btnGridView.classList.add('active');
                if (btnTableView) btnTableView.classList.remove('active');
            }
            localStorage.setItem(VIEW_PREF_KEY, mode);
        }

        if (btnGridView && btnTableView) {
            btnGridView.addEventListener('click', () => setViewMode('grid'));
            btnTableView.addEventListener('click', () => setViewMode('table'));

            // Load saved view mode (default: grid)
            const savedView = localStorage.getItem(VIEW_PREF_KEY) || 'grid';
            setViewMode(savedView);
        }

        // =====================================================================
        // 4. Input Character Counters (for Create/Edit forms)
        // =====================================================================
        const descriptionInput = document.getElementById('taskDescriptionInput');
        const charCounter = document.getElementById('descCharCounter');

        if (descriptionInput && charCounter) {
            const updateCounter = () => {
                const len = descriptionInput.value.length;
                charCounter.textContent = `${len} / 500 characters`;
            };
            descriptionInput.addEventListener('input', updateCounter);
            updateCounter();
        }
    });
})();
