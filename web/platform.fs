\ Copyright 2022 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\ Unless required by applicable law or agreed to in writing, software
\ distributed under the License is distributed on an "AS IS" BASIS,
\ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\ See the License for the specific language governing permissions and
\ limitations under the License.

vocabulary web   web definitions

: jseval! ( a n index -- ) 0 call ;

r~
(function(sp) {
  var slot = i32[sp>>2]; sp -= 4;
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  var text = GetString(a, n);
  var parts = text.split('\n');
  var args = parts[0].split(' ');
  var code = '(function(sp) {\n';
  code += 'var results = (function() {\n';
  var params = [];
  var results = [];
  var at_results = false;
  for (var i = 0; i < args.length; ++i) {
    if (args[i].length === 0 ||
        args[i] === '{' ||
        args[i] === '}') {
      continue;
    }
    if (args[i] === '--') {
      at_results = true;
      continue;
    }
    if (at_results) {
      results.push(args[i]);
    } else {
      params.push(args[i]);
    }
  }
  for (var i = params.length - 1; i >= 0; --i) {
    code += 'var ' + params[i] + ' = i32[sp>>2]; sp -= 4;\n';
  }
  code += parts.slice(1).join('\n');
  code += '})();\n';
  if (results.length === 1) {
    code += 'sp += 4; i32[sp>>2] = results;\n';
  } else {
    for (var i = 0; i < results.length; ++i) {
      code += 'sp += 4; i32[sp>>2] = results[' + i + '];\n';
    }
  }
  code += 'return sp;\n';
  code += '})\n';
  objects[slot] = eval(code);
  return sp;
})
~ 1 jseval!

2 value jsslot
: JSWORD: ( "args.." )
   create postpone r~ jsslot 1 call jsslot , 1 +to jsslot
   does> @ call ;

JSWORD: jseval { a n }
  var text = GetString(a, n);
  eval(text);
~

