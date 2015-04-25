(function (window, $, undefined) {
    if (window.Scrum == undefined)
        window.Scrum = {};

    if (window.Scrum.Sprint == undefined)
        window.Scrum.Sprint = {
            BindDialogEvent: function(context) {
                $("#AddSprintForm", context).submit(function () {
                    if ($(this).valid()) {
                        $.ajax({
                            url: this.action,
                            type: this.method,
                            data: $(this).serialize(),
                            beforeSend: Scrum.Sprint.AddSprint_Begin,
                            success: Scrum.Sprint.AddSprint_Success,
                            error: Scrum.Sprint.AddSprint_Failure
                        });
                    }
                    return false;
                });
            },
            AddSprint_Begin: function () {
                console.log("Add sprint request sent");
            },
            AddSprint_Success: function (a, b, c) {
                var bubble = document.createElement("div");
                bubble.classList.add("alert", "alert-success", "alert-dismissable");
                $(bubble).append("<button type=\"button\" class=\"close\" data-dismiss=\"alert\">×</button>");
                $(bubble).append(a != "" ? a : "Sprint Added");
                $("#addResult").empty().append(bubble);
                Scrum.Sprint.UpdateSprintsList();
            },
            AddSprint_Failure: function (a, b, c) {
                var bubble = document.createElement("div");
                bubble.classList.add("alert", "alert-dismissable", "alert-danger");
                $(bubble).append("<button type=\"button\" class=\"close\" data-dismiss=\"alert\">×</button>");
                $(bubble).append(c != "" ? c : "Something went wrong, please try again later.");
                $("#addResult").empty().append(bubble);
            },
            UpdateSprintsList: function () {
                var area = $("#sprints_list_area");
                area.load(area.data("update-url"), function () {
                    console.log("updated user list area");
                    Scrum.DelegatedAccess.BindEvent(this);
                });
                return false;
            }
        }
})(window, jQuery.noConflict());