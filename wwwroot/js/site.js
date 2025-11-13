function CheckScreeningData(id)
{
    // Показываем модальное окно (с анимацией загрузки)
    var modal = new bootstrap.Modal(document.getElementById('screeningModal'));
    document.getElementById('screeningModalBody').innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-light" role="status">
                <span class="visually-hidden">Загрузка...</span>
            </div>
        </div>`;
    modal.show();

    // AJAX-запрос на сервер
    $.ajax({
        url: "PartialCheckScreeningData",
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#screeningModalBody').html(result);
            initializeHallCanvas();
        },
        error: function (xhr, status, error) {
            $('#screeningModalBody').html('<div class="alert alert-danger">Ошибка загрузки данных: ' + error + '</div>');
        }
    });
}

function initializeHallCanvas() {
    var modalBody = document.getElementById('screeningModalBody');
    if (!modalBody) {
        return;
    }

    var canvas = modalBody.querySelector('#hallCanvas');
    if (!canvas) {
        return;
    }

    var hallJson = canvas.getAttribute('data-hall');
    if (!hallJson) {
        return;
    }

    var hallData;
    try {
        hallData = JSON.parse(hallJson);
    } catch (error) {
        console.error('Ошибка разбора данных зала:', error);
        return;
    }

    var ctx = canvas.getContext('2d');
    if (!ctx) {
        return;
    }

    var seatSize = 25;
    var seatGap = 10;
    var scale = 1;
    var offsetX = 0;
    var offsetY = 0;
    var isDragging = false;
    var dragStart = { x: 0, y: 0 };
    var lastTouchDist = 0;

    function drawHall() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.save();
        ctx.translate(offsetX, offsetY);
        ctx.scale(scale, scale);

        var startY = 50;
        hallData.forEach(function (row) {
            var startX = 50;

            row.Places.forEach(function (place) {
                var color = place.IsReserved ? "#6c757d" :
                    place.VIP ? "#ffc107" :
                        "#198754";

                ctx.fillStyle = color;
                ctx.fillRect(startX, startY, seatSize, seatSize);

                ctx.strokeStyle = "#343a40";
                ctx.lineWidth = 1;
                ctx.strokeRect(startX, startY, seatSize, seatSize);

                ctx.fillStyle = place.VIP ? "#000" : "#fff";
                ctx.font = "12px sans-serif";
                ctx.textAlign = "center";
                ctx.textBaseline = "middle";
                ctx.fillText(place.PlaceNumber, startX + seatSize / 2, startY + seatSize / 2);

                startX += seatSize + seatGap;
            });

            ctx.fillStyle = "#adb5bd";
            ctx.font = "bold 14px sans-serif";
            ctx.textAlign = "right";
            ctx.fillText(row.RowNumber, 40, startY + seatSize / 2);

            startY += seatSize + seatGap;
        });

        ctx.restore();
    }

    canvas.addEventListener("wheel", function (e) {
        e.preventDefault();
        var delta = e.deltaY > 0 ? 0.9 : 1.1;
        scale *= delta;
        scale = Math.min(Math.max(scale, 0.5), 3);
        drawHall();
    }, { passive: false });

    canvas.addEventListener("mousedown", function (e) {
        isDragging = true;
        dragStart.x = e.offsetX - offsetX;
        dragStart.y = e.offsetY - offsetY;
    });

    canvas.addEventListener("mouseup", function () { isDragging = false; });
    canvas.addEventListener("mouseleave", function () { isDragging = false; });
    canvas.addEventListener("mousemove", function (e) {
        if (!isDragging) {
            return;
        }
        offsetX = e.offsetX - dragStart.x;
        offsetY = e.offsetY - dragStart.y;
        drawHall();
    });

    canvas.addEventListener("touchstart", function (e) {
        if (e.touches.length === 2) {
            lastTouchDist = getTouchDist(e.touches);
        } else if (e.touches.length === 1) {
            isDragging = true;
            dragStart.x = e.touches[0].clientX - offsetX;
            dragStart.y = e.touches[0].clientY - offsetY;
        }
    }, { passive: false });

    canvas.addEventListener("touchmove", function (e) {
        e.preventDefault();
        if (e.touches.length === 2) {
            var dist = getTouchDist(e.touches);
            if (lastTouchDist) {
                var delta = dist / lastTouchDist;
                scale *= delta;
                scale = Math.min(Math.max(scale, 0.5), 3);
                drawHall();
            }
            lastTouchDist = dist;
        } else if (e.touches.length === 1 && isDragging) {
            offsetX = e.touches[0].clientX - dragStart.x;
            offsetY = e.touches[0].clientY - dragStart.y;
            drawHall();
        }
    }, { passive: false });

    canvas.addEventListener("touchend", function () {
        isDragging = false;
        lastTouchDist = 0;
    });

    function getTouchDist(touches) {
        var dx = touches[0].clientX - touches[1].clientX;
        var dy = touches[0].clientY - touches[1].clientY;
        return Math.sqrt(dx * dx + dy * dy);
    }

    drawHall();
}