// Admin/Staff page enhancements, loaded from _Layout for those roles only:
//  1. Every data table gets a search box (all columns or one chosen column),
//     a "showing X of Y" count, and click-to-sort column headers.
//  2. Every <select> becomes a searchable dropdown (Tom Select).
//
// Opt out per element with data-no-enhance (tables) or data-no-search
// (selects). Rows are hidden with a class rather than the `hidden`
// attribute so pages that run their own row filtering (e.g. Users) keep
// working alongside this one.
(function () {
    'use strict';

    // ---------- Tables ----------

    function cellText(row, index) {
        var cell = row.cells[index];
        return cell ? cell.textContent.replace(/\s+/g, ' ').trim() : '';
    }

    // Turns a cell's text into something comparable: numbers (incl. GPA,
    // currency, counts), then dates, then plain text.
    function sortKey(text) {
        if (text === '') return { type: 2, value: '' };
        var numeric = text.replace(/[$,%\s]/g, '');
        if (/^-?\d+(\.\d+)?$/.test(numeric)) return { type: 0, value: parseFloat(numeric) };
        if (/\d/.test(text) && /[\/\-]|\b(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)/i.test(text)) {
            var time = Date.parse(text);
            if (!isNaN(time)) return { type: 1, value: time };
        }
        return { type: 2, value: text.toLowerCase() };
    }

    function compare(a, b) {
        if (a.type !== b.type) return a.type - b.type;
        if (a.type === 2) return a.value.localeCompare(b.value, undefined, { numeric: true });
        return a.value - b.value;
    }

    function isActionsHeader(th) {
        var label = th.textContent.trim().toLowerCase();
        return label === '' || label === 'actions' || label === 'action' || th.hasAttribute('data-no-sort');
    }

    function enhanceTable(table) {
        if (table.hasAttribute('data-no-enhance') || table.dataset.enhanced) return;
        var thead = table.tHead;
        var tbody = table.tBodies[0];
        if (!thead || !tbody || table.tBodies.length > 1) return;
        var headerRow = thead.rows[thead.rows.length - 1];
        var rows = Array.prototype.slice.call(tbody.rows);

        // Grouped/report tables (group header or subtotal rows spanning
        // columns) would be scrambled by sorting - leave them alone. A lone
        // "No records found" row means there's nothing to work with either.
        var hasSpanningRows = rows.some(function (r) {
            return Array.prototype.some.call(r.cells, function (c) { return c.colSpan > 1 || c.rowSpan > 1; });
        });
        if (hasSpanningRows || rows.length < 2) return;

        table.dataset.enhanced = 'true';

        // --- Toolbar: search + column picker + count ---
        var toolbar = document.createElement('div');
        toolbar.className = 'ae-toolbar d-flex flex-wrap align-items-center gap-2 mb-2';
        toolbar.innerHTML =
            '<div class="input-group input-group-sm ae-search">' +
            '  <span class="input-group-text"><i class="bi bi-search"></i></span>' +
            '  <input type="search" class="form-control" placeholder="Search this table..." aria-label="Search this table">' +
            '</div>' +
            '<select class="form-select form-select-sm ae-column" aria-label="Search in column" data-no-search>' +
            '  <option value="-1">All columns</option>' +
            '</select>' +
            '<small class="text-muted ae-count ms-auto"></small>';

        var input = toolbar.querySelector('input');
        var columnSelect = toolbar.querySelector('select');
        var count = toolbar.querySelector('.ae-count');

        Array.prototype.forEach.call(headerRow.cells, function (th, i) {
            if (isActionsHeader(th)) return;
            var opt = document.createElement('option');
            opt.value = String(i);
            opt.textContent = th.textContent.trim();
            columnSelect.appendChild(opt);
        });

        // Put the toolbar above the table's scroll wrapper, if it has one.
        var anchor = table.parentElement && table.parentElement.classList.contains('table-responsive')
            ? table.parentElement : table;
        anchor.parentNode.insertBefore(toolbar, anchor);

        function applyFilter() {
            var term = input.value.trim().toLowerCase();
            var col = parseInt(columnSelect.value, 10);
            var shown = 0;
            rows.forEach(function (row) {
                var text = col >= 0 ? cellText(row, col) : row.textContent.replace(/\s+/g, ' ');
                var match = term === '' || text.toLowerCase().indexOf(term) !== -1;
                row.classList.toggle('ae-filtered', !match);
                if (match) shown++;
            });
            count.textContent = term === ''
                ? rows.length + ' record' + (rows.length === 1 ? '' : 's')
                : 'Showing ' + shown + ' of ' + rows.length;
        }

        input.addEventListener('input', applyFilter);
        columnSelect.addEventListener('change', applyFilter);
        applyFilter();

        // --- Click-to-sort headers ---
        Array.prototype.forEach.call(headerRow.cells, function (th, index) {
            if (isActionsHeader(th)) return;
            th.classList.add('ae-sortable');
            th.setAttribute('role', 'button');
            th.setAttribute('tabindex', '0');
            th.title = 'Click to sort';
            var icon = document.createElement('i');
            icon.className = 'bi bi-arrow-down-up ae-sort-icon';
            th.appendChild(icon);

            function sortBy() {
                var dir = th.dataset.sortDir === 'asc' ? 'desc' : 'asc';
                Array.prototype.forEach.call(headerRow.cells, function (other) {
                    delete other.dataset.sortDir;
                    var otherIcon = other.querySelector('.ae-sort-icon');
                    if (otherIcon) otherIcon.className = 'bi bi-arrow-down-up ae-sort-icon';
                });
                th.dataset.sortDir = dir;
                icon.className = 'bi ae-sort-icon ae-sort-active ' + (dir === 'asc' ? 'bi-sort-up' : 'bi-sort-down');

                var keyed = rows.map(function (row, i) { return { row: row, key: sortKey(cellText(row, index)), i: i }; });
                keyed.sort(function (a, b) {
                    var result = compare(a.key, b.key);
                    if (result === 0) return a.i - b.i;
                    return dir === 'asc' ? result : -result;
                });
                keyed.forEach(function (k) { tbody.appendChild(k.row); });
                rows = keyed.map(function (k) { return k.row; });
            }

            th.addEventListener('click', function (e) {
                if (e.target.closest('a, button, input, select')) return;
                sortBy();
            });
            th.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); sortBy(); }
            });
        });
    }

    // ---------- Dropdowns ----------

    function enhanceSelect(select) {
        if (select.tomselect || select.hasAttribute('data-no-search') || select.size > 1) return;
        if (typeof TomSelect === 'undefined') return;

        var plugins = select.multiple ? ['remove_button'] : [];
        new TomSelect(select, {
            plugins: plugins,
            allowEmptyOption: true,
            create: false,
            maxOptions: null,
            // Keep the page's own "-- Select --" option as the placeholder.
            placeholder: select.options.length && select.options[0].value === '' ? select.options[0].text : 'Search...',
            // Opening inside a modal / scroll container shouldn't get clipped.
            dropdownParent: select.closest('.modal') ? null : 'body',
            onDropdownOpen: function () { this.control_input.focus(); }
        });
    }

    function enhanceAll(root) {
        (root || document).querySelectorAll('table.table').forEach(enhanceTable);
        (root || document).querySelectorAll('select').forEach(enhanceSelect);
    }

    // Pages that set a select's value from script when a modal opens (e.g.
    // the Users access-scope modal) change the hidden original <select>;
    // pull those values back into the searchable control once it's shown.
    document.addEventListener('shown.bs.modal', function (e) {
        e.target.querySelectorAll('select').forEach(function (s) {
            if (s.tomselect) s.tomselect.sync();
        });
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { enhanceAll(); });
    } else {
        enhanceAll();
    }

    window.adminEnhance = enhanceAll;
})();
