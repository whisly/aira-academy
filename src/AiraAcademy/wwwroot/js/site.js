/* ============================================================
   Aira Academy — site.js
   Two right-hand drawers (navigation, language) and a persisted
   language preference. No framework, no build step.
   ============================================================ */
(function () {
    'use strict';

    var body = document.body;
    var LANG_KEY = 'aira.lang';
    var LABELS = { en: 'EN', zh: 'CN', ms: 'BM' };
    var DEFAULT_LANG = 'en';

    var activeDrawer = null;
    var lastFocus = null;

    /* ---------------------------------------------------- drawers */

    function drawerFor(name) {
        return document.querySelector('.drawer[data-drawer="' + name + '"]');
    }

    function closeDrawer(drawer) {
        if (!drawer) return;

        drawer.classList.remove('is-open');
        drawer.setAttribute('aria-hidden', 'true');

        var name = drawer.getAttribute('data-drawer');
        var triggers = document.querySelectorAll('[data-drawer-open="' + name + '"]');
        for (var t = 0; t < triggers.length; t++) {
            triggers[t].setAttribute('aria-expanded', 'false');
        }

        if (activeDrawer === drawer) {
            activeDrawer = null;
            body.classList.remove('is-locked');

            if (lastFocus && typeof lastFocus.focus === 'function') {
                lastFocus.focus();
            }
            lastFocus = null;
        }
    }

    function closeAll() {
        var open = document.querySelectorAll('.drawer.is-open');
        for (var i = 0; i < open.length; i++) {
            closeDrawer(open[i]);
        }
    }

    function openDrawer(name) {
        var drawer = drawerFor(name);
        if (!drawer) return;

        // Remember what had focus before the drawer opened, so Escape and the
        // backdrop can put the user back where they were.
        var opener = document.activeElement;
        closeAll();
        lastFocus = opener;

        drawer.classList.add('is-open');
        drawer.setAttribute('aria-hidden', 'false');
        body.classList.add('is-locked');
        activeDrawer = drawer;

        if (opener && opener.setAttribute) {
            opener.setAttribute('aria-expanded', 'true');
        }

        var target = drawer.querySelector('[data-drawer-autofocus]');
        if (target) target.focus();
    }

    /* ---------------------------------------------------- language */

    function readLang() {
        try {
            var stored = window.localStorage.getItem(LANG_KEY);
            return LABELS[stored] ? stored : DEFAULT_LANG;
        } catch (err) {
            return DEFAULT_LANG;
        }
    }

    function applyLang(code) {
        if (!LABELS[code]) code = DEFAULT_LANG;

        document.documentElement.setAttribute('lang', code);

        var triggers = document.querySelectorAll('[data-lang-current]');
        for (var i = 0; i < triggers.length; i++) {
            triggers[i].textContent = LABELS[code];
        }

        var options = document.querySelectorAll('[data-lang-option]');
        for (var j = 0; j < options.length; j++) {
            var on = options[j].getAttribute('data-lang-option') === code;
            options[j].classList.toggle('is-active', on);
            options[j].setAttribute('aria-pressed', on ? 'true' : 'false');
        }

        try {
            window.localStorage.setItem(LANG_KEY, code);
        } catch (err) {
            /* private mode: the choice simply will not persist */
        }

        document.dispatchEvent(new CustomEvent('aira:langchange', { detail: { lang: code } }));
    }

    /* ---------------------------------------------------- wiring */

    document.addEventListener('click', function (event) {
        var target = event.target;

        var opener = target.closest ? target.closest('[data-drawer-open]') : null;
        if (opener) {
            event.preventDefault();
            openDrawer(opener.getAttribute('data-drawer-open'));
            return;
        }

        var closer = target.closest ? target.closest('[data-drawer-close]') : null;
        if (closer) {
            event.preventDefault();
            closeDrawer(closer.closest('.drawer'));
            return;
        }

        var langOption = target.closest ? target.closest('[data-lang-option]') : null;
        if (langOption) {
            applyLang(langOption.getAttribute('data-lang-option'));
            closeDrawer(langOption.closest('.drawer'));
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeAll();
            return;
        }

        // Keep Tab inside the open drawer.
        if (event.key !== 'Tab' || !activeDrawer) return;

        var focusable = activeDrawer.querySelectorAll(
            'a[href], button:not([disabled]), input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        if (!focusable.length) return;

        var first = focusable[0];
        var last = focusable[focusable.length - 1];

        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    });

    // A drawer left open while the viewport grows back to desktop would trap
    // the pointer behind an invisible overlay.
    window.addEventListener('resize', function () {
        if (window.innerWidth > 720) closeAll();
    });

    applyLang(readLang());
})();
