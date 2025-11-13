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
        },
        error: function (xhr, status, error) {
            $('#screeningModalBody').html('<div class="alert alert-danger">Ошибка загрузки данных: ' + error + '</div>');
        }
    });
}