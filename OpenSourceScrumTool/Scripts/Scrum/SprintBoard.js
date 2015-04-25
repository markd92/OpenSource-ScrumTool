(function (window, $, undefined) {
    function storeListOrder(featureId) {
        var featureLists = $($(".sort-list[data-feature=\"" + featureId + "\"").get().reverse());
        var count = 0;
        if (featureLists.data("ordersave") != undefined) {
            var apiUrl = featureLists.data("ordersave");
            var ids = [];
            featureLists.children("li.item").each(function () {
                ids.push({
                    ID: $(this).data("id"),
                    Priority: count++,
                    State: $(this).parents(".sort-list").data("columnstate")
                });
            });
            $.ajax({
                url: "/api/ScrumApi/" + apiUrl,
                data: { '': ids },
                type: "POST",
                dataType: "json",
                success: function (data) {
                    if (data === true)
                        console.log("order stored successfully");
                    else
                        alert("Something went wrong:\n\nUnable to save the priority order.");
                },
                error: function (x, y, z) {
                    alert("Something went wrong:\n\n" + x + "\n" + y + "\n" + z);
                }
            });
        }
    }

    function setupSprintLists() {
        //Sortable lists
        $(".sprint-list:not(.sortabledone)").each(function () {
            var list = $(this);
            list.sortable({
                connectWith: "[data-feature=\"" + $(this).data("feature") + "\"]",
                placeholder: "sortable-ghost",
                forcePlaceholderSize: true,
                update: function (event, ui) {
                    var from = event.target.closest(".ui-sortable");
                    var to = event.toElement.closest(".ui-sortable");
                    var featureId = $(to).data("feature");

                    //Store order
                    storeListOrder(featureId);

                    //Check if item has moved between lists, if it has we need to remove it
                    if (from !== to) {
                        event.toElement.closest(".item").remove();
                    }
                }
            });
            list.addClass("sortabledone");
        });
    }

    //Ready
    $(function () {
        setupSprintLists();
    });

    window.bindSprintLists = setupSprintLists;
})(window, jQuery.noConflict());
