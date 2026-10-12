// Quick Actions' web half. Runs in Root.html, the document every game page iframe sits in. The game's row of
// suggestion chips above the action queue (CoreUI1) gets a width limit and scrolls sideways, so every suggestion the
// plugin lets through stays on one line. The mouse wheel over the row scrolls it. Chips scrolled out of sight stop
// taking clicks, so they cannot block clicks on the world behind them. The row is redrawn by Vue whenever the
// suggestions change, so it is checked on a short poll. Sent again by the plugin every couple of seconds; the guard
// below makes a second copy return at once.
(function () {
  if (window.__slquickactions) return;

  window.__slquickactions = true;

  var POLL_MS = 200;
  var STYLE_ID = 'slqa-style';
  var ROW = '.action-group-middle > .recommend-chips';
  var WHEEL_LINE_PX = 16;

  // The game's row has margin-left 6px and grows with its chips. The cap keeps it clear of the panels in the middle of
  // the bottom edge. The 6px padding gives the highlight glow and the pop-in animation room inside the clipped row, and
  // the matching negative margin keeps the row where the game puts it.
  var CSS = [
    ROW + '{position:relative;flex-wrap:nowrap;max-width:clamp(240px,calc(50vw - 260px),640px);overflow-x:auto;overflow-y:hidden;scrollbar-width:none;padding:6px 2px;margin:-6px 0 -6px 4px}',
    ROW + '::-webkit-scrollbar{display:none}',
    ROW + ' > .recommend-chip{flex-shrink:0}'
  ].join('\n');

  var reported = {};

  // One note per kind of failure, so a page a game update reshaped says so once instead of every poll.
  function diag(kind, error) {
    if (reported[kind]) return;

    reported[kind] = true;

    try {
      console.warn('[Quick Actions] ' + kind + ': ' + error);
    } catch (e) {}
  }

  function addStyle(doc) {
    if (doc.getElementById(STYLE_ID) || !doc.head) return;

    var style = doc.createElement('style');
    style.id = STYLE_ID;
    style.textContent = CSS;
    doc.head.appendChild(style);
  }

  // ===== The row =====

  function patchRow(row) {
    if (!row.__slqa) {
      row.__slqa = true;
      row.addEventListener('wheel', scrollSideways, { passive: false });
      row.addEventListener('scroll', onScrolled);
    }

    // The game's click-through only lets the world have a click outside elements marked like this, so the gaps
    // between chips belong to the row too and the wheel over them scrolls the row.
    if (!row.hasAttribute('data-interactive')) row.setAttribute('data-interactive', '');

    showNewBest(row);
    fitChips(row);
  }

  function scrollSideways(event) {
    var row = event.currentTarget;
    var room = row.scrollWidth - row.clientWidth;

    if (room <= 0) return;

    var delta = Math.abs(event.deltaX) > Math.abs(event.deltaY) ? event.deltaX : event.deltaY;

    if (event.deltaMode === 1) delta *= WHEEL_LINE_PX;
    if (event.deltaMode === 2) delta *= row.clientWidth;

    event.preventDefault();
    row.scrollLeft = Math.max(0, Math.min(room, row.scrollLeft + delta));
  }

  function onScrolled(event) {
    var row = event.currentTarget;

    if (row.__slqaFrame) return;

    var view = row.ownerDocument.defaultView;

    // The page can be dropped between the scroll and this frame.
    if (!view) return;

    row.__slqaFrame = true;
    view.requestAnimationFrame(function () {
      row.__slqaFrame = false;
      fitChips(row);

      // The game re-reads its clickable areas when a class changes, not when an element scrolls.
      row.classList.toggle('slqa-moved');
    });
  }

  // A chip scrolled fully out of sight still has a box on screen to the game's click-through, which would swallow
  // clicks on the world there. Turning its pointer events off takes it out of that list until it scrolls back.
  // Vue keeps the row and its scroll position when the suggestions change, so after scrolling right a new best
  // suggestion could sit out of sight to the left. A new first chip scrolls the row back to the start.
  function showNewBest(row) {
    var first = row.querySelector('.recommend-chip:not(.chip-leave-active)');

    if (first === row.__slqaFirst) return;

    row.__slqaFirst = first;
    row.scrollLeft = 0;
  }

  function fitChips(row) {
    var view = row.getBoundingClientRect();
    var chips = row.children;

    for (var i = 0; i < chips.length; i++) {
      var chip = chips[i];

      if (!chip.classList.contains('recommend-chip')) continue;

      var box = chip.getBoundingClientRect();
      var isInSight = box.right > view.left + 1 && box.left < view.right - 1;
      var wanted = isInSight ? '' : 'none';

      if (chip.style.pointerEvents !== wanted) chip.style.pointerEvents = wanted;
    }
  }

  // ===== Finding the page =====

  // The game's pages are same-origin iframes of this document, created and dropped whenever the game likes.
  function patchPages() {
    var frames = document.querySelectorAll('iframe');

    for (var i = 0; i < frames.length; i++) {
      var src = frames[i].src || '';

      if (/CoreUI1\.html/.test(src)) patchFrame(frames[i], src);
    }
  }

  function patchFrame(frame, src) {
    var doc = null;

    try {
      doc = frame.contentDocument;
    } catch (e) {}

    if (!doc) return diag('page access', 'cannot reach ' + src);
    if (doc.readyState === 'loading') return;

    try {
      addStyle(doc);

      var row = doc.querySelector(ROW);

      if (row) patchRow(row);
    } catch (e) {
      diag('CoreUI1', e);
    }
  }

  setInterval(function () {
    try {
      patchPages();
    } catch (e) {
      diag('poll', e);
    }
  }, POLL_MS);
})();
