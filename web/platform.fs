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

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  var text = GetString(a, n);
  eval(text);
  return sp;
})
| 1 jseval!
: jseval ( a n -- ) 1 call ;

r~
context.inbuffer = [];
context.outbuffer = [];
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

  context.AddLine = function() {
    if (context.last_line) {
      context.Update(true);
    }
    context.outbuffer = [];
    var line = document.createElement('pre');
    line.style.width = '100%';
    line.style.whiteSpace = 'pre-wrap';
    line.style.margin = '0px';
    context.terminal.appendChild(line);
    context.lines.push(line);
    context.last_line = line;
  };

  context.terminal = document.createElement('div');
  context.terminal.style.width = '100%';
  context.terminal.style.whiteSpace = 'pre-wrap';
  context.screen.appendChild(context.terminal);
  context.lines = [];
  context.last_line = null;
  context.outbuffer = [];

  context.ResetTerminal = function() {
    for (var i = 0; i < context.lines.length; ++i) {
      context.terminal.removeChild(context.lines[i]);
    }
    context.lines = [];
    context.last_line = null;
    context.outbuffer = [];
    context.AddLine();
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
    context.inbuffer.push(e.keyCode);
    e.preventDefault();
    return false;
  }
  window.onkeypress = KeyPress;
  function KeyDown(e) {
    if (e.keyCode == 8) {
      context.inbuffer.push(e.keyCode);
      e.preventDefault();
      return false;
    }
  }
  window.onkeydown = KeyDown;
  context.Update = function(newline) {
    var cursor = String.fromCharCode(0x2592);
    context.last_line.innerText = new TextDecoder('utf-8').decode(
        new Uint8Array(context.outbuffer)) + (newline ? '' : cursor);
  };
  context.ResetTerminal();
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
      if (ch == 12) {
        context.ResetTerminal();
        context.outbuffer = [];
      } else if (ch == 8) {
        context.outbuffer = context.outbuffer.slice(0, -1);
      } else if (ch == 10) {
        context.AddLine();
        newline = true;
      } else if (ch == 13) {
      } else {
        context.outbuffer.push(ch);
      }
    }
    context.Update();
    if (newline) {
      window.scrollTo(0, document.body.scrollHeight);
    }
    sp += 4; i32[sp>>2] = newline ? -1 : 0;
  }
  return sp;
})
| 2 jseval!
: web-type ( a n -- ) 2 call if yield then ;
' web-type is type

r|
(function(sp) {
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
| 3 jseval!
: web-key ( -- n ) begin yield 3 call dup if exit then drop again ;
' web-key is key

r|
(function(sp) {
  if (globalObj.readline) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  sp += 4; i32[sp>>2] = context.inbuffer.length ? -1 : 0;
  return sp;
})
| 4 jseval!
: web-key? ( -- f ) yield 4 call ;
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
| 5 jseval!
: terminate ( n -- ) 5 call ;

r|
(function(sp) {
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = 0;  // Disable echo.
  } else {
    sp += 4; i32[sp>>2] = -1;  // Enable echo.
  }
  return sp;
})
| 6 jseval! 6 call echo !

r|
(function(sp) {
  var mode = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    return sp;
  }
  context.setMode(mode);
  return sp;
})
| 7 jseval!

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
| 8 jseval!

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
| 9 jseval!

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
| 10 jseval!

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
| 11 jseval!

r|
(function(sp) {
  sp += 4; i32[sp>>2] = context.mobile;
  return sp;
})
| 12 jseval!

r|
(function(sp) {
  sp += 4; i32[sp>>2] = context.KEYBOARD_HEIGHT;
  return sp;
})
| 13 jseval!

forth definitions web

: bye   0 terminate ;
: page   12 emit ;
: gr   1 7 call ;
: text   0 7 call ;
: mobile ( -- f ) 12 call ;
: keys-height ( -- n ) 13 call ;
$ffffff value color
: box ( x y w h -- ) color 8 call ;
: window ( w h -- ) 9 call ;
: viewport@ ( -- w h ) 10 call ;
: show-text ( f -- )
  if
    mobile if 3000 120 keys-height + else 1667 120 then
  else
    mobile if 0 keys-height else 0 0 then
  then 11 call ;

forth definitions
