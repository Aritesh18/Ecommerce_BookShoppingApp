var DataTable;

    $(document).ready(function () {

        loadDataTable();
    })

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "description", "width": "35%" },
            { "data": "author", "width": "15%" },
            { "data": "isbn", "width": "10%" },
            { "data": "price", "width": "10%" },
            {
                "data": "imageUrl",
                "render": function (data) {
                    return `
                            <div>
                         <img src="${data}" class="rounded" height="100" width="80"/>
                             </div>
                               `;
                }
            },


            {
                "data": "id",
                "render": function (data) {
                    return `
                       <div class="text-center">
                       <a href="/Admin/Product/Upsert/${data}" class="btn btn-info">
                       <i class="fas fa-edit"></i>
                       </a>
                       <a class="btn btn-danger" onclick=Delete("/Admin/Product/Delete/${data}")>
                       <i class="fas fa-trash-alt"></i>
                       </a>
                       </div>
                    `;
                }
            }
        ]
    })
}
function Delete(url) {
//    alert(url);
    swal({
        title: "Want to delete data?",
        text: "Delete Information!!!",
        icon: "warning",
        dangerModel: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    
                    }
                }

            })
        }

    })
}