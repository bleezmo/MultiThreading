// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
    var entries = [];
    getData();
    //getData(); //uncomment to see js 'race condition'
    function getData() {
        entries = [];
        $.get("/home/getdata", function (data) {
            for (var i = 0; i < data.length; i++) {
                entries.push(data[i]);
                printData(entries);
            }
        });
    }
    function printData(entries) {
        var s = "";
        for (var i = 0; i < entries.length; i++) {
            s = s + "<p>" + entries[i] + "</p>";
        }
        $("#entries").html(s);
    }
})();
