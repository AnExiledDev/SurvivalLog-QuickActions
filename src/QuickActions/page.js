// Quick Actions' web half. Runs in Root.html, the document every game page iframe sits in. The game's row of
// suggestion chips above the action queue (CoreUI1) gets a width limit and scrolls sideways, so every suggestion the
// plugin lets through stays on one line. The mouse wheel over the row scrolls it. Chips scrolled out of sight stop
// taking clicks, so they cannot block clicks on the world behind them.
// Once the row's page is found it is held: a slow poll only checks that it is still the same page, and mutation
// observers on the row say when the chips change, so the chips are measured once per change instead of on every poll.
// The plugin sends this script again only when the page has lost it; the guard below makes a second copy return at once.
(function () {
  if (window.__slquickactions) return;

  window.__slquickactions = true;

  var POLL_MS = 250;
  var PAGE = 'CoreUI1.html';
  var STYLE_ID = 'slqa-style';
  var MIDDLE = '.action-group-middle';
  var ROW_CLASS = 'recommend-chips';
  var CHIP_CLASS = 'recommend-chip';
  var LEAVING_CLASS = 'chip-leave-active';
  var WHEEL_LINE_PX = 16;
  var STATUS_BOX = '.bottom-right-container';
  var STATUS = STATUS_BOX + ' > .status-panel';

  // The game's row has margin-left 6px and grows with its chips. The cap keeps it clear of the panels in the middle of
  // the bottom edge. The 6px padding gives the highlight glow and the pop-in animation room inside the clipped row, and
  // the matching negative margin keeps the row where the game puts it.
  var ROW = MIDDLE + ' > .' + ROW_CLASS;
  var CSS = [
    ROW + '{position:relative;flex-wrap:nowrap;max-width:clamp(240px,calc(50vw - 260px),640px);overflow-x:auto;overflow-y:hidden;scrollbar-width:none;padding:6px 2px;margin:-6px 0 -6px 4px}',
    ROW + '::-webkit-scrollbar{display:none}',
    ROW + ' > .recommend-chip{flex-shrink:0}',
    // The status bars in the bottom right sit above the action panel, and their rows are as wide as the widest row of
    // buffs, so their empty space covers the right end of the row. Only what is drawn there takes the mouse now;
    // buffs leaving (bf-leave-active) keep the game's own pointer-events:none.
    STATUS_BOX + ',' + STATUS + ',' + STATUS + ' .status-row,' + STATUS + ' .buff-container{pointer-events:none}',
    STATUS_BOX + ' > :not(.status-panel),' + STATUS + ' > :not(.status-row),' + STATUS + ' .status-row > :not(.buff-container),' + STATUS + ' .buff:not(.bf-leave-active){pointer-events:auto}'
  ].join('\n');

  // What a chip change looks like: chips added or removed, a chip's class (its enter, leave and move transitions, the
  // highlight) or its text. The row's own class is left out: the script toggles it after every scroll.
  var ROW_CHANGES = { childList: true, subtree: true, attributes: true, attributeFilter: ['class'], characterData: true };

  // Live: the browser keeps it current, so the poll reads it without searching the document.
  var frames = document.getElementsByTagName('iframe');
  var reported = {};
  var held = null;

  // One note per kind of failure, so a page a game update reshaped says so once instead of every poll.
  function diag(kind, error) {
    if (reported[kind]) return;

    reported[kind] = true;

    try {
      console.warn('[Quick Actions] ' + kind + ': ' + error);
    } catch (e) {}
  }

  // ===== Holding the page =====

  // The common case of every poll: the same page is still there, so nothing is searched or measured. A page the game
  // dropped, or reloaded into the same frame, still reports its old elements as connected, so the frame is checked too.
  function poll() {
    if (isHeld()) return;

    release();
    findPage();
  }

  function isHeld() {
    return !!held && held.frame.isConnected && held.frame.contentDocument === held.doc && held.middle.isConnected;
  }

  function release() {
    if (!held) return;

    held.middleWatch.disconnect();
    held.rowWatch.disconnect();
    held = null;
  }

  // The game's pages are same-origin iframes of this document, created and dropped whenever the game likes.
  function findPage() {
    for (var i = 0; i < frames.length; i++) {
      var frame = frames[i];

      if ((frame.src || '').indexOf(PAGE) < 0) continue;

      try {
        holdFrame(frame);
      } catch (e) {
        diag('CoreUI1', e);
      }

      if (held) return;
    }
  }

  function holdFrame(frame) {
    var doc = null;

    try {
      doc = frame.contentDocument;
    } catch (e) {}

    if (!doc) return diag('page access', 'cannot reach ' + frame.src);
    if (doc.readyState === 'loading') return;

    var middle = doc.querySelector(MIDDLE);

    if (!middle) return;

    addStyle(doc);
    held = {
      frame: frame,
      doc: doc,
      view: doc.defaultView,
      middle: middle,
      row: null,
      isRefitDue: false,
      middleWatch: new MutationObserver(guarded('row change', takeRow)),
      rowWatch: new MutationObserver(guarded('chip change', onChipsChanged))
    };

    // The row is only drawn while there are suggestions, so it comes and goes inside the middle group.
    held.middleWatch.observe(middle, { childList: true });
    watchResize(held.view);
    takeRow();
  }

  function addStyle(doc) {
    if (doc.getElementById(STYLE_ID) || !doc.head) return;

    var style = doc.createElement('style');

    style.id = STYLE_ID;
    style.textContent = CSS;
    doc.head.appendChild(style);
  }

  function guarded(kind, work) {
    return function (records) {
      try {
        work(records);
      } catch (e) {
        diag(kind, e);
      }
    };
  }

  // ===== The row =====

  function takeRow() {
    if (!held) return;

    var row = childWithClass(held.middle, ROW_CLASS);

    if (row === held.row) return;

    held.rowWatch.disconnect();
    held.row = row;

    if (!row) return;

    patchRow(row);
    held.rowWatch.observe(row, ROW_CHANGES);
    refitSoon();
  }

  function childWithClass(parent, className) {
    var children = parent.children;

    for (var i = 0; i < children.length; i++) {
      if (children[i].classList.contains(className)) return children[i];
    }

    return null;
  }

  function patchRow(row) {
    if (row.__slqa) return;

    row.__slqa = true;
    row.addEventListener('wheel', scrollSideways, { passive: false });
    row.addEventListener('scroll', onScrolled);
  }

  function onChipsChanged(records) {
    if (!held) return;

    for (var i = 0; i < records.length; i++) {
      var record = records[i];

      if (record.type === 'attributes' && record.target === held.row) continue;

      refitSoon();

      return;
    }
  }

  function watchResize(view) {
    if (!view || view.__slqaResize) return;

    view.__slqaResize = true;
    view.addEventListener('resize', function () {
      if (held && held.view === view) refitSoon();
    });
  }

  // However many changes come in one frame, the chips are measured once, after Vue has drawn them all.
  function refitSoon() {
    if (held.isRefitDue || !held.view) return;

    held.isRefitDue = true;
    held.view.requestAnimationFrame(guarded('refit', refit));
  }

  function refit() {
    if (!held) return;

    held.isRefitDue = false;

    var row = held.row;

    if (!row || !row.isConnected) return;

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

  // Vue keeps the row and its scroll position when the suggestions change, so after scrolling right a new best
  // suggestion could sit out of sight to the left. A new first chip scrolls the row back to the start.
  function showNewBest(row) {
    var first = firstChip(row);

    if (first === row.__slqaFirst) return;

    row.__slqaFirst = first;
    row.scrollLeft = 0;
  }

  function firstChip(row) {
    var chips = row.children;

    for (var i = 0; i < chips.length; i++) {
      var classes = chips[i].classList;

      if (classes.contains(CHIP_CLASS) && !classes.contains(LEAVING_CLASS)) return chips[i];
    }

    return null;
  }

  // A chip scrolled fully out of sight still has a box on screen to the game's click-through, which would swallow
  // clicks on the world there. Turning its pointer events off takes it out of that list until it scrolls back.
  // Every box is read before any style is written, so the page lays out once.
  function fitChips(row) {
    var view = row.getBoundingClientRect();
    var chips = row.children;
    var wanted = [];

    for (var i = 0; i < chips.length; i++) {
      var chip = chips[i];

      if (!chip.classList.contains(CHIP_CLASS)) {
        wanted.push(null);

        continue;
      }

      var box = chip.getBoundingClientRect();

      wanted.push(box.right > view.left + 1 && box.left < view.right - 1 ? '' : 'none');
    }

    for (var j = 0; j < wanted.length; j++) {
      if (wanted[j] !== null && chips[j].style.pointerEvents !== wanted[j]) chips[j].style.pointerEvents = wanted[j];
    }
  }

  setInterval(guarded('poll', poll), POLL_MS);
})();
