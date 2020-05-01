
function onInit() {
    let connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").withAutomaticReconnect().build();

    connection.on("ReceiveMessage", (user, message) => {
        console.log(`received: ${user} - ${message}`);

        if (user == "done") {
            $("#pLoading").text("");
            message = `<b>${message}</b>`;
        }

        let old = $("#messages").html();
        $("#messages").html(`${old} <br />${message}`)
    });

    connection.start().then(() => {
        console.log("SignalR connected!");
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

function purchase() {
    $("#messages").text("");
    $("#pLoading").text("Loading...");


    var hotel = $("#hotel").val();
    var flight = $("#flight").val();
    var car = $("#car").val();

    fetch('/Booking/book', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ hotel, flight, car })
    }).then(() => {
        openModal();
    });
}

function openModal() {
    $("#myModal").modal();
}

onInit();