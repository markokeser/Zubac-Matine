// Zubac — shared client-side behaviour.
// Plain browser JS (no build step). Page-specific setup lives in the views' Scripts sections
// and calls into the small `Zubac` namespace defined here.
(function (window, document) {
    'use strict';

    const Zubac = window.Zubac = window.Zubac || {};

    const icon = (name) =>
        `<svg class="icon" aria-hidden="true"><use href="#i-${name}"></use></svg>`;

    Zubac.icon = icon;

    Zubac.money = function (value) {
        return (Math.round(value * 100) / 100).toFixed(2) + ' €';
    };

    /* ---------- Toasts ---------------------------------------------------- */

    const toastIcons = { success: 'check', error: 'alert', info: 'info', warning: 'alert' };

    Zubac.toast = function (message, type) {
        type = type || 'success';
        const root = document.getElementById('toasts');
        if (!root || !message) return;

        const el = document.createElement('div');
        el.className = `ztoast ztoast--${type}`;
        el.setAttribute('role', type === 'error' ? 'alert' : 'status');
        el.innerHTML = `<span class="ztoast__icon">${icon(toastIcons[type] || 'info')}</span><span class="ztoast__text"></span>`;
        el.querySelector('.ztoast__text').textContent = message;
        root.appendChild(el);

        while (root.children.length > 3) root.firstElementChild.remove();

        setTimeout(() => {
            el.classList.add('is-leaving');
            el.addEventListener('animationend', () => el.remove(), { once: true });
        }, type === 'error' ? 4200 : 2800);
    };

    /* ---------- Confirm dialog ------------------------------------------- */

    // Usage: <form data-confirm="Delete this?" data-confirm-title="…" data-confirm-ok="Delete" data-confirm-tone="danger">
    //        <a href="…" data-confirm="…">
    Zubac.confirm = function (options) {
        return new Promise((resolve) => {
            const modalEl = document.getElementById('confirmModal');
            if (!modalEl || !window.bootstrap) {
                resolve(window.confirm(options.message));
                return;
            }

            modalEl.querySelector('[data-confirm-title]').textContent = options.title || 'Are you sure?';
            modalEl.querySelector('[data-confirm-message]').textContent = options.message || '';
            const ok = modalEl.querySelector('[data-confirm-ok]');
            ok.textContent = options.ok || 'Confirm';
            ok.className = 'button ' + (options.tone === 'danger' ? 'button--danger' : 'button--primary');
            const chip = modalEl.querySelector('.chip-icon');
            chip.setAttribute('data-tone', options.tone === 'danger' ? 'danger' : '');
            chip.innerHTML = icon(options.icon || (options.tone === 'danger' ? 'trash' : 'check-circle'));

            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            let confirmed = false;

            const onOk = () => { confirmed = true; modal.hide(); };
            const onHidden = () => {
                ok.removeEventListener('click', onOk);
                modalEl.removeEventListener('hidden.bs.modal', onHidden);
                resolve(confirmed);
            };

            ok.addEventListener('click', onOk);
            modalEl.addEventListener('hidden.bs.modal', onHidden);
            modal.show();
        });
    };

    function confirmOptions(el) {
        return {
            message: el.getAttribute('data-confirm'),
            title: el.getAttribute('data-confirm-title'),
            ok: el.getAttribute('data-confirm-ok'),
            tone: el.getAttribute('data-confirm-tone'),
            icon: el.getAttribute('data-confirm-icon')
        };
    }

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement) || !form.hasAttribute('data-confirm')) return;
        if (form.dataset.confirmed === 'true') return;

        e.preventDefault();
        const submitter = e.submitter;
        Zubac.confirm(confirmOptions(form)).then((yes) => {
            if (!yes) return;
            form.dataset.confirmed = 'true';
            setLoading(submitter || form.querySelector('[type="submit"]'));
            form.submit();
        });
    }, true);

    document.addEventListener('click', function (e) {
        const link = e.target.closest('a[data-confirm]');
        if (!link) return;
        e.preventDefault();
        Zubac.confirm(confirmOptions(link)).then((yes) => {
            if (yes) window.location.href = link.href;
        });
    });

    /* ---------- Loading buttons ------------------------------------------ */

    function setLoading(button) {
        if (!button || !button.classList.contains('button')) return;
        button.classList.add('is-loading');
        button.setAttribute('aria-busy', 'true');
    }

    Zubac.setLoading = setLoading;
    Zubac.clearLoading = function (button) {
        if (!button) return;
        button.classList.remove('is-loading');
        button.removeAttribute('aria-busy');
    };

    // Forms marked with data-loading show a spinner on their submit button once they actually submit.
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement) || !form.hasAttribute('data-loading')) return;
        setTimeout(() => {
            if (e.defaultPrevented) return;
            if (window.jQuery && jQuery.fn.valid && jQuery(form).data('validator') && !jQuery(form).valid()) return;
            setLoading(e.submitter || form.querySelector('[type="submit"]'));
        }, 0);
    });

    // Restore buttons when the page is shown from the back/forward cache.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        document.querySelectorAll('.button.is-loading').forEach(Zubac.clearLoading);
    });

    /* ---------- Password visibility -------------------------------------- */

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('[data-toggle-password]');
        if (!btn) return;
        const input = btn.parentElement.querySelector('input');
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        btn.innerHTML = icon(show ? 'eye-off' : 'eye');
        btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    });

    /* ---------- Spotlight hover on tiles --------------------------------- */

    document.addEventListener('pointermove', function (e) {
        const tile = e.target.closest && e.target.closest('.tile');
        if (!tile) return;
        const r = tile.getBoundingClientRect();
        tile.style.setProperty('--mx', `${e.clientX - r.left}px`);
        tile.style.setProperty('--my', `${e.clientY - r.top}px`);
    }, { passive: true });

    /* ---------- Greeting -------------------------------------------------- */

    document.querySelectorAll('[data-greeting]').forEach((el) => {
        const h = new Date().getHours();
        el.textContent = h < 5 ? 'Good evening' : h < 12 ? 'Good morning' : h < 18 ? 'Good afternoon' : 'Good evening';
    });

    /* ---------- Order builder (POS) --------------------------------------- */

    Zubac.orderBuilder = function (form) {
        if (!form) return;

        const mode = form.dataset.mode; // "table" | "bar" | "free"
        const isFree = mode === 'free';
        const products = Array.from(form.querySelectorAll('[data-product]'));
        const byId = new Map(products.map((p) => [p.dataset.id, {
            id: p.dataset.id,
            name: p.dataset.name,
            price: parseFloat(p.dataset.price) || 0,
            el: p
        }]));

        const cart = new Map(); // id -> qty
        const MAX_QTY = 99;

        const linesEl = form.querySelector('[data-cart-lines]');
        const emptyEl = form.querySelector('[data-cart-empty]');
        const hiddenEl = form.querySelector('[data-hidden-fields]');
        const totalEls = form.querySelectorAll('[data-cart-total]');
        const valueEls = form.querySelectorAll('[data-cart-value]');
        const countEls = form.querySelectorAll('[data-cart-count]');
        const clearBtn = form.querySelector('[data-cart-clear]');
        const searchInput = form.querySelector('[data-menu-search]');
        const chips = Array.from(form.querySelectorAll('[data-filter]'));
        const menuEmpty = form.querySelector('[data-menu-empty]');
        const tableLabel = form.querySelector('[data-table-label]');

        let activeFilter = 'all';

        function add(id, delta) {
            const item = byId.get(id);
            if (!item) return;
            const next = Math.max(0, Math.min(MAX_QTY, (cart.get(id) || 0) + delta));
            if (next === 0) cart.delete(id); else cart.set(id, next);

            if (delta > 0) {
                item.el.classList.remove('is-bump');
                void item.el.offsetWidth; // restart animation
                item.el.classList.add('is-bump');
            }
            render();
        }

        function render() {
            let total = 0;
            let count = 0;
            linesEl.innerHTML = '';

            cart.forEach((qty, id) => {
                const item = byId.get(id);
                const lineTotal = item.price * qty;
                total += lineTotal;
                count += qty;

                const line = document.createElement('div');
                line.className = 'cart-line';
                line.innerHTML = `
                    <div class="cart-line__info">
                        <div class="cart-line__name"></div>
                        <div class="cart-line__meta"></div>
                    </div>
                    <div class="cart-line__total"></div>
                    <div class="stepper">
                        <button type="button" data-dec aria-label="Remove one">${icon(qty === 1 ? 'trash' : 'minus')}</button>
                        <output>${qty}</output>
                        <button type="button" data-inc aria-label="Add one">${icon('plus')}</button>
                    </div>`;
                line.querySelector('.cart-line__name').textContent = item.name;
                line.querySelector('.cart-line__meta').textContent = isFree
                    ? 'On the house'
                    : `${Zubac.money(item.price)} each`;
                line.querySelector('.cart-line__total').textContent = isFree ? Zubac.money(0) : Zubac.money(lineTotal);
                line.querySelector('[data-dec]').addEventListener('click', () => add(id, -1));
                line.querySelector('[data-inc]').addEventListener('click', () => add(id, 1));
                linesEl.appendChild(line);
            });

            products.forEach((p) => {
                const qty = cart.get(p.dataset.id) || 0;
                const badge = p.querySelector('[data-qty]');
                p.classList.toggle('is-in-cart', qty > 0);
                badge.hidden = qty === 0;
                badge.textContent = qty;
                p.setAttribute('aria-label', `${p.dataset.name}${qty ? `, ${qty} in order` : ''}. Add one.`);
            });

            emptyEl.hidden = cart.size > 0;
            if (clearBtn) clearBtn.hidden = cart.size === 0;
            totalEls.forEach((el) => { el.textContent = Zubac.money(isFree ? 0 : total); });
            valueEls.forEach((el) => { el.textContent = Zubac.money(total); });
            countEls.forEach((el) => { el.textContent = `${count} ${count === 1 ? 'item' : 'items'}`; });
        }

        function applyFilter() {
            const q = (searchInput ? searchInput.value : '').trim().toLowerCase();
            let visible = 0;
            products.forEach((p) => {
                const matchesFilter = activeFilter === 'all'
                    || p.dataset.kind === activeFilter
                    || p.dataset.type === activeFilter;
                const matchesSearch = !q || p.dataset.name.toLowerCase().includes(q) || (p.dataset.type || '').toLowerCase().includes(q);
                const show = matchesFilter && matchesSearch;
                p.hidden = !show;
                if (show) visible++;
            });
            if (menuEmpty) menuEmpty.hidden = visible > 0;
        }

        products.forEach((p) => p.addEventListener('click', () => add(p.dataset.id, 1)));

        chips.forEach((chip) => chip.addEventListener('click', () => {
            activeFilter = chip.dataset.filter;
            chips.forEach((c) => {
                c.classList.toggle('is-active', c === chip);
                c.setAttribute('aria-pressed', c === chip ? 'true' : 'false');
            });
            applyFilter();
        }));

        if (searchInput) searchInput.addEventListener('input', applyFilter);

        if (clearBtn) clearBtn.addEventListener('click', () => {
            cart.clear();
            render();
        });

        form.querySelectorAll('[data-cart-jump]').forEach((btn) => btn.addEventListener('click', () => {
            const target = form.querySelector('.pos__cart');
            if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }));

        form.querySelectorAll('input[name="TableNumber"]').forEach((radio) => radio.addEventListener('change', () => {
            if (tableLabel) tableLabel.textContent = `Table ${radio.value}`;
        }));

        form.addEventListener('submit', (e) => {
            if (mode === 'table' && !form.querySelector('input[name="TableNumber"]:checked')) {
                e.preventDefault();
                Zubac.toast('Pick a table first.', 'error');
                const picker = form.querySelector('.table-picker');
                if (picker) picker.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return;
            }

            if (cart.size === 0) {
                e.preventDefault();
                Zubac.toast('Add at least one item to the order.', 'error');
                return;
            }

            // Contiguous indexes so MVC model binding picks up every line.
            hiddenEl.innerHTML = '';
            let i = 0;
            cart.forEach((qty, id) => {
                [['ArticleId', id], ['Quantity', qty], ['IsSelected', 'true']].forEach(([field, value]) => {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = `SelectedArticles[${i}].${field}`;
                    input.value = value;
                    hiddenEl.appendChild(input);
                });
                i++;
            });

            form.querySelectorAll('[data-submit]').forEach(setLoading);
        });

        // Restore a cart that came back from the server (validation error).
        const initial = form.querySelector('[data-cart-initial]');
        if (initial) {
            try {
                JSON.parse(initial.textContent || '[]').forEach((line) => {
                    const id = String(line.id);
                    if (byId.has(id) && line.qty > 0) cart.set(id, Math.min(MAX_QTY, line.qty));
                });
            } catch (err) { /* ignore malformed state */ }
        }

        const checkedTable = form.querySelector('input[name="TableNumber"]:checked');
        if (checkedTable && tableLabel) tableLabel.textContent = `Table ${checkedTable.value}`;

        document.body.classList.add('has-cart-bar');
        render();
        applyFilter();
    };

    /* ---------- Menu manager (food / drinks settings) -------------------- */

    Zubac.menuManager = function (root) {
        if (!root) return;

        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        const noun = root.dataset.noun || 'Item';

        async function post(url, body) {
            const res = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token ? token.value : ''
                },
                body: JSON.stringify(body)
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        }

        function bindToggle(selector, url, onMsg, offMsg) {
            root.querySelectorAll(selector).forEach((cb) => cb.addEventListener('change', async function () {
                const value = this.checked;
                const row = this.closest('.list-row');
                this.disabled = true;
                try {
                    await post(url, { id: parseInt(this.dataset.id, 10), enabled: value });
                    Zubac.toast(value ? onMsg : offMsg, 'success');
                    if (row && selector === '.js-available') row.dataset.available = value ? 'true' : 'false';
                    refreshCounts();
                } catch (err) {
                    this.checked = !value;
                    Zubac.toast('Could not save the change. Please try again.', 'error');
                } finally {
                    this.disabled = false;
                }
            }));
        }

        bindToggle('.js-available', '/Settings/ToggleAvailable', `${noun} is now available`, `${noun} marked as unavailable`);
        bindToggle('.js-ai', '/Settings/ToggleSommelier', 'AI Sommelier enabled', 'AI Sommelier disabled');

        function refreshCounts() {
            const rows = root.querySelectorAll('.list-row[data-item]');
            const avail = root.querySelector('[data-count-available]');
            const ai = root.querySelector('[data-count-ai]');
            if (avail) avail.textContent = Array.from(rows).filter((r) => r.querySelector('.js-available').checked).length;
            if (ai) ai.textContent = Array.from(rows).filter((r) => r.querySelector('.js-ai').checked).length;
        }

        // Search + type filter
        const search = root.querySelector('[data-list-search]');
        const typeFilter = root.querySelector('[data-list-type]');
        const noResults = root.querySelector('[data-no-results]');

        function filter() {
            const q = search ? search.value.trim().toLowerCase() : '';
            const type = typeFilter ? typeFilter.value : '';
            let visible = 0;
            root.querySelectorAll('.list-row[data-item]').forEach((row) => {
                const show = (!q || row.dataset.name.includes(q)) && (!type || row.dataset.type === type);
                row.classList.toggle('is-hidden', !show);
                if (show) visible++;
            });
            if (noResults) noResults.hidden = visible > 0;
        }

        if (search) search.addEventListener('input', filter);
        if (typeFilter) typeFilter.addEventListener('change', filter);

        // Edit modal
        const modalEl = document.getElementById('editItemModal');
        root.querySelectorAll('[data-edit]').forEach((btn) => btn.addEventListener('click', () => {
            const d = btn.dataset;
            modalEl.querySelector('[name="Id"]').value = d.id;
            modalEl.querySelector('[name="Name"]').value = d.name;
            modalEl.querySelector('[name="Price"]').value = d.price;

            const select = modalEl.querySelector('select[name="Type"]');
            if (d.type && !Array.from(select.options).some((o) => o.value === d.type)) {
                select.add(new Option(d.type, d.type));
            }
            select.value = d.type || '';

            modalEl.querySelector('#editAvailable').checked = d.available === 'True';
            modalEl.querySelector('#editAi').checked = d.ai === 'True';
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }));

        // Add modal: focus the first field when opened
        document.querySelectorAll('.modal').forEach((m) => m.addEventListener('shown.bs.modal', () => {
            const first = m.querySelector('input:not([type="hidden"]), select');
            if (first) first.focus();
        }));
    };

})(window, document);
