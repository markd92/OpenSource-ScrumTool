(function (window, $, undefined) {

    //Ready
    $(function () {
        $.ajaxSetup({ cache: false });

        bindModalButtons(document);

        //Bind open
        $("#myModal").on("show.bs.modal", function () {
            bindForm(this);
        });
    });

    window.bindModalButtons = function (context) {
        if (context == undefined)
            context = document;

        $("a[data-modal]", context).each(function() {
            var self = $(this);
            if (self.data("modalLinkDone") === undefined) {
                self.on("click", function(e) {
                    // hide dropdown if any
                    $(e.target).closest(".btn-group").children(".dropdown-toggle").dropdown("toggle");
                    $("#myModalContent").load(this.href, function(response, status, xhr) {
                        if (xhr.status === 200) {
                            $("#myModal").modal({
                                keyboard: true
                            }, "show");
                        } else {
                            alert("Something went wrong\n" + xhr.statusText);
                        }
                    });
                    return false;
                });
                self.data("modalLinkDone", true);
            }
        });
    }

    function bindForm(dialog) {
        bindModalButtons(dialog);
        if (window.bindAjaxButtons)
            window.bindAjaxButtons(dialog);

        $("form:not(.ignoreModalForm)", dialog).each(function() {
            var self = $(this);
            if (self.data("ajaxSubmitDone") === undefined) {
                self.submit(function(e) {
                    try {
                        tinyMCE.triggerSave();
                    } catch (exception) {
                        console.log("Couldn't trigger save on TinyMCE");
                    }
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: $(this).serialize(),
                        success: function(result) {
                            if (result.success) {
                                $("#myModal").modal("hide");
                                //Refresh
                                //location.reload();
                            } else {
                                $("#myModalContent").html(result);
                                bindForm();
                            }
                        }
                    });
                    //Stop the page navigating
                    e.preventDefault();
                    return false;
                });
                self.data("ajaxSubmitDone", true);
            }
        });
    }

})(window, jQuery.noConflict());