function OrderCondition(rowData, userLCID) {
    debugger;
    if (rowData == null || rowData == 'undefined') return;
    var str = JSON.parse(rowData);
    var coldata = str.statecode_Value;
    //get row/record guid
    var rowId = str.RowId;
    if (coldata == null || coldata == 'undefined' || coldata.length < 1) return;

    switch (coldata) {
    case "Active": // active
        $('span:contains("Active")').closest('tr[oid="' + rowId + '"]').css('background-color', '#ff7700');
        break;
    case "Invoiced": //Invoiced
        $('span:contains("Invoiced")').closest('tr[oid="' + rowId + '"]').css('background-color', '#11ff00');
        break;
    default:
        break;
    }
}