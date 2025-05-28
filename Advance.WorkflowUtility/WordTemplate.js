ExecuteWordMerge = function (wordtemplateid, entitytypecodeint, ids, templatetype, fieldforfilename, filenameoverride) {
    try {
        Xrm.Page.ui.clearFormNotification("worderror");
        var funcpath = Xrm.Page.context.getClientUrl() + "/_grid/print/print_data.aspx";
        if (typeof ids !== "object") {
            var tids = ids;
            ids = new Array();
            ids.push(tids);
        }
        var wordTemplateId = wordtemplateid;//"f1f7b994-543b-e711-8106-c4346bac2908" test data;
        var currentEntityTypeCode = entitytypecodeint;//"10063" test data;
        var templateType = (templatetype || 9940); //9940 is global and 9941 is personal
        var fieldForFileName = (fieldforfilename || "");
        var formdata = "exportType=MergeWordTemplate&selectedRecords=" + encodeURIComponent(JSON.stringify(ids)) +
            "&associatedentitytypecode=" + currentEntityTypeCode + "&TemplateId=" + wordTemplateId + "&TemplateType=" + templateType;
        var req = new XMLHttpRequest();
        req.open("POST", funcpath, true);
        req.responseType = "arraybuffer";
        req.setRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
        req.setRequestHeader("Accept-Language", "en-US,en;q=0.8");
        req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        req.onreadystatechange = function () {
            if (this.readyState == 4) {/* complete */
                req.onreadystatechange = null;
                if (this.status >= 200 && this.status <= 299) {//200 range okay
                    var mimetype = (2 === 2) ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var blob = new Blob([req.response], { type: mimetype });
                    var fileNameTemplate = req.getResponseHeader('content-disposition').split('filename=')[1].replace(/'/g, "");
                    var dloadurl = URL.createObjectURL(blob);
                    var filename = (fieldForFileName !== "" && Xrm.Page.getAttribute(fieldForFileName) !== null && Xrm.Page.getAttribute(fieldForFileName).getValue() !== "") ?
                        Xrm.Page.getAttribute(fieldForFileName).getValue() : fileNameTemplate;
                    filename = filenameoverride || filename;
                    //new code, prevent IE errors
                    if (navigator.msSaveOrOpenBlob) {
                        navigator.msSaveOrOpenBlob(blob, filename);
                        return;
                    }
                    else if (window.navigator.msSaveBlob) { // for IE browser
                        window.navigator.msSaveBlob(blob, filename);
                        return;
                    }
                    var a = document.createElement("a");
                    document.body.appendChild(a);
                    a.style = "display: none";
                    a.href = dloadurl;
                    a.download = filename;
                    a.click();
                    URL.revokeObjectURL(dloadurl);
                    //window.location = dloadurl;//we can use just this instead of creating an anchor but we don't get to the name the file
                }
                else {
                    Xrm.Page.ui.setFormNotification("An Error occurred generating the word document, please contact support if the issue persists,code: " + this.status, "ERROR", "worderror");
                }
            }
        };
        req.send(formdata);
    }
    catch (err) {
        Xrm.Page.ui.setFormNotification("An Error occurred generating the word document, please contact support if the issue persists. " + err.message, "ERROR", "worderror");
    }

}