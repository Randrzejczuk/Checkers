/*import { start } from "@popperjs/core";*/
class Move
{
    constructor(startX, startY, targetX, targetY)
    {
        this.startX = parseInt(startX);
        this.startY = parseInt(startY);
        this.targetX = targetX;
        this.targetY = targetY;
        this.isvalid = false;
        this.destroyX = null;
        this.destroyY = null;
    }
}
//Shows fields, that selected piece can go to, or redirects to move submition process 
function showAvailable(x, y, model, user) {

    let myId = (x).toString() + y.toString();
    //Gets selected field
    let myField = document.getElementById(myId);
    //If chosen available field submits move
    if (myField.className == "tile-available") {
        submit(x, y, model, user);
    }
    else {
        resetSelection();
        //Gets color and type of checker piece
        let checker = myField.getAttribute("Name");
        //Changes color of selected field
        myField.className = "tile-grey";
        // Chooses and highlights available fields if checkerpiece is black or queen of any color
        if (checker == "b" || checker == "B" || checker == "W") {
            let result = changeColor(x + 1, y + 1)
            if (result != 1 && result != checker.toLowerCase())
                changeColor(x + 2, y + 2);
            result = changeColor(x - 1, y + 1)
            if (result != 1 && result != checker.toLowerCase())
                changeColor(x - 2, y + 2);
        }
        // Chooses and highlights available fields if checkerpiece is white or queen of any color
        if (checker == "w" || checker == "B" || checker == "W") {
            let result = changeColor(x + 1, y - 1)
            if (result != 1 && result != checker.toLowerCase())
                changeColor(x + 2, y - 2);
            result = changeColor(x - 1, y - 1)
            if (result != 1 && result != checker.toLowerCase())
                changeColor(x - 2, y - 2);
        }
    }
}
//Submits move
function submit(x, y, model, user)
{
    let fields = document.getElementsByClassName("tile-grey");
    let field = fields.item(0);
    let startX = field.id.substring(0, 1);
    let startY = field.id.substring(1, 2);
    let move = new Move(startX, startY, x, y);
    submitMove(move, model, user);
}
//Sends request to hub to realize move
function submitMove(move, model, user) {
    connection.invoke('SubmitMove', move, model, user);
}
//If move is valid realizes movement, otherwise, displays error message
function realizeMovement(move, message)
{
    if (move.isvalid) {
        let start = document.getElementById(move.startX.toString() + move.startY.toString());
        let target = document.getElementById(move.targetX.toString() + move.targetY.toString());
        target.innerHTML = start.innerHTML;
        target.setAttribute("Name", start.getAttribute("Name"));
        start.innerHTML = null;
        start.setAttribute("Name", "e");
        if (move.targetY == 8 && target.getAttribute("Name") == "b") {
            target.setAttribute("Name", "B");
            target.innerHTML = target.innerHTML.replace("Black", "Black Q");
        }
        else if (move.targetY == 1 && target.getAttribute("Name") == "w") {
            target.setAttribute("Name", "W");
            target.innerHTML = target.innerHTML.replace("White", "White Q");
        }
        if (move.destroyX != null) {
            let destroy = document.getElementById(move.destroyX.toString() + move.destroyY.toString());
            destroy.innerHTML = null;
            destroy.setAttribute("Name", "e");
        }
        if (message != "") {
            alert(message);
            window.location.reload(true);
        }
    }
    else
    {
    alert(message);
    }
    resetSelection();
}
//Changes color of field to show it as available
function changeColor(x, y)
{
    let field = document.getElementById((x).toString() + y.toString());
    if (field != null) {
        if (field.getAttribute("Name") != "e")
            return field.getAttribute("Name").toLowerCase();
        field.className = "tile-available";
        return 1;
    }
}
//Resets selected field of the board
function resetSelection()
{
    let fields = document.getElementsByClassName("tile-available");
    while (fields.length > 0) {
        let field = fields.item(0);
        field.className = "tile-white";
    }
    fields = document.getElementsByClassName("tile-grey");
    while (fields.length > 0) {
        let field = fields.item(0);
        field.className = "tile-white";
    }
}
//Refreshes browser so both players have initial board state
function refresh()
{
    window.location.reload(true);
}
//Updates remaigning time for players
function updateTime(time1, time2) {
    timers1 = document.getElementsByName("p1Time");
    timers2 = document.getElementsByName("p2Time");
    timer1 = timers1[0]; 
    timer2 = timers2[0];
    timer1.innerHTML = time1;
    timer2.innerHTML = time2;
}
//Displays alert message and changes start/surrender buttons
function gameOver(message,userId) {
    alert(message);
    /*startButton = document.getElementById("Start");
    surrButton = document.getElementById("Surr");
    startButton.hidden = false;
    surrButton.hidden = true;
    if (userId == null)*/
        refresh();
}
//Sends to hub command to start game
function startButtonPressed(model) {
    connection.invoke('StartGame', model);
}
//Sends to hub informaion that one of the players has surrendered
function surrenderButtonPressed(model, user) {
    connection.invoke('SurrenderGame', model, user);
}