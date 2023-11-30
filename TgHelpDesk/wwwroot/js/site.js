
// Write your JavaScript code.
var tg = window.Telegram.WebApp;

var tokenKey = "accessToken";

let container = document.getElementById("main");
let loadSpinner = document.getElementById("loading-spinner");

//Visual
function spinnerLoading(loading) {
    if (loading == true) {
        if (loadSpinner.classList.contains("visually-hidden") == true) {
            loadSpinner.classList.remove("visually-hidden");
        }
    }
    else {
        if (loadSpinner.classList.contains("visually-hidden") == false) {
            loadSpinner.classList.add("visually-hidden");
        }
    }
}

//ActionOnLoad
document.addEventListener("DOMContentLoaded", function () {
    tg.expand();
    Authorize();
});

////GO TO PAGE
//function goToPage(e) {
//    event.preventDefault();
//    goToPageLink(e.href);
//}
//async function goToPageLink(e) {
//    let response = await fetch(e, {
//        method: "GET",
//        headers: {
//            "Accept": "application/json",
//        }
//    });
//    if (response.ok === true) {
//        let data = await response.text();
//        container.innerHTML = data;
//    }
//    else {
//        console.log("Status: ", response.status);
//    }
//    console.log("goToPageLink done");
//}

//Authorization
async function Authorize() {
    let response = await fetch("/authorization",
        {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                initData: tg.initData,
                tgId: tg.initDataUnsafe.user.id
            })
        }
    );
    if (response.ok === true) {
        let data = await response.json();
        sessionStorage.setItem(tokenKey, data.access_token);
    }
    else {
        console.log("Status: ", response.status);
    }
}


//Actions
async function SendHelpRequest() {
    let token = sessionStorage.getItem(tokenKey);

    let title = document.getElementById("inputTitle").value;
    let description = document.getElementById("inputDescription").value;
    let priority = document.getElementById("inputPriority").value;

    if (title == "") {
        alert("Название заяки не должно быть пустым.");
        return false;
    }
    if (description == "") {
        alert("Описание проблемы не должно быть пустым.");
        return false;
    }

    let response = await fetch("/home/post",
        {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token  // передача токена в заголовке

            },
            body: JSON.stringify({
                Title: title,
                Description: description,
                TgId: tg.initDataUnsafe.user.id,
                Priority: priority
            })
        }
    );
    if (response.ok === true) {
        tg.close();
    }
    else {
        alert("Произошла ошибка");
    }
}

function tagClick(text) {
    let title = document.getElementById("inputTitle");
    if (title.value.includes(text)) {
        title.value = title.value.replace(text+" ", "")
    }
    else {
        title.value = text + " " + title.value;
    }

}