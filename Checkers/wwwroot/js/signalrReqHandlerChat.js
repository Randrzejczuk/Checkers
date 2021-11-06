var connection = new signalR.HubConnectionBuilder()
    .withUrl('/ChatHub')
    .build();

connection.on('receiveMessage', receiveMessage);

connection.start()
    .catch(error => {
        console.error(error.message);
    });

