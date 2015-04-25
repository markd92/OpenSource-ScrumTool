(function (window, $, undefined) {

    //Ready
    $(function () {
        //Sortable lists

        bindSortLists();
        bindDblClicks();
    });

    function bindDblClicks() {
        $("li.item").each(function() {
            var self = $(this);
            if (self.data("manageDblClickDone") === undefined) {
                self.dblclick(function () {
                if(window.inPlanning !== true)
                    navigateTo($(this).find(".manage-btn").attr("href"));
                });
                self.data("manageDblClickDone", true);
            }
        });
    }
    window.bindDblClicks = bindDblClicks;

    function bindSortLists() {
        var $sortList = $("[data-ordersave]:not(.sort-list-set)");
        $sortList.sortable({
            connectWith: "[data-ordersave]",
            placeholder: "sortable-ghost",
            forcePlaceholderSize: true,
            update: function (event, ui) {
                //Store order
                storeListOrder();
                //Check if item has moved between lists, if it has we need to remove it
                var from = event.target.closest(".ui-sortable");
                var to = event.toElement.closest(".ui-sortable");
                if (from !== to) {
                    event.toElement.closest(".item").remove();
                }
            }
        });
        $sortList.addClass("sort-list-set");
    }
    window.bindSortLists = bindSortLists;

    function storeListOrder() {
        var $sortList = $($(".sort-list").get().reverse());
        var boardView = $sortList.length > 1;
        var count = 0;
        if ($sortList.data("ordersave") != undefined) {
            var apiUrl = $sortList.data("ordersave");
            var ids = [];
            $sortList.children("li.item").each(function () {
                if (boardView) {
                    ids.push({
                        ID: $(this).data("id"),
                        Priority: count++,
                        State: $(this).parents(".sort-list").data("columnstate")
                    });
                } else {
                    ids.push($(this).data("id"));
                }

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

})(window, jQuery.noConflict());

function navigateTo(href) {
    jQuery("<form>").attr({ action: href, method: "GET" }).appendTo(jQuery("body")).submit();
}