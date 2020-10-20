// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

'use strict';

// chrome.storage.sync.get('text', function(data) {
//   showText(data);
// });

function linkButton(name) {
  document.getElementById(name)
    .onclick = function (element) {
      sendMessage({ action: name })
    }
}

function showText(txt) {
  document.getElementById('text').innerHTML = txt;
}

linkButton('connect');
linkButton('disconnect');

let textarea = document.getElementById('formula');
chrome.storage.sync.get('formula', function (data) {
  let txt = data.formula;
  if (!txt) {
    txt = "!power {{--name|Pixel Roll --Strength Check|[[ @ + [[ 4 ]] ]]}}";
    saveFormula(txt);
  }
  textarea.value = txt;
});

let button = document.getElementById('save');
button.addEventListener('click', function () {
  saveFormula(textarea.value);
});

function saveFormula(txt) {
  sendMessage({ action: "setFormula", formula: txt });
  chrome.storage.sync.set({ formula: txt }, function () {
    console.log('Formula stored: ' + txt);
  });
}

function sendMessage(data, responseCallback) {
  chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
    chrome.tabs.sendMessage(tabs[0].id, data, responseCallback);
  });
}

chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
  if (request.action == "showText")
    showText(request.text);
});

chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
  chrome.tabs.executeScript(
    tabs[0].id,
    { file: "roll20.js" },
    function (whatIsThat) {
      sendMessage({ action: "getStatus" });
      chrome.storage.sync.get('formula', function (data) {
        sendMessage({ action: "setFormula", formula: data.formula });
      })
    })
});
