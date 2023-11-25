﻿var loader = document.querySelector(".loader-container"); // use the class name

document.getElementById('file-input').addEventListener('change', function (e) {
    var file = e.target.files[0];
    var reader = new FileReader();
    reader.onloadend = function () {
        var img = document.getElementById('preview-image')
        img.src = reader.result;
        setTimeout(function () {
            document.getElementById('imageSize').textContent = 'The image size is (' + img.naturalWidth + 'X' + img.naturalHeight + ')'
        }, 10)
    }

    reader.readAsDataURL(file);
});

document.getElementById('form').addEventListener('submit', function (e) {
    e.preventDefault();

    var file = document.getElementById('file-input').files[0];
    var width = Number(document.getElementById('width').value);
    var height = Number(document.getElementById('height').value);

    var reader = new FileReader();
    reader.onloadend = function () {
        var base64String = reader.result.replace("data:", "")
            .replace(/^.+,/, "");

        loader.style.display = "flex"; // show the loader before sending the request

        fetch('http://127.0.0.1:5100', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Picture:
                {
                    Base64Content: base64String,
                    Name: "test"
                },
                WindowWidth: width,
                WindowHeight: height
            })
        })
            .then(response => response.json())
            .then(data => {
                document.getElementById('result-image').src = 'data:image/png;base64,' + data.HeatMapBase64;
                loader.style.display = "none"; // hide the loader after receiving the response
            });
    }
    reader.readAsDataURL(file);
});
