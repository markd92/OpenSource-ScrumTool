(function (window, $, undefined) {

    window.bindAjaxButtons = function(context) {
        var elements = $("[data-ajax]:not(.bindAjaxButtons)", context);
        elements.addClass("bindAjaxButtons");
        elements.each(function () {
            if ($(this).is("a")) {
                $(this).click(function(event) {
                    event.preventDefault();
                    var self = $(this);
                    var url = self.attr("href");
                    var method = self.data("ajax-type");
                    var spinner = self.siblings(".ajax-loading");
                    var success = self.siblings(".ajax-success");
                    var error = self.siblings(".ajax-error");
                    $.ajax({
                        url: url,
                        type: method != undefined ? method : "GET",
                        beforeSend: function() {
                            self.hide();
                            spinner.show();
                        },
                        success: function(data, textStatus, xhr) {
                            if (data.success === true) {
                                spinner.hide();
                                success.show();
                                $("#ErrorMessageAlert").hide();
                            } else {
                                spinner.hide();
                                error.show();
                                $("#ErrorMessageText").text(data.error);
                                $("#ErrorMessageAlert").show();
                            }
                        },
                        error: function(xhr, textStatus, errorThrown) {
                            spinner.hide();
                            error.show();

                            $("#ErrorMessageText").text(errorThrown);
                            $("#ErrorMessageAlert").show();
                        }
                    });
                });
            } else if($(this).is("form")) {
                //bind to submit
                $(this).submit(function (event) {
                    event.preventDefault();
                    var self = $(this);

                    //Check validation
                    if (self.find(".input-validation-error").length > 0)
                        return;

                    var url = self.attr("target");
                    var method = self.attr("method");
                    var spinner = self.find(".ajax-loading");
                    var success = self.find(".ajax-success");
                    var error = self.find(".ajax-error");
                    $.ajax({
                        url: url,
                        type: method != undefined ? method : "GET",
                        data: self.serialize(),
                        beforeSend: function () {
                            error.hide();
                            success.hide();
                            spinner.show();
                        },
                        success: function (data, textStatus, xhr) {
                            if (data.success === true) {
                                spinner.hide();
                                success.show();
                                $("#ErrorMessageAlert").hide();
                            } else {
                                spinner.hide();
                                error.show();
                                $("#ErrorMessageText").text(data.error);
                                $("#ErrorMessageAlert").show();
                            }
                        },
                        error: function (xhr, textStatus, errorThrown) {
                            spinner.hide();
                            error.show();

                            $("#ErrorMessageText").text(errorThrown);
                            $("#ErrorMessageAlert").show();
                        }
                    });
                });
            } else if ($(this).is("input")) {
                //trigger submit on change
                $(this).on("change", function () {
                    //If the user is pressing the arrow buttons in a text input it doesn't call keyup
                    $(this).trigger("keyup");
                });
                $(this).keyup($.debounce(function() {
                    // Will only execute 300ms after the last keypress.
                    $(this).closest("form").trigger("submit");
                }, 200));
            }
        });
    }

    //Ready
    $(function () {
        $.ajaxSetup({ cache: false });

        window.bindAjaxButtons(document);

        //Bind open
        $("#myModal").on("show.bs.modal", function () {
            window.bindAjaxButtons(this);
        });
    });

})(window, jQuery.noConflict());