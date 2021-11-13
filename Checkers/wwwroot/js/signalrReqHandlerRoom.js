var connection = new signalR.HubConnectionBuilder()
    .withUrl('/RoomHub')
    .build();

connection.on('realizeMovement', realizeMovement);
connection.on('refresh', refresh);
connection.on('updateTime', updateTime);
connection.on('gameOver', gameOver);
connection.on('receiveMessage', receiveMessage);