r~
globalObj.ueforth = context;
context.inbuffer = [];
context.Update = function() {};
if (!globalObj.write) {
  function AddMeta(name, content) {
    var meta = document.createElement('meta');
    meta.name = name;
    meta.content = content;
    document.head.appendChild(meta);
  }

  AddMeta('apple-mobile-web-app-capable', 'yes');
  AddMeta('apple-mobile-web-app-status-bar-style', 'black-translucent');
  AddMeta('viewport', 'width=device-width, initial-scale=1.0, ' +
                      'maximum-scale=1.0, user-scalable=no, minimal-ui');

  context.screen = document.getElementById('ueforth');
  if (context.screen === null) {
    context.screen = document.createElement('div');
    context.screen.style.width = '100%';
    document.body.appendChild(context.screen);
  }
  context.filler = document.createElement('div');
  document.body.insertBefore(context.filler, document.body.firstChild);
  context.canvas = document.createElement('canvas');
  context.canvas.width = 1000;
  context.canvas.height = 1000;
  context.canvas.style.width = '1px';
  context.canvas.style.height = '1px';
  context.canvas.style.top = 0;
  context.canvas.style.left = 0;
  context.canvas.style.position = 'fixed';
  context.canvas.style.backgroundColor = '#000';
  context.screen.appendChild(context.canvas);
  context.ctx = context.canvas.getContext('2d');

  context.mouse_x = 0;
  context.mouse_y = 0;
  context.mouse_b = 0;
  context.canvas.onpointermove = function(e) {
    context.mouse_x = e.clientX;
    context.mouse_y = e.clientY;
  };
  context.canvas.onpointerdown = function(e) {
    context.mouse_x = e.clientX;
    context.mouse_y = e.clientY;
    context.mouse_b = 1;
    e.target.setPointerCapture(e.pointerId);
  };
  context.canvas.onpointerup = function(e) {
    context.mouse_x = e.clientX;
    context.mouse_y = e.clientY;
    context.mouse_b = 0;
    e.target.releasePointerCapture(e.pointerId);
  };

  context.cursor = null;
  context.cursor_time = new Date().getTime();
  setInterval(function() {
    if (context.cursor) {
      var now = new Date().getTime();
      var state = Math.floor((now - context.cursor_time) / 250) % 2;
      if (state || context.min_text_portion === 0) {
        context.cursor.style.visibility = 'hidden';
      } else {
        context.cursor.style.visibility = 'visible';
      }
    }
  }, 50);

  context.GetRawString = function(data8, addr, len) {
    var data = '';
    for (var i = 0; i < len; ++i) {
      data += String.fromCharCode(data8[addr + i]);
    }
    return data;
  };

  context.handles = [];
  context.free_handles = [];
  context.next_handle = 1;
  context.AllotHandle = function() {
    if (context.free_handles.length) {
      return context.free_handles.pop();
    }
    return context.next_handle++;
  };
  context.ReleaseHandle = function(id) {
    if (context.handles[id] !== null) {
      context.handles[id] = null;
      context.free_handles.push(id);
    }
  };

  context.terminal = document.createElement('div');
  context.terminal.style.width = '100%';
  context.terminal.style.whiteSpace = 'pre-wrap';
  context.screen.appendChild(context.terminal);
  const DEFAULT_FG = 0x000000;
  const DEFAULT_BG = 0xFFFFFF;
  context.attrib = [DEFAULT_FG, DEFAULT_BG];
  context.lines = [];
  context.escaping = [];

  context.LineFeed = function() {
    var line = document.createElement('pre');
    line.style.width = '100%';
    line.style.whiteSpace = 'pre-wrap';
    line.style.margin = '0px';
    if (context.cy < 0) {
      context.terminal.appendChild(line);
    } else {
      context.terminal.insertBefore(line, context.lines[context.cy].nextSibling);
    }
    context.cx = 0;  // implicit cr
    if (context.cy >= 0) {
      context.dirty[context.cy] = 1;
    }
    ++context.cy;
    context.lines.splice(context.cy, 0, [line, []]);
    context.dirty[context.cy] = 1;
  };

  context.toRGB = function(col) {
    var r = (col >> 16) & 0xff;
    var g = (col >> 8) & 0xff;
    var b = col & 0xff;
    return 'rgb(' + r + ',' + g + ',' + b + ')';
  };

  context.ResetTerminal = function() {
    // TODO: Make this nicer.
    document.body.style.color = context.toRGB(context.attrib[0]);
    document.body.style.backgroundColor = context.toRGB(context.attrib[1]);
    for (var i = 0; i < context.lines.length; ++i) {
      context.terminal.removeChild(context.lines[i][0]);
    }
    context.lines = [];
    context.cx = 0;
    context.cy = -1;
    context.dirty = {};
    context.LineFeed();
  };
  context.ResetTerminal();

  context.TermColor = function(n) {
    n = n & 0xff;
    if (n < 16) {
      var i = n & 8;
      var r = (n & 1) ? (i ? 255 : 192) : (i ? 128 : 0);
      var g = (n & 2) ? (i ? 255 : 192) : (i ? 128 : 0);
      var b = (n & 4) ? (i ? 255 : 192) : (i ? 128 : 0);
      return (r << 16) | (g << 8) | b;
    } else if (n < 232) {
      n -= 16;
      var r = Math.round((Math.floor(n / 36) % 6) * 255 / 5);
      var g = Math.round((Math.floor(n / 6) % 6) * 255 / 5);
      var b = Math.round((n % 6) * 255 / 5);
      return (r << 16) | (g << 8) | b;
    } else {
      n = Math.round((n - 232) * 255 / 23);
      return (n << 16) | (n << 8) | n;
    }
  };

  context.EscapeCode = function(code) {
    var m;
    if (code == '[2J') {
      context.ResetTerminal();
    } else if (code == '[H') {
      context.cx = 0;
      context.cy = 0;
    } else if (code == '[0m') {
      context.attrib = [DEFAULT_FG, DEFAULT_BG];
    } else if (m = code.match(/\[38;5;([0-9]+)m/)) {
      context.attrib[0] = context.TermColor(parseInt(m[1]));
    } else if (m = code.match(/\[48;5;([0-9]+)m/)) {
      context.attrib[1] = context.TermColor(parseInt(m[1]));
    } else {
      console.log('Unknown escape code: ' + code);
    }
  };

  context.Emit = function(ch) {
    if (ch === 27) {
      context.escaping = [27];
      return;
    }
    if (context.escaping.length) {
      context.escaping.push(ch);
      if ((ch >= 65 && ch <= 90) || (ch >= 97 && ch <= 122)) {  // [A-Za-z]
        context.EscapeCode(new TextDecoder('utf-8').decode(new Uint8Array(context.escaping)).slice(1));
        context.escaping = [];
      }
      return;
    }
    if (ch === 12) {
      context.ResetTerminal();
      context.dirty = {};
      return;
    } else if (ch == 10) {
      context.LineFeed();
      return;
    }
    context.dirty[context.cy] = 1;
    if (ch === 8) {
      context.cx = Math.max(0, context.cx - 1);
    } else if (ch === 13) {
      context.cx = 0;
    } else {
      context.lines[context.cy][1].splice(
          context.cx++, 1, [context.attrib[0], context.attrib[1], ch]);
    }
  };

  context.Update = function() {
    const CURSOR = String.fromCharCode(0x2592);
    var count = 0;
    for (var y in context.dirty) {
      ++count;
      var tag = context.lines[y][0];
      var line = context.lines[y][1];
      var parts = [];
      var p = null;
      for (var x = 0; x < line.length; ++x) {
        if (x == 0 ||
            (x == context.cx && y == context.cy) ||
            p[0] != line[x][0] || p[1] != line[x][1]) {
          parts.push([line[x][0], line[x][1], []]);
          p = parts[parts.length - 1];
          if (x == context.cx && y == context.cy) {
            p[0] |= 0x1000000;
          }
        }
        p[2].push(line[x][2]);
      }
      if (x == context.cx && y == context.cy) {
        if (parts.length > 0) {
          parts.push([parts[parts.length - 1][0] | 0x1000000,
                      parts[parts.length - 1][1], []]);
        } else {
          parts.push([context.attrib[0] | 0x1000000,
                      context.attrib[1], []]);
        }
      }
      var ntag = document.createElement('pre');
      ntag.style.width = '100%';
      ntag.style.whiteSpace = 'pre-wrap';
      ntag.style.margin = '0px';
      for (var i = 0; i < parts.length; ++i) {
        var span = document.createElement('span');
        span.innerText = new TextDecoder('utf-8').decode(new Uint8Array(parts[i][2]));
        span.style.color = context.toRGB(parts[i][0]);
        span.style.backgroundColor = context.toRGB(parts[i][1]);
        if (parts[i][0] & 0x1000000) {
          span.style.position = 'relative';
          var cursor = document.createElement('span');
          cursor.classList.add('cursor');
          cursor.innerText = CURSOR;
          cursor.style.position = 'absolute';
          cursor.style.left = '0px';
          cursor.style.backgroundColor = span.style.backgroundColor;
          span.appendChild(cursor);
          context.cursor = cursor;
          context.cursor.style.visibility = 'hidden';
        }
        ntag.appendChild(span);
        if (i === parts.length - 1) {
          ntag.style.color = span.style.color;
          ntag.style.backgroundColor = span.style.backgroundColor;
        }
      }
      context.terminal.replaceChild(ntag, tag);
      context.lines[y][0] = ntag;
    }
    var newline = count > 1 || context.dirty[context.lines.length - 1];
    context.dirty = {};
    if (newline) {
      window.scrollTo(0, document.body.scrollHeight);
    }
  };

  context.keyboard = document.createElement('div');
  context.KEY_HEIGHT = 45;
  context.KEYBOARD_HEIGHT = context.KEY_HEIGHT * 4;
  const TAB = ['&#11134;', 9, 45];
  const PIPE = [String.fromCharCode(124), 124, 45];
  const BACKSLASH = ['\\', 92, 45];
  const ENTER = ['&#9166;', 13, 45];
  const SHIFT = ['&#x21E7;', 1, 45, 0];
  const SHIFT2 = ['&#x2B06;', 0, 45, 0];
  const SHIFT3 = ['=\\<', 3, 45, 0];
  const NUMS = ['?123', 2, 45, 0];
  const ABC = ['ABC', 0, 45, 0];
  const BACKSPACE = ['&#x232B;', 8, 45];
  const BACKTICK = String.fromCharCode(96);
  const TILDE = String.fromCharCode(126);
  const PASTE = ['^V', 22, 30];
  const G1 = ['Gap', 0, 15];
  const KEY_COLOR = 'linear-gradient(to bottom right, #ccc, #999)';
  var keymaps = [
    AddKeymap([
      'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'Newline',
      G1, 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', G1, 'Newline',
      SHIFT, 'z', 'x', 'c', 'v', 'b', 'n', 'm', BACKSPACE, 'Newline',
      NUMS, '/', [' ', 32, 5 * 30], '.', ENTER,
    ]),
    AddKeymap([
      'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'Newline',
      G1, 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', G1, 'Newline',
      SHIFT2, 'Z', 'X', 'C', 'V', 'B', 'N', 'M', BACKSPACE, 'Newline',
      NUMS, '/', [' ', 32, 5 * 30], '.', ENTER,
    ]),
    AddKeymap([
      '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'Newline',
      PASTE, '@', '$', '_', '&', '-', '+', '(', ')', '/', 'Newline',
      SHIFT3, '*', '"', '\'', ':', ';', '!', '?', BACKSPACE, 'Newline',
      ABC, ',', [' ', 32, 5 * 30], '.', ENTER,
    ]),
    AddKeymap([
      TILDE, BACKTICK, '3', '4', '5', '^', '7', '8', '9', '0', 'Newline',
      '#', '@', '$', '_', '&', '-', '=', '{', '}', '\\', 'Newline',
      NUMS, '%', '"', '\'', ':', ';', '[', ']', BACKSPACE, 'Newline',
      ABC, '<', [' ', 32, 5 * 30], '>', ENTER,
    ]),
  ];
  function SwitchKeymap(n) {
    for (var i = 0; i < keymaps.length; ++i) {
      keymaps[i].style.display = i == n ? '' : 'none';
    }
  }
  context.Inject = function(text) {
    var data = new TextEncoder().encode(text);
    for (var i = 0; i < data.length; ++i) {
      context.inbuffer.push(data[i]);
    }
  };
  context.Paste = function() {
    navigator.clipboard.readText().then(function(clipText) {
      context.Inject(clipText);
    });
  };
  function AddKey(keymap, item) {
    if (item === 'Newline') {
      var k = document.createElement('br');
      keymap.appendChild(k);
      return;
    }
    var k = document.createElement('button');
    k.style.fontFamily = 'monospace';
    k.style.verticalAlign = 'middle';
    k.style.border = 'none';
    k.style.margin = '0';
    k.style.padding = '0';
    k.style.backgroundImage = KEY_COLOR;
    k.style.width = (100 / 10) + '%';
    k.style.height = context.KEY_HEIGHT + 'px';
    if (item.length > 2) {
      k.style.width = (100 / 10 * item[2] / 30) + '%';
    }
    if (item[0] === 'Gap') {
      k.style.backgroundColor = '#444';
      k.style.backgroundImage = '';
      keymap.appendChild(k);
      return;
    }
    if (item.length > 1) {
      var keycode = item[1];
    } else {
      var keycode = item[0].charCodeAt(0);
    }
    k.innerHTML = item instanceof Array ? item[0] : item;
    k.onclick = function() {
      if (item.length > 3) {  // SHIFT
        SwitchKeymap(item[1]);
      } else if (keycode === 22) {  // PASTE
        context.Paste();
      } else {
        context.inbuffer.push(keycode);
      }
    };
    keymap.appendChild(k);
  }
  function AddKeymap(keymap) {
    var div = document.createElement('div');
    for (var i = 0; i < keymap.length; ++i) {
      var item = keymap[i];
      AddKey(div, item);
    }
    context.keyboard.appendChild(div);
    return div;
  }
  SwitchKeymap(0);
  context.keyboard.style.position = 'fixed';
  context.keyboard.style.width = '100%';
  context.keyboard.style.bottom = '0px';
  if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
    context.mobile = -1;
    context.tailer = document.createElement('div');
    context.tailer.style.width = '1px';
    context.tailer.style.height = context.KEYBOARD_HEIGHT + 'px';
    context.screen.appendChild(context.tailer);
    document.body.appendChild(context.keyboard);
  } else {
    context.mobile = 0;
  }

  context.text_fraction = context.mobile ? 3000 : 1667;
  context.min_text_portion = 120 + (context.mobile ? context.KEYBOARD_HEIGHT : 0);
  context.mode = 1;
  function setMode(mode) {
    if (context.mode === mode) {
      return ;
    }
    if (mode) {
      context.filler.style.display = '';
      context.canvas.style.display = '';
    } else {
      context.filler.style.display = 'none';
      context.canvas.style.display = 'none';
    }
    context.mode = mode;
  }
  context.setMode = setMode;
  function Resize() {
    var width = window.innerWidth;
    var theight = Math.max(context.min_text_portion,
                           Math.floor(window.innerHeight *
                                      context.min_text_portion / 10000));
    var height = window.innerHeight - theight;
    if (width === context.width && height === context.height) {
      return;
    }
    context.canvas.style.width = width + 'px';
    context.canvas.style.height = height + 'px';
    if (context.text_fraction == 0 &&
        context.min_text_portion == 0) {
      context.filler.style.width = '1px';
      context.filler.style.height = '0px';
    } else {
      context.filler.style.width = '1px';
      context.filler.style.height = height + 'px';
    }
    context.width = width;
    context.height = height;
  }
  context.Resize = Resize;
  function Clear() {
    Resize();
    context.ctx.fillStyle = '#000';
    context.ctx.fillRect(0, 0, context.canvas.width, context.canvas.height);
  }
  context.Clear = Clear;
  window.onresize = function(e) {
    Resize();
  };
  function KeyPress(e) {
    context.cursor_time = new Date().getTime();
    context.inbuffer.push(e.keyCode);
    e.preventDefault();
    return false;
  }
  window.onkeypress = KeyPress;
  function KeyDown(e) {
    if (e.keyCode == 8) {
      context.cursor_time = new Date().getTime();
      context.inbuffer.push(e.keyCode);
      e.preventDefault();
      return false;
    }
  }
  window.onkeydown = KeyDown;
  window.addEventListener('paste', function(e) {
    context.Inject(e.clipboardData.getData('text'));
  });
  setMode(0);
  context.Clear();
}
~ jseval

JSWORD: web-type-raw { a n -- yld }
  if (globalObj.write) {
    var text = GetString(a, n);
    write(text);
    return 0;
  } else {
    var newline = false;
    for (var i = 0; i < n; ++i) {
      var ch = u8[a + i];
      if (ch == 10) { newline = true; }
      context.Emit(ch);
    }
    if (newline) {
      context.Update();
    }
    return newline ? -1 : 0;
  }
~

JSWORD: web-key-raw { -- n }
  context.Update();
  if (globalObj.readline && !context.inbuffer.length) {
    var line = unescape(encodeURIComponent(readline()));
    for (var i = 0; i < line.length; ++i) {
      context.inbuffer.push(line.charCodeAt(i));
    }
    context.inbuffer.push(13);
  }
  if (context.inbuffer.length) {
    return context.inbuffer.shift();
  } else {
    return 0;
  }
~

JSWORD: web-key?-raw { -- f }
  context.Update();
  if (globalObj.readline) {
    return -1;
  }
  return context.inbuffer.length ? -1 : 0;
~

JSWORD: web-terminate { retval }
  if (globalObj.quit) {
    quit(retval);
  } else {
    Init();
  }
~

JSWORD: shouldEcho? { -- f }
  if (globalObj.write) {
    return 0;  // Disable echo.
  } else {
    return -1;  // Enable echo.
  }
~
shouldEcho? echo !

JSWORD: grmode { mode }
  context.setMode(mode);
~
: gr   1 grmode ;
: text   0 grmode ;

JSWORD: color! { c }
  function HexDig(n) {
    return ('0' + n.toString(16)).slice(-2);
  }
  context.ctx.fillStyle = '#' + HexDig((c >> 16) & 0xff) +
                                HexDig((c >> 8) & 0xff) +
                                HexDig(c & 0xff);
  context.ctx.strokeStyle = '#' + HexDig((c >> 16) & 0xff) +
                                  HexDig((c >> 8) & 0xff) +
                                  HexDig(c & 0xff);
~

JSWORD: lineWidth { w -- }
  context.ctx.lineWidth = w;
~

JSWORD: box { x y w h }
  context.ctx.fillRect(x, y, w, h);
~

JSWORD: beginPath { }
  context.ctx.beginPath();
~

JSWORD: moveTo { x y }
  context.ctx.moveTo(x, y);
~

JSWORD: lineTo { x y }
  context.ctx.lineTo(x, y);
~

JSWORD: stroke { }
  context.ctx.stroke();
~

JSWORD: fill { }
  context.ctx.fill();
~

: line ( x1 y1 x2 y2 -- )
  beginPath
  >r >r moveTo
  r> r> lineTo
  stroke
;

JSWORD: window { w h }
  context.canvas.width = w;
  context.canvas.height = h;
~

JSWORD: viewport@ { -- w h }
  return [context.width, context.height];
~

JSWORD: textRatios { tf mp }
  context.text_fraction = tf;
  context.min_text_portion = mp;
  context.Resize();
~

JSWORD: mobile { -- f }
  return context.mobile;
~

JSWORD: keys-height { -- n }
  return context.KEYBOARD_HEIGHT;
~

: show-text ( f -- )
  if
    mobile if 3000 120 keys-height + else 1667 120 then
  else
    mobile if 0 keys-height else 0 0 then
  then textRatios ;

JSWORD: translate { x y }
  context.ctx.translate(x, y);
~

JSWORD: scale { x y div }
  context.ctx.scale(x / div, y / div);
~

JSWORD: rotate { angle div }
  context.ctx.rotate(Math.PI * 2 * angle / div);
~

JSWORD: gpush { }
  context.ctx.save();
~

JSWORD: gpop { }
  context.ctx.restore();
~

JSWORD: smooth { f }
  context.canvas.style.imageRendering = f ? '' : 'pixelated';
~

JSWORD: setItem { value value_len key key_len session }
  if (session) {
    sessionStorage.setItem(context.GetRawString(u8, key, key_len),
                           context.GetRawString(u8, value, value_len));
  } else {
    localStorage.setItem(context.GetRawString(u8, key, key_len),
                         context.GetRawString(u8, value, value_len));
  }
~

JSWORD: getItem { dst dst_limit key key_len session -- n }
  if (session) {
    var data = sessionStorage.getItem(context.GetRawString(u8, key, key_len));
  } else {
    var data = localStorage.getItem(context.GetRawString(u8, key, key_len));
  }
  if (data === null) {
    return -1;
  }
  for (var i = 0; i < dst_limit && i < data.length; ++i) {
    u8[dst + i] = data.charCodeAt(i);
  }
  return data.length;
~

JSWORD: removeItem { key key_len session }
  if (session) {
    sessionStorage.removeItem(context.GetRawString(u8, key, key_len));
  } else {
    localStorage.removeItem(context.GetRawString(u8, key, key_len));
  }
~

JSWORD: clearItems { session }
  if (session) {
    sessionStorage.clear();
  } else {
    localStorage.clear();
  }
~

JSWORD: getKey { key key_limit index session -- n }
  if (session) {
    var data = sessionStorage.key(index);
  } else {
    var data = localStorage.key(index);
  }
  if (data === null) {
    return -1;
  }
  for (var i = 0; i < key_limit && i < data.length; ++i) {
    u8[key + i] = data.charCodeAt(i);
  }
  return i;
~

JSWORD: keyCount { session -- n }
  if (session) {
    var len = sessionStorage.length;
  } else {
    var len = localStorage.length;
  }
  return len;
~

JSWORD: release { handle }
  context.ReleaseHandle(handle);
~

JSWORD: importScripts { dst dst_limit -- n }
  if (context.scripts === undefined) {
    return 0;
  }
  var data = context.scripts;
  for (var i = 0; i < dst_limit && i < data.length; ++i) {
    u8[dst + i] = data[i];
  }
  return data.length;
~

r~
context.audio_context = null;
context.audio_channels = [];
context.initAudio = function() {
  if (context.audio_context !== null) {
    return;
  }
  context.audio_context = new AudioContext();
  var master = context.audio_context.createGain();
  master.connect(context.audio_context.destination);
  master.gain.value = 1/8;
  for (var i = 0; i < 8; ++i) {
    var oscillator = context.audio_context.createOscillator();
    oscillator.type = 'sine';
    var gain = context.audio_context.createGain();
    gain.gain.value = 0;
    oscillator.connect(gain);
    gain.connect(master);
    oscillator.start();
    context.audio_channels.push([gain, oscillator]);
  }
};
~ jseval

JSWORD: tone { pitch volume channel -- }
  context.initAudio();
  context.audio_channels[channel][0].gain.value = volume / 100;
  context.audio_channels[channel][1].frequency.value = 27.5 * Math.pow(2, (pitch - 21) / 12);
~
: silence   8 0 do 0 0 i tone loop ;

JSWORD: ms-ticks { -- ms }
  return Date.now();
~

r~
if (!globalObj.write) {
  var filepick = document.createElement('input');
  filepick.type = 'file';
  filepick.name = 'files[]';
  filepick.style.display = 'none';
  document.body.appendChild(filepick);
  context.handleImport = function() {
    document.body.onblur = function() {
      document.body.onfocus = function() {
        document.body.onfocus = null;
        setTimeout(function() {
          if (filepick.files.length === 0) {
            context.filepick_result = 0;
            context.filepick_filename = null;
          }
        }, 500);
      };
    };
  };
  filepick.onchange = function(event) {
    if (event.target.files.length > 0) {
      var reader = new FileReader();
      reader.onload = function(e) {
        var data = context.GetRawString(
           new Uint8Array(e.target.result), 0, e.target.result.byteLength);
        try {
          if (context.filepick_filename === null) { throw 'fail'; }
          localStorage.setItem(context.filepick_filename, data);
          context.filepick_result = -1;
          context.filepick_filename = null;
        } catch (e) {
          context.filepick_result = 0;
          context.filepick_filename = null;
        }
      };
      reader.readAsArrayBuffer(event.target.files[0]);
    } else {
      context.filepick_filename = null;
      context.filepick_result = 0;
    }
  };
  context.filepick = filepick;
  context.filepick_filename = null;
  context.filepick_result = 0;
}
~ jseval

JSWORD: upload-start { filename n }
  context.filepick_filename = context.GetRawString(u8, filename, n);
  context.handleImport();
  context.filepick.click();
~

JSWORD: upload-done? { -- f }
  return context.filepick_filename === null ? -1 : 0;
~

JSWORD: upload-success? { -- f }
  return context.filepick_result;
~

JSWORD: log { a n -- }
  console.log(GetString(a, n));
~

JSWORD: font { a n -- }
  context.ctx.font = GetString(a, n);
~

JSWORD: fillText { a n x y -- }
  context.ctx.fillText(GetString(a, n), x, y);
~

JSWORD: textWidth { a n -- w }
  return context.ctx.measureText(GetString(a, n)).width;
~

JSWORD: mouse { -- x y }
  return [context.mouse_x, context.mouse_y];
~

JSWORD: button { -- b }
  return context.mouse_b;
~

JSWORD: random { n -- n }
  return Math.floor(Math.random() * n);
~

0 0 importScripts constant scripts#
create scripts   scripts# allot
scripts scripts# importScripts drop

forth definitions web

: ms-ticks   ms-ticks ;

forth definitions
