// Copyright 2025 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

'use strict';

const MAX_BLOCKS = 16 * 1024;
const SYNC_CHUNK = 64;
const DIRTY = 1;
const LOADED = 2;
var blocks = new Uint8Array(1024 * MAX_BLOCKS);
var flags = new Uint8Array(MAX_BLOCKS);
var clipboard = [];
var screen_history = [];
var keymap = {};
var scr = 0;
var pos = 0;
var marker = 0;
var passwd = '';
var ForthKeyDown = null;
var ForthKeyPress = null;
var forth_added = false;
var ueforth = null;

document.body.style.overflow = 'hidden';
document.body.style.margin = '0';
document.body.style.border = '0';
document.body.style.padding = '0';
var canvas = document.createElement('canvas');
document.body.appendChild(canvas);
var ctx = canvas.getContext('2d');

function SpaceIt() {
  if (!(flags[scr] & LOADED)) {
    return;
  }
  if (blocks[scr * 1024] == 0) {
    for (var i = 0; i < 1024; ++i) {
      blocks[i + scr * 1024] = 32;
    }
    flags[scr] |= DIRTY;
  }
}

function IsEmpty(n) {
  for (var i = 0; i < 1024; i++) {
    var ch = blocks[i + n * 1024];
    if (ch != 32 && ch != 0) {
      return false;
    }
  }
  return true;
}

function FindEnd() {
  var i = pos;
  while (i < 1023 &&
         (blocks[scr * 1024 + i] != 32 ||
          blocks[scr * 1024 + i + 1] != 32)) {
    i++;
  }
  return i;
}

function Insert() {
  var end = FindEnd();
  for (var i = end; i >= pos; i--) {
    blocks[scr * 1024 + i + 1] = blocks[scr * 1024 + i];
  }
  blocks[scr * 1024 + pos] = 32;
  flags[scr] |= DIRTY;
}

function Delete() {
  var end = FindEnd();
  for (var i = pos; i < end; i++) {
    blocks[scr * 1024 + i] = blocks[scr * 1024 + i + 1];
  }
  flags[scr] |= DIRTY;
}

function FindSpan() {
  var i = Math.floor(pos / 64) * 64;
  var min = 63;
  var max = 0;
  for (var j = 0; j < 64; j++) {
    if (blocks[scr * 1024 + i + j] != 32) {
      min = Math.min(min, j);
      max = Math.max(max, j);
    }
  }
  if (min > max) {
    return [i, i + 63];
  }
  return [i + min, i + max];
}

function Home() {
  var span = FindSpan();
  var start = Math.floor(pos / 64) * 64;
  if (pos == span[0]) {
    pos = start;
  } else {
    pos = span[0];
  }
}

function End() {
  var span = FindSpan();
  var end = Math.floor(pos / 64) * 64 + 63;
  var span1 = Math.min(span[1] + 1, end);
  if (pos == span1) {
    pos = end;
  } else {
    pos = span1;
  }
}

function Up() {
  if (pos >= 64) {
    pos -= 64;
  }
}

function Down() {
  if (pos + 64 < 1024) {
    pos += 64;
  }
}

function Left() {
  pos = Math.max(0, pos - 1);
}

function Right() {
  pos = Math.min(1023, pos + 1);
}

function Copy() {
  var row = Math.floor(pos / 64) * 64;
  clipboard.push(blocks.slice(scr * 1024 + row, scr * 1024 + row + 64));
  Up();
}

function Cut() {
  var row = Math.floor(pos / 64) * 64;
  clipboard.push(blocks.slice(scr * 1024 + row, scr * 1024 + row + 64));
  for (var j = 0; j < 64; j++) {
    blocks[scr * 1024 + row + j] = 32;
  }
  flags[scr] |= DIRTY;
  Up();
}

function Paste() {
  if (clipboard.length == 0) {
    return;
  }
  var row = Math.floor(pos / 64) * 64;
  var data = clipboard.pop();
  for (var j = 0; j < 64; j++) {
    blocks[scr * 1024 + row + j] = data[j];
  }
  flags[scr] |= DIRTY;
  Down();
}

function Update() {
  SpaceIt();
  ctx.fillStyle = 'black';
  ctx.fillRect(0, 0, canvas.width, canvas.height);
  ctx.font = '16px consolas, Monaco, monospace';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.save();
  ctx.scale(canvas.width / 64 , canvas.height / 17);
  ctx.fillStyle = '#750';
  if (window.onkeydown === Login) {
    ctx.fillRect(10, 0, 1, 1);
  } else {
    for (var j = 0; j < 16; ++j) {
      for (var i = 0; i < 64; ++i) {
        if (pos === i + j * 64) {
          ctx.fillRect(i, j, 1, 1);
        }
      }
    }
  }
  ctx.save();
  var m = ctx.measureText('W');
  var w = m.width;
  var h = m.fontBoundingBoxAscent + m.fontBoundingBoxDescent;
  ctx.scale(1 / w, 1 / h);
  ctx.fillStyle = '#fb0';
  if (window.onkeydown === Login) {
    ctx.fillText('password:', 0.5 * w * 9, 0.5 * h);
  } else {
    for (var j = 0; j < 16; ++j) {
      for (var i = 0; i < 64; ++i) {
        var ch = String.fromCharCode(blocks[i + j * 64 + scr * 1024]);
        ctx.fillText(ch, (i + 0.5) * w, (j + 0.5) * h);
      }
    }
  }
  ctx.fillStyle = '#750';
  ctx.textAlign = 'right';
  var info = '';
  if (flags[scr] & DIRTY) {
    info += 'D ';
  }
  if (!(flags[scr] & LOADED)) {
    info += 'L ';
  }
  info += scr;
  ctx.fillText(info, 63.5 * w, 16.5 * h);
  ctx.restore();
  ctx.restore();
}

