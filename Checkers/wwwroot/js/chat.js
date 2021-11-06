
function clearInputField() {
    textInput = document.getElementById('messageText');
    textInput.value = "";
}
//
function receiveMessage(message) {
    let messages = document.getElementsByClassName("msg");
    for (let i = 0; i < messages.count() - 1; i++)
    {
        messages[i].innerHTML = messages[i + 1].innerHTML;
    }
    messages[messages.count() - 1].innerHTML = message;
}
