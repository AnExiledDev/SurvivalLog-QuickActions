// Test code only. A Root.html with game page frames in it, holding just what page.js touches. Elements report their
// changes to mutation observers, which run only on flush(), the way a browser runs them before it paints. Animation
// frames and the script's poll run by hand. Every search of a document and every layout read is counted.
var window = this;
var console = { warnings: [], warn: function (message) { this.warnings.push(String(message)); } };
var counts = { searches: 0, measures: 0 };
var polls = [];
var frameCallbacks = [];
var observers = [];
var pending = [];

function setInterval(callback) {
    polls.push(callback);

    return polls.length;
}

// One tick of the script's poll, then the observers it woke.
function poll() {
    polls.forEach(function (callback) { callback(); });
    flush();
}

// One animation frame: the callbacks asked for before it, then the observers.
function nextFrame() {
    var due = frameCallbacks;

    frameCallbacks = [];
    due.forEach(function (callback) { callback(); });
    flush();
}

function resetCounts() {
    counts.searches = 0;
    counts.measures = 0;
}

// ===== Mutation observers =====

function MutationObserver(callback) {
    this.callback = callback;
    this.targets = [];
    this.records = [];
}

MutationObserver.prototype.observe = function (target, options) {
    this.targets.push({ node: target, options: options });

    if (observers.indexOf(this) < 0) observers.push(this);
};

MutationObserver.prototype.disconnect = function () {
    var at = pending.indexOf(this);

    this.targets = [];
    this.records = [];

    if (at >= 0) pending.splice(at, 1);
};

MutationObserver.prototype.takeRecords = function () {
    var records = this.records;

    this.records = [];

    return records;
};

function isInside(node, ancestor) {
    for (var at = node; at; at = at.parentElement) {
        if (at === ancestor) return true;
    }

    return false;
}

function hears(watch, record) {
    var options = watch.options;

    if (record.type === 'childList' && !options.childList) return false;
    if (record.type === 'characterData' && !options.characterData) return false;
    if (record.type === 'attributes' && !options.attributes) return false;
    if (record.type === 'attributes' && options.attributeFilter && options.attributeFilter.indexOf(record.attributeName) < 0) return false;

    return watch.node === record.target || (!!options.subtree && isInside(record.target, watch.node));
}

function report(record) {
    observers.forEach(function (observer) {
        if (!observer.targets.some(function (watch) { return hears(watch, record); })) return;

        observer.records.push(record);

        if (pending.indexOf(observer) < 0) pending.push(observer);
    });
}

// An observer that keeps waking itself fails the test instead of hanging it.
function flush() {
    for (var round = 0; pending.length > 0; round++) {
        if (round > 50) throw new Error('an observer keeps waking itself');

        var due = pending;

        pending = [];
        due.forEach(function (observer) {
            var records = observer.takeRecords();

            if (records.length > 0) observer.callback(records);
        });
    }
}

// ===== Elements =====

function ClassList(element) {
    this.element = element;
    this.names = [];
}

ClassList.prototype.contains = function (name) {
    return this.names.indexOf(name) >= 0;
};

ClassList.prototype.add = function (name) {
    if (this.contains(name)) return;

    this.names.push(name);
    report({ type: 'attributes', target: this.element, attributeName: 'class' });
};

ClassList.prototype.remove = function (name) {
    if (!this.contains(name)) return;

    this.names.splice(this.names.indexOf(name), 1);
    report({ type: 'attributes', target: this.element, attributeName: 'class' });
};

ClassList.prototype.toggle = function (name) {
    if (this.contains(name)) this.remove(name);
    else this.add(name);
};

function Element(tag, doc) {
    this.tagName = tag.toUpperCase();
    this.ownerDocument = doc;
    this.parentElement = null;
    this.children = [];
    this.attributes = {};
    this.listeners = {};
    this.style = {};
    this.classList = new ClassList(this);
    this.id = '';
    this.textContent = '';
    this.scrollLeft = 0;
    this.scrollWidth = 0;
    this.clientWidth = 0;
    this.box = { left: 0, right: 0 };
}

// Connected while its document's root element is above it. An element of a page the game dropped still is.
Object.defineProperty(Element.prototype, 'isConnected', {
    get: function () {
        var top = this;

        while (top.parentElement) top = top.parentElement;

        return !!top.ownerDocument && top === top.ownerDocument.documentElement;
    }
});

Element.prototype.appendChild = function (child) {
    return this.insertBefore(child, null);
};

Element.prototype.insertBefore = function (child, before) {
    var at = before ? this.children.indexOf(before) : this.children.length;

    child.parentElement = this;
    this.children.splice(at, 0, child);
    report({ type: 'childList', target: this });

    return child;
};

Element.prototype.removeChild = function (child) {
    this.children.splice(this.children.indexOf(child), 1);
    child.parentElement = null;
    report({ type: 'childList', target: this });

    return child;
};

Element.prototype.hasAttribute = function (name) {
    return Object.prototype.hasOwnProperty.call(this.attributes, name);
};

Element.prototype.setAttribute = function (name, value) {
    this.attributes[name] = String(value);
    report({ type: 'attributes', target: this, attributeName: name });
};