function LoadChunk(n) {
  if ((flags[n * SYNC_CHUNK] & LOADED)) {
    return Promise.resolve();
  } else {
    return LoadBlocks(n * SYNC_CHUNK, (n + 1) * SYNC_CHUNK).then(function() {
      Update();
    });
  }
}

function MaybeLoad() {
  var s = Math.floor(scr / SYNC_CHUNK);
  return LoadChunk(s);
}

function LineString(blk, row) {
  var result = '';
  for (var col = 0; col < 64; col++) {
    result += String.fromCharCode(blocks[blk * 1024 + row * 64 + col]);
  }
  return result;
}

function BlockString(blk, linebreaks) {
  var result = '';
  for (var row = 0; row < 16; row++) {
    result += LineString(blk, row);
    if (linebreaks) {
      result += '\n';
    }
  }
  return result;
}

function Eval(n) {
  eval(BlockString(n, true));
}

function Print() {
  var start = Math.min(marker, scr);
  var end = Math.max(marker, scr);
  var content = '';
  content += '<!DOCTYPE html>\n';
  for (var i = start; i <= end; i++) {
    content += '<pre style="border: 1px solid; display: inline-block;">\n';
    content += BlockString(i, true).replaceAll('<', '&lt;');
    content += '<hr/>' + i;
    content += '</pre><br/>\n';
  }
  var blob = new Blob([content], { type: 'text/html' });
  var url = URL.createObjectURL(blob);
  window.open(url, '_blank');
}

function Backspace() {
  if (pos > 0) {
    --pos;
    Delete();
  }
}

function Goto(n) {
  scr = n;
  MaybeLoad();
}

function Gosub(n) {
  screen_history.push(scr);
  Goto(n);
}

function Adjust(n) {
  Goto(Math.max(0, Math.min(MAX_BLOCKS - 1, scr + n)));
}

function Enter() {
  pos = Math.floor((pos + 64) / 64) * 64;
  if (pos > 1023) {
    pos -= 64;
  }
}

function ShiftUp() {
  pos = (pos % 64);
}

function ShiftDown() {
  pos = (pos % 64) + 15 * 64;
}

function GetLink() {
  var s = BlockString(scr, false);
  var at = s.indexOf('@', pos);
  var paren = s.indexOf(' )', pos);
  if (at >= 0 && (at < paren || paren < 0)) {
    return s.slice(at).split(' ')[0];
  }
  if (paren >= 0) {
    var p2 = s.lastIndexOf('( ', paren);
    if (p2 >= 0) {
      return s.slice(p2, paren + 2);
    }
  }
  return '';
}

function Find(s) {
  for (var i = 0; i < MAX_BLOCKS; i++) {
    var j = (scr + 1 + i) % MAX_BLOCKS;
    if (BlockString(j, false).indexOf(s) >= 0) {
      return j;
    }
  }
  return null;
}

function FollowLink() {
  var link = GetLink();
  if (link.startsWith('@')) {
    var n = parseInt(link.slice(1));
    Gosub(n);
  } else if (link.startsWith('( ')) {
    var n = Find(link.slice(2, -2));
    if (n !== null) {
      Gosub(n);
    }
  }
}

function Type(ch) {
  Insert();
  blocks[pos + scr * 1024] = ch;
  pos = Math.min(1023, pos + 1);
}

function Key(e) {
  if (e.ctrlKey && keymap['^' + e.key]) {
    keymap['^' + e.key](e);
  } else if (e.shiftKey && keymap['+' + e.key]) {
    keymap['+' + e.key](e);
  } else if (keymap[e.key]) {
    keymap[e.key](e);
  } else if (e.key.length == 1 && !e.ctrlKey) {
    Type(e.key.charCodeAt(0));
  } else {
    return true;
  }
  Update();
  e.preventDefault();
  return false;
}

function Login(e) {
  if (e.key == 'Backspace') {
    passwd = passwd.slice(0, -1);
  } else if (e.key == 'Enter') {
    window.onkeydown = Key;
    MaybeLoad().then(function() {
      Eval(63);
    });
  } else if (e.key.length == 1) {
    passwd += e.key;
  }
  Update();
  e.preventDefault();
  return false;
}

function Resize() {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight;
  Update();
}

