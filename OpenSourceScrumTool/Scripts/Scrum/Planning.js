(function (window, $, undefined) {
    
    //Ready
    $(function () {
        //Setup planning button
        $(".btn[data-action=\"PlanningTool\"]").planning();
    });

    var SELECTED_ITEM_CLASS = "planning-possibleFeatures";
    
    var planningTool = function() {
        var self = $(this);
        var projectId = $("#FeaturesPanel").data("project");
        var saveUrl = "/api/ScrumApi/" + $("#FeaturesPanel").data("save");
        var planningThisSprint = $(".item[data-in-current-sprint]").length === 0;
        var addButton = $(this).siblings("[data-action=\"PlanningToolAddToSprint\"]");
        var addButtonText = addButton.find("span[data-planning=\"sprintName\"]");
        var addButtonWeight = addButton.find("span[data-planning=\"sprintWeight\"]");
        window.inPlanning = false;
        var hiddenFeatures;
        var velocity;
        var selectedWeight = 0;
        var ignoreVelocity = false;
        var forceNextSprint = false;

        //Enable button
        self.removeClass("disabled btn-warning").addClass("btn-primary");

        $("a.addToSprint").click(function (event) {
            if (planningThisSprint) {
                event.preventDefault();
                var self = $(this);
                bootbox.confirm("You haven't done planning for this sprint yet, are you sure you want to add this feature to the sprint?", function (result) {
                    if (result === true) {
                        window.location.href = self.attr("href");
                    }
                });
            }
        });

        function getVelocity() {
            velocity = self.data("team-velocity");
        }

        //Click event
        self.click(function () {
            if (inPlanning)
                closePlanning();
            else
                openPlanning();
        });

        addButton.click(function () {
            if (selectedWeight > velocity && !ignoreVelocity && !forceNextSprint) {
                //Above weight and user clicked no to prompt
                bootbox.confirm("You are above the estimated weight, are you sure you want to continue?", function(result) {
                    ignoreVelocity = result;
                    if (result === true)
                        addButton.trigger("click");
                });
                return;
            }
            ignoreVelocity = false;
            //Lock button
            addButton.addClass("disabled");

            //Collect data
            var data = {
                projectId: projectId,
                featureIds: [],
                thisSprint: planningThisSprint,
                forceNextSpring: forceNextSprint
            };
            $("." + SELECTED_ITEM_CLASS).each(function() {
                data.featureIds.push($(this).data("id"));
            });

            //Post to server
            $.ajax({
                url: saveUrl,
                method: "POST",
                data: data,
                success: function (data, textStatus, xhr) {
                    //Unlock button
                    addButton.removeClass("disabled");
                    if (data.SprintSet === true) {
                        closePlanning();
                        bootbox.alert("Features added to " + (planningThisSprint ? "this" : "next") + " sprint");
                    } else if (data.NextSprintAlreadySet === true) {
                        bootbox.confirm("The next sprint is already set, would you like to overwrite it?", function(result) {
                            forceNextSprint = result;
                            if (result === true)
                                addButton.trigger("click");
                        });
                    } else {
                        bootbox.alert("Something went wrong, response: " + data);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    //Unlock button
                    addButton.removeClass("disabled");
                    bootbox.alert("Something went wrong, " + textStatus);
                }
            });
        });

        function openPlanning() {
            //Try reset filters, it doesn't matter if it doesn't work (live updating may not be enabled)
            try {
                window.Scrum.LiveUpdate.ViewTypes.Feature.Index.Model.FilterSprint(null);
                window.Scrum.LiveUpdate.ViewTypes.Feature.Index.Model.FilterState(null);
            } catch(ex) {
    
            }

            forceNextSprint = false;
            getVelocity();

            if (velocity === -1) {
                //No teams
                bootbox.alert("You don't have any teams associated with this project, please do this before continuing.");
                return;
            } else if (velocity === 0) {
                //Velocity not setup
                bootbox.alert("The teams assigned to this project have no velocity set, please do this before continuing.");
                return;
            }
            
            //Disable button here as we don't want to if the velocity is invalid
            self.addClass("disabled");

            //Hide features that are done
            var doneFeatures = $(".item[data-state=\"2\"");
            doneFeatures.hide();
            hiddenFeatures = doneFeatures;

            //Highlight next feasible features
            var thisSprintFeatures = $(".item[data-in-current-sprint]");
            planningThisSprint = thisSprintFeatures.length === 0;
            if (!planningThisSprint) {
                //Add the features in the current sprint to the hidden items
                thisSprintFeatures.hide();
                $.merge(hiddenFeatures, thisSprintFeatures);
                addButtonText.text("next");
            } else {
                addButtonText.text("this");
            }

            var potentialFeatures = $(".item:visible");

            if (potentialFeatures.length === 0) {
                closePlanning();
                bootbox.alert("There aren't any more features to do.");
                return;
            }

            var nextFeaureWeight = potentialFeatures.first().data("weight");
            if (nextFeaureWeight > velocity) {
                bootbox.alert("The next feature is too big for the current team, we recommend increasing the size of the team.");
            }

            var possibleFeatures = $(); //Empty jQuery array
            var reachedLimit = false;
            var skipLimit = 1;
            var skippedItems = 0;
            var index = 0;
            selectedWeight = 0;
            while (!reachedLimit) {
                var item = $(potentialFeatures[index]);
                var itemWeight = item.data("weight");
                if (selectedWeight + itemWeight <= velocity) {
                    //Select item
                    selectedWeight += itemWeight;
                    $.merge(possibleFeatures, item);
                } else {
                    skippedItems++;
                    if (skippedItems > skipLimit)
                        reachedLimit = true;
                }
                index++;
            }

            if (possibleFeatures.length === 0) {
                closePlanning();
                bootbox.alert("Something has gone wrong working out the possible features");
                return;
            }

            $(possibleFeatures).addClass(SELECTED_ITEM_CLASS);
            $(potentialFeatures).bind("click", itemClickEvent);

            //Button to set these to the next sprint
            //Don't know how to do this yet

            //Set planning button to leave planning
            inPlanning = true;
            addButton.show();
            updateAddButtonWeight();
            self.text("Leave Planning").removeClass("disabled");
        }

        function updateAddButtonWeight() {
            addButtonWeight.text(selectedWeight + "/" + velocity);
            if (selectedWeight === 0 && $("." + SELECTED_ITEM_CLASS).length > 0)
                addButton.removeClass("btn-success").addClass("btn-warning disabled");
            else if (selectedWeight > velocity)
                addButton.removeClass("btn-success disabled").addClass("btn-warning");
            else 
                addButton.removeClass("btn-warning disabled").addClass("btn-success");
        }

        function itemClickEvent() {
            var itemWeight = $(this).data("weight");
            if ($(this).hasClass(SELECTED_ITEM_CLASS)) {
                selectedWeight -= itemWeight;
            } else {
                selectedWeight += itemWeight;
            }
            $(this).toggleClass(SELECTED_ITEM_CLASS);
            updateAddButtonWeight();
        }

        function closePlanning() {
            self.addClass("disabled");

            //reset view
            $("." + SELECTED_ITEM_CLASS).removeClass(SELECTED_ITEM_CLASS);
            $(".item").unbind("click", itemClickEvent);
            hiddenFeatures.show();
            addButton.hide();
            inPlanning = false;
            self.text("Planning").removeClass("disabled");
        }
    }

    //Add to jQuery
    jQuery.fn.extend({
        planning: planningTool
    });
})(window, jQuery.noConflict());