//Sends message to hub
function sendMessage(user, room) {
    connection.invoke('SendMessage', user,room);
}
//Clears text field
function clearInputField() {
    textInput = document.getElementById('messageText');
    textInput.value = "";
}
//Adds messgae to chat
function receiveMessage(message) {
    let messages = document.getElementsByClassName("msg");
    let lastElemnt = messages.length - 1;
    if (messages[lastElemnt].innerHTML == "") {
        for (let i = messages.length - 1; i >= 0; i--) {
            if (messages[i].innerHTML == "") {
                lastElemnt = i;
            }
            else {
                break;
            }
        }
    }
    else {
        for (let i = 0; i < (messages.length - 1); i++) {
            messages[i].innerHTML = messages[i + 1].innerHTML;
        }
    }
    messages[lastElemnt].innerHTML = message;
}
