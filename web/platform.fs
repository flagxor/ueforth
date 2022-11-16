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

1 value jsslot
: jseval! ( a n index -- ) 0 call ;
: JSWORD: ( "name" )
   create jsslot jseval! jsslot , 1 +to jsslot
   does> @ call ;

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  var text = GetString(a, n);
  eval(text);
  return sp;
})
| JSWORD: jseval ( a n -- )

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

  context.cursor = null;
  context.cursor_time = new Date().getTime();
  setInterval(function() {
    if (context.cursor) {
      var now = new Date().getTime();
      var state = Math.floor((now - context.cursor_time) / 250) % 2;
      if (state) {
        context.cursor.style.visibility = 'hidden';
      } else {
        context.cursor.style.visibility = 'visible';
      }
    }
  }, 50);

  context.GetRawString = function(addr, len) {
    var data = '';
    for (var i = 0; i < len; ++i) {
      data += String.fromCharCode(u8[addr + i]);
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

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    var text = GetString(a, n);
    write(text);
    sp += 4; i32[sp>>2] = 0;
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
    sp += 4; i32[sp>>2] = newline ? -1 : 0;
  }
  return sp;
})
| JSWORD: web-type-raw ( a n -- yield? )
: web-type ( a n -- ) web-type-raw if yield then ;
' web-type is type

r|
(function(sp) {
  context.Update();
  if (globalObj.readline && !context.inbuffer.length) {
    var line = unescape(encodeURIComponent(readline()));
    for (var i = 0; i < line.length; ++i) {
      context.inbuffer.push(line.charCodeAt(i));
    }
    context.inbuffer.push(13);
  }
  if (context.inbuffer.length) {
    sp += 4; i32[sp>>2] = context.inbuffer.shift();
  } else {
    sp += 4; i32[sp>>2] = 0;
  }
  return sp;
})
| JSWORD: web-key-raw ( -- n )
: web-key ( -- n ) begin yield web-key-raw dup if exit then drop again ;
' web-key is key

r|
(function(sp) {
  context.Update();
  if (globalObj.readline) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  sp += 4; i32[sp>>2] = context.inbuffer.length ? -1 : 0;
  return sp;
})
| JSWORD: web-key?-raw ( -- f )
: web-key? ( -- f ) yield web-key?-raw ;
' web-key? is key?

r|
(function(sp) {
  var val = i32[sp>>2]; sp -= 4;
  if (globalObj.quit) {
    quit(val);
  } else {
    Init();
  }
  return sp;
})
| JSWORD: terminate ( n -- )

r|
(function(sp) {
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = 0;  // Disable echo.
  } else {
    sp += 4; i32[sp>>2] = -1;  // Enable echo.
  }
  return sp;
})
| JSWORD: shouldEcho? ( -- f )
shouldEcho? echo !

r|
(function(sp) {
  var mode = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.setMode(mode);
  return sp;
})
| JSWORD: grmode ( mode -- )
: gr   1 grmode ;
: text   0 grmode ;

r|
(function(sp) {
  var c = i32[sp>>2]; sp -= 4;
  var h = i32[sp>>2]; sp -= 4;
  var w = i32[sp>>2]; sp -= 4;
  var y = i32[sp>>2]; sp -= 4;
  var x = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  function HexDig(n) {
    return ('0' + n.toString(16)).slice(-2);
  }
  context.ctx.fillStyle = '#' + HexDig((c >> 16) & 0xff) +
                                HexDig((c >> 8) & 0xff) +
                                HexDig(c & 0xff);
  context.ctx.fillRect(x, y, w, h);
  return sp;
})
| JSWORD: rawbox ( x y w h col -- )
$ffffff value color
: box ( x y w h -- ) color rawbox ;

r|
(function(sp) {
  var h = i32[sp>>2]; sp -= 4;
  var w = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.canvas.width = w;
  context.canvas.height = h;
  return sp;
})
| JSWORD: window ( w h -- )

r|
(function(sp) {
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = 1;
    sp += 4; i32[sp>>2] = 1;
    return sp;
  }
  sp += 4; i32[sp>>2] = context.width;
  sp += 4; i32[sp>>2] = context.height;
  return sp;
})
| JSWORD: viewport@ ( -- w h )

r|
(function(sp) {
  var mp = i32[sp>>2]; sp -= 4;
  var tf = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.text_fraction = tf;
  context.min_text_portion = mp;
  context.Resize();
  return sp;
})
| JSWORD: textRatios ( tf mp -- )

r|
(function(sp) {
  sp += 4; i32[sp>>2] = context.mobile;
  return sp;
})
| JSWORD: mobile ( -- f )

r|
(function(sp) {
  sp += 4; i32[sp>>2] = context.KEYBOARD_HEIGHT;
  return sp;
})
| JSWORD: keys-height ( -- n )

: show-text ( f -- )
  if
    mobile if 3000 120 keys-height + else 1667 120 then
  else
    mobile if 0 keys-height else 0 0 then
  then textRatios ;

