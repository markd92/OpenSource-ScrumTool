(function (window, $, undefined) {
    //Ready
    $(function () {
        $("#myModal").on("show.bs.modal", function () {
            function checkAndLockTimeRemaining() {
                if ($("#State").val() === "2") { //2 is Done
                    $("#TimeRemaining").prop("disabled", true).val(0);
                } else {
                    $("#TimeRemaining").prop("disabled", false);
                }
            }

            checkAndLockTimeRemaining();
            //Prevent multiple binds
            if ($("#State").data("disableTimeRemaining") === undefined) {
                $("#State").data("disableTimeRemaining", true);
                $("#State").change(checkAndLockTimeRemaining);
            }
        });
    });
})(window, jQuery.noConflict());