'use strict';

function hookButton(name) {
  document.getElementById(name)
    .onclick = element => sendMessage({ action: name })
}

hookButton('connect');
hookButton('disconnect');

function showText(txt) {
  document.getElementById('text').innerHTML = txt;
}

function initTextAreaFromStorage(textarea, storageName, dataGetter, defaultValue) {
  chrome.storage.sync.get(storageName, data => {
    let txt = dataGetter(data);
    if (!txt) {
      txt = defaultValue;
      saveFormula(txt);
    }
    textarea.value = txt;
  });
}

let textareaFormula = document.getElementById('formula');
initTextAreaFromStorage(
  textareaFormula, 'formula', data => data.formula,
 "Pixel #pixel_name rolled a #face_value");

let button = document.getElementById('save');
button.addEventListener('click', () => saveFormula(textareaFormula.value));

function saveFormula(txt) {
  sendMessage({ action: "setFormula", formula: txt });
  chrome.storage.sync.set({ formula: txt }, () => console.log('Formula stored: ' + txt));
}

function sendMessage(data, responseCallback) {
  chrome.tabs.query({ active: true, currentWindow: true }, tabs =>
    chrome.tabs.sendMessage(tabs[0].id, data, responseCallback));
}

chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.action == "showText")
    showText(request.text);
});

chrome.tabs.query({ active: true, currentWindow: true }, tabs => {
  chrome.tabs.executeScript(
    tabs[0].id,
    { file: "roll20.js" },
    _ => {
      sendMessage({ action: "getStatus" });
      chrome.storage.sync.get('formula', data => sendMessage({ action: "setFormula", formula: data.formula }))
    })
});
