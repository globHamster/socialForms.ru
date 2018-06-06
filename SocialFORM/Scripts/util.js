$(function () {

    $('#chatBody').hide();
    $('.blink').hide();
    // Ссылка на автоматически-сгенерированный прокси хаба
    var chat = $.connection.chatHub;
    // Объявление функции, которая хаб вызывает при получении сообщений
    chat.client.addMessage = function (name, message) {
        // Добавление сообщений на веб-страницу 
        $('#chatroom').append('<p><b>' + htmlEncode(name)
            + '</b>: ' + htmlEncode(message) + '</p>');
        $('.blink[name="Operator"]').show();
        $(function () {
            var flag = false;
            $('.blink[name="Operator"]').css("color", "black");
            setTimeout(function () {
                $('.blink[name="Operator"]').css("color", "black");
                setInterval(function () {
                    $('.blink[name="Operator"]').css("color", flag ? "black" : "red");
                    flag = !flag;
                }, 500)
            }, 1000);
        });
    };

    // Функция, вызываемая при подключении нового пользователя
    chat.client.onConnected = function (id, userName, allUsers) {

        $('#chatBody').show();
        // установка в скрытых полях имени и id текущего пользователя
        $('#hdId').val(id);
        $('#username').val(userName);

    }

    //// Добавляем нового пользователя
    //chat.client.onNewUserConnected = function (id, name) {

    //    AddUser(id, name);
    //}

    //// Удаляем пользователя
    //chat.client.onUserDisconnected = function (id, userName) {

    //    $('#' + id).remove();
    //}

    // Открываем соединение
    $.connection.hub.start().done(function () {

        $('#sendmessage').click(function () {
            // Вызываем у хаба метод Send
            chat.server.send($('#username').val(), $('#message').val());
            $('#message').val('');
        });

        // обработка логина
        $(".btnLogin").click(function () {

            var name = $('div.operatorID').text();
            $('.blink').hide();
            if (name.length > 0) {
                chat.server.connect(name);
            }
            else {
                alert("Введите имя");
            }
        });
    });
});
// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
