var myMap;
var geolocation;
// Дождёмся загрузки API и готовности DOM.
ymaps.ready(init);

function init () {
    // Создание экземпляра карты и его привязка к контейнеру с
    // заданным id ("map").
    geolocation = ymaps.geolocation
   
    geolocation.get({
        provider: 'browser',
        mapStateAutoApply: true
    }).then(function (result) {

        result.geoObjects.options.set('present', 'island#blueCircleIcon');
        alert(result.geoObjects.get(0).geometry.getCoordinates());
        

    })


    document.getElementById('destroyButton').onclick = function () {
        // Для уничтожения используется метод destroy.
        myMap.destroy();
    };

}