r|
(function(sp) {
  var x = i32[sp>>2]; sp -= 4;
  var y = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.ctx.translate(x, y);
  return sp;
})
| JSWORD: translate ( x y )

r|
(function(sp) {
  var d = i32[sp>>2]; sp -= 4;
  var x = i32[sp>>2]; sp -= 4;
  var y = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.ctx.scale(x / d, y / d);
  return sp;
})
| JSWORD: scale ( x y div )

r|
(function(sp) {
  var d = i32[sp>>2]; sp -= 4;
  var angle = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.ctx.rotate(Math.PI * 2 * angle / d);
  return sp;
})
| JSWORD: rotate ( angle div )

r|
(function(sp) {
  if (globalObj.write) {
    return sp;
  }
  context.ctx.save();
  return sp;
})
| JSWORD: gpush

r|
(function(sp) {
  if (globalObj.write) {
    return sp;
  }
  context.ctx.restore();
  return sp;
})
| JSWORD: gpop

r|
(function(sp) {
  var f = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.canvas.style.imageRendering = f ? '' : 'pixelated';
  return sp;
})
| JSWORD: smooth ( f -- )

r|
(function(sp) {
  var session = i32[sp>>2]; sp -= 4;
  var key_len = i32[sp>>2]; sp -= 4;
  var key = i32[sp>>2]; sp -= 4;
  var value_len = i32[sp>>2]; sp -= 4;
  var value = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  if (session) {
    sessionStorage.setItem(context.GetRawString(key, key_len),
                           context.GetRawString(value, value_len));
  } else {
    localStorage.setItem(context.GetRawString(key, key_len),
                         context.GetRawString(value, value_len));
  }
  return sp;
})
| JSWORD: setItem ( a n a n sess -- )

r|
(function(sp) {
  var session = i32[sp>>2]; sp -= 4;
  var key_len = i32[sp>>2]; sp -= 4;
  var key = i32[sp>>2]; sp -= 4;
  var dst_limit = i32[sp>>2]; sp -= 4;
  var dst = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  if (session) {
    var data = sessionStorage.getItem(context.GetRawString(key, key_len));
  } else {
    var data = localStorage.getItem(context.GetRawString(key, key_len));
  }
  if (data === null) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  for (var i = 0; i < dst_limit && i < data.length; ++i) {
    u8[dst + i] = data.charCodeAt(i);
  }
  sp += 4; i32[sp>>2] = data.length;
  return sp;
})
| JSWORD: getItem ( a n a n sess -- n )

r|
(function(sp) {
  var session = i32[sp>>2]; sp -= 4;
  var index = i32[sp>>2]; sp -= 4;
  var key_limit = i32[sp>>2]; sp -= 4;
  var key = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  if (session) {
    var data = sessionStorage.key(index);
  } else {
    var data = localStorage.key(index);
  }
  if (data === null) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  for (var i = 0; i < key_limit && i < data.length; ++i) {
    u8[key + i] = data.charCodeAt(i);
  }
  sp += 4; i32[sp>>2] = data.length;
  return sp;
})
| JSWORD: getKey ( a n sess -- n )

r|
(function(sp) {
  var session = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  if (session) {
    var len = sessionStorage.length;
  } else {
    var len = localStorage.length;
  }
  sp += 4; i32[sp>>2] = len;
  return sp;
})
| JSWORD: keyCount ( sess -- n )

r|
(function(sp) {
  var handle = i32[sp>>2]; sp -= 4;
  context.ReleaseHandle(handle);
  return sp;
})
| JSWORD: release ( handle -- )

r|
(function(sp) {
  var i = context.AllotHandle();
  sp += 4; i32[sp>>2] = i;
  context.handles[i] = new AudioContext();
  return sp;
})
| JSWORD: newAudioContext ( -- h )

r|
(function(sp) {
  var audio_ctx = i32[sp>>2]; sp -= 4;
  var i = context.AllotHandle();
  sp += 4; i32[sp>>2] = i;
  context.handles[i] = context.handles[audio_ctx].createOscillator();
  return sp;
})
| JSWORD: createOscillator ( h -- h )

r|
(function(sp) {
  var audio_ctx = i32[sp>>2]; sp -= 4;
  var i = context.AllotHandle();
  sp += 4; i32[sp>>2] = i;
  context.handles[i] = context.handles[audio_ctx].createGain();
  return sp;
})
| JSWORD: createGain ( h -- h )

r|
(function(sp) {
  var audio_ctx = i32[sp>>2]; sp -= 4;
  var i = context.AllotHandle();
  sp += 4; i32[sp>>2] = i;
  context.handles[i] = context.handles[audio_ctx].createBiquadFilter();
  return sp;
})
| JSWORD: createBiquadFilter ( h -- h )

r|
(function(sp) {
  var audio_ctx = i32[sp>>2]; sp -= 4;
  var i = context.AllotHandle();
  sp += 4; i32[sp>>2] = i;
  context.handles[i] = context.handles[audio_ctx].createBufferSource();
  return sp;
})
| JSWORD: createBufferSource ( h -- h )

forth definitions web

: bye   0 terminate ;

forth definitions
