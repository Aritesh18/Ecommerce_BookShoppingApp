var dataTable;



$(document).ready(function () {
    loadDataTable();
    $('#fromDate').datepicker();
    $('#toDate').datepicker();
    $('#findButton').click(function () {
        loadDataTable();
    });
})

function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [year, month, day].join('-');
}

function loadDataTable() {
    var fromDate = new Date();
    var toDate = new Date();
    if ($('#fromDate').val() == '') {
        var dt = new Date();
        dt.setDate(dt.getDate() - 30);
        fromDate = dt;
    }
    else {
        fromDate = $("#fromDate").datepicker('getDate');
    }

    if ($('#toDate').val() == '') {
        var dt = new Date();
        toDate = dt;
    }
    else {
        toDate = $("#toDate").datepicker('getDate');
    }
    $("#tblData").dataTable().fnDestroy()
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Customer/OrderStatus/AllOrders",
            "type": "POST",
            "serverSide": true,
            "processing": true,
            'data': {
                fromDate: formatDate(fromDate),
                toDate: formatDate(toDate)
            }
        },
        "columns": [
            { "data": "id", "width": "30%" },
            { "data": "orderDate", "width": "30%" },
            { "data": "orderTotal", "width": "20%" },
            { "data": "orderStatus", "width": "30%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href = "/Customer/OrderStatus/Details/${data}" class="btn btn-info rounded-pill shadow")>
                                <i class="fa fa-info-circle" aria-hidden="true"></i>
                                </a>
                            </div>
                    `;
                }
            }

        ]
    })
}