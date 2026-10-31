(function () {
  "use strict";

  var ME = null;
  var lastId = 0;
  var pollTimer = null;

  var chatEl = document.getElementById("chat");
  var inpEl = document.getElementById("inp");
  var whoEl = document.getElementById("who");
  var emojiEl = document.getElementById("emoji");
  var dropEl = document.getElementById("drop");

  function api(path, body) {
    return fetch(path, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body || {})
    }).then(function (r) { return r.json(); });
  }

  function esc(s) {
    return String(s).replace(/[&<>"']/g, function (c) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c];
    });
  }

  function hhmm(ts) {
    var d = new Date((ts || Date.now() / 1000) * 1000);
    return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  }

  function render(m) {
    var div = document.createElement("div");

    if (m.type === "system") {
      div.className = "sys";
      div.textContent = m.content;
      chatEl.appendChild(div);
      return;
    }

    div.className = "msg " + (m.user === ME ? "own" : "other");

    var meta = document.createElement("div");
    meta.className = "meta";
    meta.textContent = m.user + " \u00b7 " + hhmm(m.ts);
    div.appendChild(meta);

    if (m.type === "text") {
      var p = document.createElement("div");
      p.textContent = m.content;
      div.appendChild(p);
    } else if (m.type === "file") {
      var isImg = (m.mime || "").indexOf("image/") === 0;
      if (isImg) {
        var img = document.createElement("img");
        img.src = "data:" + m.mime + ";base64," + m.data;
        img.alt = m.name;
        div.appendChild(img);
      }
      var a = document.createElement("a");
      a.className = "dl";
      a.textContent = (isImg ? "\u2b07\ufe0f " : "\ud83d\udcce ") + m.name;
      a.href = "data:" + (m.mime || "application/octet-stream") + ";base64," + m.data;
      a.download = m.name;
      div.appendChild(a);
    }

    chatEl.appendChild(div);
  }

  function scrollDown() {
    chatEl.scrollTop = chatEl.scrollHeight;
  }

  function poll() {
    fetch("/poll?since=" + lastId + "&user=" + encodeURIComponent(ME || ""))
      .then(function (r) { return r.json(); })
      .then(function (data) {
        (data.messages || []).forEach(function (m) {
          render(m);
          if (m.id > lastId) lastId = m.id;
        });
        if ((data.messages || []).length) scrollDown();
        whoEl.textContent = "\ud83d\udfe2 " + (data.users || []).length + " online";
        pollTimer = setTimeout(poll, 900);
      })
      .catch(function () {
        whoEl.textContent = "\u26a0\ufe0f disconnesso, riprovo...";
        pollTimer = setTimeout(poll, 2500);
      });
  }

  function sendText() {
    var text = inpEl.value.trim();
    if (!text) return;
    inpEl.value = "";
    api("/send", { user: ME, type: "text", content: text }).then(function () {
      poll();
    });
  }

  function sendFile(file) {
    if (file.size > 50 * 1024 * 1024) {
      alert("File troppo grande (max 50 MB): " + file.name);
      return;
    }
    var reader = new FileReader();
    reader.onload = function (ev) {
      var b64 = String(ev.target.result).split(",")[1] || "";
      api("/send", {
        user: ME,
        type: "file",
        name: file.name,
        mime: file.type || "application/octet-stream",
        data: b64
      }).then(function () { poll(); });
    };
    reader.readAsDataURL(file);
  }

  var EMOJIS = ["\ud83d\ude00","\ud83d\ude02","\ud83d\ude0d","\ud83d\ude0e","\ud83d\udc4d","\u2764\ufe0f","\ud83d\udd25","\ud83c\udf89","\ud83d\udcaf","\ud83d\udc4b","\ud83e\udd14","\ud83d\ude22","\ud83d\ude21","\ud83d\udc40","\ud83d\ude80","\ud83d\udca1","\u2705","\u274c","\u26a1","\ud83c\udf1f","\ud83c\udfb5","\ud83d\udcf7","\ud83d\udcc1","\ud83d\udd12","\ud83c\udf0d","\u2600\ufe0f","\ud83c\udf19","\ud83c\udf55","\ud83c\udf7a","\ud83c\udfae","\ud83c\udfc6","\ud83d\udcbb"];

  function buildEmoji() {
    EMOJIS.forEach(function (e) {
      var b = document.createElement("button");
      b.className = "eb";
      b.type = "button";
      b.textContent = e;
      b.onclick = function () {
        inpEl.value += e;
        inpEl.focus();
        emojiEl.style.display = "none";
      };
      emojiEl.appendChild(b);
    });
  }

  function bind() {
    document.getElementById("sendb").onclick = sendText;
    inpEl.addEventListener("keydown", function (e) {
      if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendText(); }
    });

    document.getElementById("ebtn").onclick = function (e) {
      e.stopPropagation();
      emojiEl.style.display = emojiEl.style.display === "grid" ? "none" : "grid";
    };
    document.addEventListener("click", function (e) {
      if (!emojiEl.contains(e.target)) emojiEl.style.display = "none";
    });

    var fsel = document.getElementById("fsel");
    document.getElementById("fbtn").onclick = function () { fsel.click(); };
    fsel.onchange = function () {
      Array.prototype.forEach.call(fsel.files, sendFile);
      fsel.value = "";
    };

    var bgc = document.getElementById("bgc");
    var fgc = document.getElementById("fgc");
    bgc.oninput = function () {
      document.documentElement.style.setProperty("--bg", bgc.value);
      localStorage.setItem("chathub_bg", bgc.value);
    };
    fgc.oninput = function () {
      document.documentElement.style.setProperty("--fg", fgc.value);
      localStorage.setItem("chathub_fg", fgc.value);
    };
    var sb = localStorage.getItem("chathub_bg");
    var sf = localStorage.getItem("chathub_fg");
    if (sb) { bgc.value = sb; document.documentElement.style.setProperty("--bg", sb); }
    if (sf) { fgc.value = sf; document.documentElement.style.setProperty("--fg", sf); }

    var dragDepth = 0;
    window.addEventListener("dragenter", function (e) {
      e.preventDefault(); dragDepth++; dropEl.style.display = "flex";
    });
    window.addEventListener("dragover", function (e) { e.preventDefault(); });
    window.addEventListener("dragleave", function (e) {
      e.preventDefault(); dragDepth--;
      if (dragDepth <= 0) { dragDepth = 0; dropEl.style.display = "none"; }
    });
    window.addEventListener("drop", function (e) {
      e.preventDefault(); dragDepth = 0; dropEl.style.display = "none";
      if (e.dataTransfer && e.dataTransfer.files) {
        Array.prototype.forEach.call(e.dataTransfer.files, sendFile);
      }
    });

    window.addEventListener("beforeunload", function () {
      navigator.sendBeacon("/leave", JSON.stringify({ user: ME }));
    });
  }

  function boot() {
    var nick = prompt("Il tuo nickname:", localStorage.getItem("chathub_nick") || "");
    if (nick === null) { document.body.innerHTML = "<p style='padding:20px'>Chat annullata.</p>"; return; }
    nick = (nick || "").trim().slice(0, 24) || ("Utente" + Math.floor(Math.random() * 900 + 100));
    localStorage.setItem("chathub_nick", nick);

    api("/join", { user: nick }).then(function (res) {
      ME = res.user || nick;
      document.title = "Chat Hub - " + ME;
      buildEmoji();
      bind();
      poll();
    }).catch(function () {
      document.body.innerHTML = "<p style='padding:20px'>Server non raggiungibile.</p>";
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