function SaveBlock(i) {
  var fd = new FormData();
  fd.append('command', 'write');
  fd.append('passwd', passwd);
  fd.append('index', i);
  fd.append('data', new Blob([blocks.slice(i * 1024, (i + 1) * 1024)],
                             {type: "application/octet-stream"}));
  return fetch('/io', {'method': 'POST', body: fd}).then(function() {
    flags[i] &= ~DIRTY;
    Update();
  });
}

function Sync() {
  for (var i = 0; i < MAX_BLOCKS; ++i) {
    if ((flags[i] & LOADED) && (flags[i] & DIRTY)) {
      SaveBlock(i);
    }
  }
}

function FindEmpty() {
  screen_history.push(scr);
  while (!IsEmpty(scr)) {
    Adjust(1);
  }
}

function StampLog() {
  var now = new Date();
  var month = ('0' + (1 + now.getMonth())).slice(-2);
  var day = ('0' + now.getDate()).slice(-2);
  var dt = now.getFullYear() + '-' + month + '-' + day;
  for (var i = 0; i < dt.length; i++) {
    blocks[54 + i + scr * 1024] = dt.charCodeAt(i);
  }
  var mark = 'LOG: ';
  for (var i = 0; i < mark.length; i++) {
    blocks[i + scr * 1024] = mark.charCodeAt(i);
  }
  pos = mark.length;
  flags[scr] |= DIRTY;
}

function Back() {
  if (screen_history.length) {
    Goto(screen_history.pop());
  }
}

function LoadBlocks(start, end) {
  var fd = new FormData();
  fd.append('command', 'read');
  fd.append('passwd', passwd);
  fd.append('start', start);
  fd.append('end', end);
  return fetch('/io', {'method': 'POST', body: fd}).then(function(response) {
    if (!response.ok) {
      throw 'bad fetch';
    }
    return response.arrayBuffer().then(function(data) {
      var u8 = new Uint8Array(data);
      if (u8.length != (end + 1 - start) * 1024) {
        throw 'bad load';
      }
      for (var i = start; i < end; i++) {
        if (flags[i] & LOADED) {
          return;
        }
        var dst = i * 1024;
        var src = (i - start) * 1024;
        for (var j = 0; j < 1024; j++) {
          blocks[j + dst] = u8[j + src];
        }
        flags[i] |= LOADED;
      }
    });
  });
}

function ForthKeyFilter(e) {
  if (e.key == 'f' && e.ctrlKey) {
    ToggleForth();
    e.preventDefault();
    return false;
  }
  return ForthKeyDown(e);
}

function ToggleForth() {
  if (!forth_added) {
    forth_added = true;
    var fscript = document.createElement('script');
    fscript.src = 'myforth.fs';
    fscript.type = 'text/forth';
    document.body.appendChild(fscript);
    var script = document.createElement('script');
    script.src = 'https://eforth.appspot.com/ueforth.js';
    document.body.appendChild(script);
    function Loader() {
      if (ueforth !== null) {
        ueforth.Start();
        canvas.style.display = 'none';
        setTimeout(function() {
          ForthKeyDown = window.onkeydown;
          ForthKeyPress = window.onkeypress;
          window.onkeydown = ForthKeyFilter;
        }, 500);
      } else {
        setTimeout(Loader, 10);
      }
    }
    Loader();
    return;
  }
  if (window.onkeydown === Key) {
    window.onkeydown = ForthKeyFilter;
    window.onkeypress = ForthKeyPress;
    canvas.style.display = 'none';
    ueforth.screen.style.display = '';
  } else {
    window.onkeydown = Key;
    window.onkeypress = null;
    ueforth.screen.style.display = 'none';
    canvas.style.display = '';
    Resize();
  }
}

function Init() {
  keymap['Delete'] = Delete;
  keymap['Backspace'] = Backspace;
  keymap['PageUp'] = function() { Adjust(-1); };
  keymap['PageDown'] = function() { Adjust(1); };
  keymap['+PageUp'] = function() { Adjust(-16); };
  keymap['+PageDown'] = function() { Adjust(16); };
  keymap['Home'] = Home;
  keymap['End'] = End;
  keymap['Enter'] = Enter;
  keymap['^Enter'] = FollowLink;
  keymap['ArrowUp'] = Up;
  keymap['ArrowDown'] = Down;
  keymap['ArrowLeft'] = Left;
  keymap['ArrowRight'] = Right;
  keymap['+ArrowUp'] = ShiftUp;
  keymap['+ArrowDown'] = ShiftDown;
  keymap['+ArrowLeft'] = Home;
  keymap['+ArrowRight'] = End;
  keymap['^c'] = Copy;
  keymap['^x'] = Cut;
  keymap['^v'] = Paste;
  keymap['^m'] = function() { marker = scr; };
  keymap['^p'] = Print;
  keymap['^g'] = function() { Eval(scr); };
  keymap['^o'] = FindEmpty;
  keymap['^l'] = StampLog;
  keymap['^b'] = Back;
  keymap['^f'] = ToggleForth;
  keymap['^h'] = function() { Gosub(0); };

  window.addEventListener('resize', Resize);
  window.onkeydown = Login;
  Resize();
}
Init();

setInterval(Sync, 3000);
