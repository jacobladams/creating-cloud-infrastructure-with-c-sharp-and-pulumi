document.querySelector('button').addEventListener('click', function(e) {
    e.preventDefault();
    for(var i= 01; i<=5; i++) {
        var img = document.createElement('img');
        img.src = `./images/image${i}.jpg`;
        document.body.appendChild(img);
    }
});