//declare global variables
var rowsSelected = 0;

//change the background color of a row when selected and
//also count how many rows are selected
function colorRow(srcElement) {
    var cb = event.srcElement;
    var curElement = cb;
    while (curElement && !(curElement.tagName == "TR")) {
        curElement = curElement.parentElement;
    }
    if (!(curElement == cb) && (cb.name != "cbxSelectAll")) {
        if (cb.checked) {
            curElement.style.backgroundColor = "#848484";
            rowsSelected = rowsSelected + 1;
            curElement.removeAttribute("onmouseover");
            curElement.removeAttribute("onmouseout");
            curElement.removeAttribute("onclick");
        }
        else {
            curElement.style.backgroundColor = "#ffffff";
            rowsSelected = rowsSelected - 1;
            curElement.setAttribute("onmouseover", "highlight(this, true);");
            curElement.setAttribute("onmouseout", "highlight(this, false);");
            curElement.setAttribute("onclick", "link(this, true);");
        }
    }
}

//On Mouse Over function
function highlight(tableRow, active) {
    if (active) {
        tableRow.style.backgroundColor = '#f0f0f0';
    }
    else{
        tableRow.style.backgroundColor = '#fff';
    }
}