var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Customer/OrderStatus/PendingOrders"
        },
        "columns": [
            { "data": "id", "width": "30%" },
            {
                "data": "orderDate", "width": "30%"
            },
            { "data": "orderTotal", "width": "20%" },
            { "data": "orderStatus", "width": "30%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                            <a href="/Customer/OrderStatus/Details/${data}" class="btn btn-info">
                            Detail
                            </a>                        
                           `;
                }
            }
        ]
    })
}