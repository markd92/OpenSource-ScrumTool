(function (window, $, undefined) {

    //Ready
    $(function () {
        //hide remembered divs
        var hiddenPanelsStartup = localStorage.getItem("hiddenPanels");
        if (hiddenPanelsStartup) {
            hiddenPanelsStartup = JSON.parse(hiddenPanelsStartup);
            $(".panel-dismissable").each(function() {
                var panelName = $(this).data("panel-details");
                if (hiddenPanelsStartup[panelName] === true) {
                    //hide panel body
                    var panel = $(this);
                    panel.find(".panel-body").hide();
                    //swap buttons
                    panel.find("button.close:not(.show-btn)").hide();
                    panel.find("button.show-btn").show();
                }
            });
        }
        
        //bind event
        $(".panel-dismissable button.close:not(.show-btn)").click(function () {
            //hide panel body
            var panel = $(this).closest(".panel");
            panel.find(".panel-body").hide();
            //swap buttons
            $(this).hide();
            panel.find("button.show-btn").show();
            //store change
            var hiddenPanels = localStorage.getItem("hiddenPanels");
            if (!hiddenPanels)
                hiddenPanels = {};
            else
                hiddenPanels = JSON.parse(hiddenPanels);
            hiddenPanels[panel.data("panel-details")] = true;
            localStorage.setItem("hiddenPanels", JSON.stringify(hiddenPanels));
        });

        $(".panel-dismissable button.show-btn").click(function () {
            //show panel body
            var panel = $(this).closest(".panel");
            panel.find(".panel-body").show();
            //swap buttons
            $(this).hide();
            panel.find("button.close:not(.show-btn)").show();
            //store change
            var hiddenPanels = localStorage.getItem("hiddenPanels");
            if (!hiddenPanels)
                hiddenPanels = {};
            else
                hiddenPanels = JSON.parse(hiddenPanels);
            hiddenPanels[panel.data("panel-details")] = false;
            localStorage.setItem("hiddenPanels", JSON.stringify(hiddenPanels));
        });
    });

})(window, jQuery.noConflict());