Element.prototype.addEventListener = function (type, listener) {
    (this.listeners[type] = this.listeners[type] || []).push(listener);
};

Element.prototype.getBoundingClientRect = function () {
    counts.measures++;

    return { left: this.box.left, right: this.box.right };
};

Element.prototype.querySelector = function (selector) {
    counts.searches++;

    return findByClass(this, selector);
};

Element.prototype.querySelectorAll = function () {
    counts.searches++;
    throw new Error('the fake page has no querySelectorAll');
};

// Only the one selector shape the script uses to find the page: a single class.
function findByClass(from, selector) {
    if (!/^\.[\w-]+$/.test(selector)) throw new Error('the fake page cannot search for ' + selector);

    var name = selector.substring(1);

    for (var i = 0; i < from.children.length; i++) {
        var child = from.children[i];
        var found = child.classList.contains(name) ? child : findByClass(child, selector);

        if (found) return found;
    }

    return null;
}

function element(doc, tag, className) {
    var made = new Element(tag, doc);

    if (className) made.classList.names = className.split(' ');

    return made;
}

// A text change inside a chip, as Vue makes it when a label changes.
function setText(node, text) {
    node.textContent = text;
    report({ type: 'characterData', target: node });
}

// ===== Documents =====

function View() {
    this.listeners = {};
}

View.prototype.requestAnimationFrame = function (callback) {
    frameCallbacks.push(callback);

    return frameCallbacks.length;
};

View.prototype.addEventListener = function (type, listener) {
    (this.listeners[type] = this.listeners[type] || []).push(listener);
};

View.prototype.fire = function (type) {
    (this.listeners[type] || []).forEach(function (listener) { listener({ type: type }); });
};

function Doc() {
    this.readyState = 'complete';
    this.defaultView = new View();
    this.documentElement = new Element('html', this);
    this.head = new Element('head', this);
    this.body = new Element('body', this);
    this.documentElement.children = [this.head, this.body];
    this.head.parentElement = this.documentElement;
    this.body.parentElement = this.documentElement;
}

Doc.prototype.createElement = function (tag) {
    return new Element(tag, this);
};

Doc.prototype.querySelector = function (selector) {
    counts.searches++;

    return findByClass(this.documentElement, selector);
};

Doc.prototype.querySelectorAll = Element.prototype.querySelectorAll;

Doc.prototype.getElementById = function (id) {
    counts.searches++;

    return findById(this.documentElement, id);
};

function findById(from, id) {
    for (var i = 0; i < from.children.length; i++) {
        var child = from.children[i];
        var found = child.id === id ? child : findById(child, id);

        if (found) return found;
    }

    return null;
}

// ===== CoreUI1 and the row =====

// The row is 200px wide on screen; chips are 60px wide with 10px gaps, so the third ends at 200 and the fourth is
// out of sight.
var ROW_WIDTH = 200;

function chip(doc, name) {
    var made = element(doc, 'div', 'recommend-chip');
    var label = element(doc, 'span', 'chip-name');

    label.textContent = name;
    made.appendChild(label);

    return made;
}

function row(doc, names) {
    var made = element(doc, 'div', 'recommend-chips');

    made.box = { left: 0, right: ROW_WIDTH };
    names.forEach(function (name) { made.appendChild(chip(doc, name)); });
    layOut(made);

    return made;
}

// Where each chip sits after the row scrolled to scrollLeft.
function layOut(rowElement) {
    rowElement.children.forEach(function (child, at) {
        var left = at * 70 - rowElement.scrollLeft;

        child.box = { left: left, right: left + 60 };
    });
}

// A CoreUI1 document, with or without a row of chips in its middle group.
function corePage(names) {
    var doc = new Doc();
    var panel = element(doc, 'div', 'action-panel');
    var middle = element(doc, 'div', 'action-group-middle');

    doc.body.appendChild(panel);
    panel.appendChild(middle);

    if (names) middle.appendChild(row(doc, names));

    return { doc: doc, middle: middle, row: function () { return middle.children[0] || null; } };
}

function chipNamed(rowElement, name) {
    for (var i = 0; i < rowElement.children.length; i++) {
        if (rowElement.children[i].children[0].textContent === name) return rowElement.children[i];
    }

    return null;
}

// The pointer-events of each chip, in row order: "on" or "off".
function clickable(rowElement) {
    return rowElement.children.map(function (child) { return child.style.pointerEvents === 'none' ? 'off' : 'on'; }).join(',');
}

// ===== Root.html =====

var document = new Doc();
var rootFrames = [];

document.getElementsByTagName = function (tag) {
    if (tag !== 'iframe') throw new Error('the fake root only lists iframes');

    return rootFrames;
};

function openFrame(page, src) {
    var frame = element(document, 'iframe');

    frame.src = src || 'https://game.local/CoreUI1.html';
    frame.contentDocument = page.doc;
    document.body.appendChild(frame);
    rootFrames.push(frame);

    return frame;
}

function closeFrame(frame) {
    document.body.removeChild(frame);
    rootFrames.splice(rootFrames.indexOf(frame), 1);
}
