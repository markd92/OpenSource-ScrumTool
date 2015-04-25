(function (window, $, undefined) {
    if (window.Scrum == undefined)
        window.Scrum = {};

    if (window.Scrum.LiveUpdate == undefined) {
        window.Scrum.LiveUpdate = {
            Hub: null,
            Connecting: false,
            MonitoredObjects: {}, //Use object so we can only have one property for each monitor method name
            SetupHub: function () {
                /// <summary>
                /// Starts the connection to the LiveUpdate Hub and starts listening for changes
                /// </summary>
                // Reference the auto-generated proxy for the hub.  
                /// <summary>
                /// s this instance.
                /// </summary>
                /// <returns></returns>
                this.Hub = $.connection.liveUpdateHub;
                // Create a function that the hub can call back to display messages.
                this.Hub.client.UpdateData = this.UpdateData;
                this.Hub.client.UpdateItemData = this.UpdateItemData;
                this.Hub.client.AddData = this.AddData;
                this.Hub.client.RemoveData = this.RemoveData;

                // Start the connection.
                this.Connecting = true;
                window.Scrum.LiveUpdate.Log("Connecting...");
                $.connection.hub.start().done(function () {
                    window.Scrum.LiveUpdate.Log("Connected");
                    window.Scrum.LiveUpdate.Connecting = false;
                });

                // Reconnect events
                var reconnectHandler = function () {
                    var self = window.Scrum.LiveUpdate;
                    //Loop through all of the objects we are monitoring
                    for (var i in self.MonitoredObjects) {
                        if (self.MonitoredObjects.hasOwnProperty(i)) {
                            //Find MonitorMethod in ViewTypes
                            for (var j in self.ViewTypes) {
                                if (self.ViewTypes.hasOwnProperty(j)) {
                                    for (var k in self.ViewTypes[j]) {
                                        if (self.ViewTypes[j].hasOwnProperty(k)) {
                                            //Check item has MonitorMehod and check it is what we are looking for
                                            if (self.ViewTypes[j][k].hasOwnProperty("MonitorMethod") && self.ViewTypes[j][k].MonitorMethod === self.MonitoredObjects[i]) {
                                                //Found object with this method
                                                self.StartMonitoring(ko.mapping.toJS(self.ViewTypes[j][k].Model), self.ViewTypes[j][k].MonitorMethod);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                $.connection.hub.reconnected(reconnectHandler);

                // Reconnect client if connection lost
                $.connection.hub.disconnected(function () {
                    window.Scrum.LiveUpdate.Log("Lost connection, trying again in 5 seconds");
                    window.Scrum.LiveUpdate.Connecting = true;
                    setTimeout(function () {
                        window.Scrum.LiveUpdate.Log("Connecting...");
                        $.connection.hub.start().done(function () {
                            window.Scrum.LiveUpdate.Log("Connected");
                            window.Scrum.LiveUpdate.Connecting = false;
                            reconnectHandler();
                        });
                    }, 5000); // Restart connection after 5 seconds.
                });
                $(function () {
                    //Lock controls
                    var dialogMonitorType, dialogMonitorId;
                    $("#myModal").on("show.bs.modal", function () {
                        var self = $(this);
                        var modalBody = self.find(".modal-body");
                        if (modalBody.length === 0)
                            return;

                        //Check if edit dialog
                        if (modalBody.data("edit")) {
                            dialogMonitorId = self.find("#Id").val();
                            dialogMonitorType = modalBody.data("edit");
                            //Check on server is anyone else has edit open
                            window.Scrum.LiveUpdate.Hub.server.edit_RequestLock(dialogMonitorType, dialogMonitorId).done(function (result) {
                                if (!result) {
                                    //Another user has this open, we need to lock it
                                    $("#editModal").modal({
                                        backdrop: "static",
                                        keyboard: false
                                    }, "show");
                                }
                            });
                        }
                    });
                    $("#myModal").on("hidden.bs.modal", function () {
                        //If I've setup this as monitored
                        if (dialogMonitorType && dialogMonitorId) {
                            //Tell the server I'm done
                            window.Scrum.LiveUpdate.Hub.server.edit_StopLock(dialogMonitorType, dialogMonitorId).done(function (result) {
                                dialogMonitorType = undefined;
                                dialogMonitorId = undefined;
                            });
                        }
                    });
                    $("#EditCancel").click(function () {
                        $("#myModal").modal("hide");
                    });
                    $("#EditTakeControl").click(function () {
                        $("#editModal").modal("hide");
                        window.Scrum.LiveUpdate.Hub.server.edit_TakeControl(dialogMonitorType, dialogMonitorId).done(function (result) {
                            window.Scrum.LiveUpdate.Log("Successfully told server I'm in control.");
                        });
                    });
                    window.Scrum.LiveUpdate.Hub.client.edit_UnlockDialog = function () {
                        if (!dialogMonitorType) {
                            //We weren't expecting this
                            window.Scrum.LiveUpdate.Hub.server.edit_StopAllLocks();
                        } else {
                            //Handle unlock
                            $("#editModal").modal("hide");
                            //TODO: Refresh view...
                            dialogMonitorType = undefined;
                            dialogMonitorId = undefined;
                        }
                    };
                    window.Scrum.LiveUpdate.Hub.client.edit_RemoveAccess = function () {
                        if (!dialogMonitorType) {
                            //We weren't expecting this
                            window.Scrum.LiveUpdate.Hub.server.edit_StopAllLocks();
                        } else {
                            //Handle lock
                            $("#editModal").modal({
                                backdrop: "static",
                                keyboard: false
                            }, "show");
                        }
                    };
                });
            },
            Log: function (message) {
                /// <summary>
                /// Displays a message in the Console
                /// </summary>
                /// <param name="message">Message to display</param>
                console.log("[LiveUpdate] - " + message);
            },
            EscapeSpecialChars: function (jsonString) {
                /// <summary>
                /// Escapes new lines in a json string
                /// </summary>
                /// <param name="jsonString">json string</param>
                return jsonString.replace(/\n/g, "\\n")
                    .replace(/\r/g, "\\r")
                    .replace(/\t/g, "\\t")
                    .replace(/\f/g, "\\f");
            },
            UpdateData: function (modelType, actionType, obj) {
                /// <summary>
                /// Handles an update command from the server
                /// </summary>
                /// <param name="modelType">Model Type, e.g. Project|Feature...</param>
                /// <param name="actionType">Action Type, e.g. Index|Details|Create</param>
                /// <param name="obj">View Model object</param>
                var viewType = window.Scrum.LiveUpdate.ViewTypes[modelType][actionType];

                if (viewType.Model == null) {
                    //Initial bind!
                    if (viewType.Mapping === undefined)
                        viewType.Model = ko.mapping.fromJS(obj);
                    else
                        viewType.Model = ko.mapping.fromJS(obj, viewType.Mapping);

                    if (viewType.Create != undefined)
                        viewType.Create(viewType.Model);

                    //Apply to DOM
                    ko.applyBindings(viewType.Model, $(viewType.Element)[0]);
                    //Start monitoring
                    if (this.Hub != null) {
                        this.StartMonitoring(obj, viewType.MonitorMethod);
                    }
                } else {
                    ko.mapping.fromJS(obj, viewType.Model);
                    window.Scrum.LiveUpdate.Log("Updated model " + viewType.Element);
                }
                //Call bind modal to ensure buttons are linked
                window.bindModalButtons();
                window.bindAjaxButtons();
                window.bindDblClicks();
            },
            UpdateItemData: function (modelType, actionType, obj) {
                /// <summary>
                /// Handles an update item command from the server
                /// </summary>
                /// <param name="modelType">Model Type, e.g. Project|Feature...</param>
                /// <param name="actionType">Action Type, e.g. Index|Details|Create</param>
                /// <param name="obj">Item View Model object</param>
                var viewType = window.Scrum.LiveUpdate.ViewTypes[modelType][actionType];

                if (viewType.Model == null) {
                    //Error!
                    window.Scrum.LiveUpdate.Log("Cannot update " + viewType.Element + " model is null");
                } else {
                    //Find and update in original object
                    var modelObj = ko.mapping.toJS(viewType.Model);
                    var key = viewType.Key != undefined ? viewType.Key : "Id"; //Note capital Id
                    for (var i in modelObj[viewType.ListItem]) {
                        if (modelObj[viewType.ListItem].hasOwnProperty(i)) {
                            var item = modelObj[viewType.ListItem][i];
                            if (item[key] === obj[key]) {
                                modelObj[viewType.ListItem][i] = obj;
                                //Item found, stop looping
                                break;
                            }
                        }
                    }
                    //Update with changed model
                    ko.mapping.fromJS(modelObj, viewType.Model);
                    //Check and refresh sort
                    if (viewType.Sort != undefined)
                        viewType.Sort(viewType.Model);
                    //Call bind modal to ensure buttons are linked
                    window.bindModalButtons();
                    window.bindAjaxButtons();
                    window.bindDblClicks();
                    if (window.bindSprintLists != undefined)
                        window.bindSprintLists();
                    window.Scrum.LiveUpdate.Log("Updated item in " + viewType.Element);
                }
            },
            AddData: function (modelType, actionType, obj) {
                /// <summary>
                /// Handles an add command from the server
                /// </summary>
                /// <param name="modelType">Model Type, e.g. Project|Feature...</param>
                /// <param name="actionType">Action Type, e.g. Index|Details|Create</param>
                /// <param name="obj">View Model object</param>

                var viewType = window.Scrum.LiveUpdate.ViewTypes[modelType][actionType];
                if (viewType.Model == null) {
                    window.Scrum.LiveUpdate.UpdateData(modelType, actionType, obj);
                } else {
                    var item = ko.mapping.fromJS(obj);
                    //Check it hasn't already been added
                    if (viewType.Model[viewType.ListItem].mappedIndexOf(item) === -1) {
                        viewType.Model[viewType.ListItem].push(item);
                        //Check and refresh sort
                        if (viewType.Sort != undefined)
                            viewType.Sort(viewType.Model);
                        //Update the dialog buttons
                        window.bindModalButtons();
                        window.bindAjaxButtons();
                        window.bindDblClicks();
                        //Start monitoring it
                        //TODO: Change this to only pass back the new item not the full model! or find another way
                        window.Scrum.LiveUpdate.StartMonitoring(ko.mapping.toJS(viewType.Model), viewType.MonitorMethod);
                        window.Scrum.LiveUpdate.Log("Added item to " + viewType.Element);
                    }
                }
            },
            RemoveData: function (modelType, actionType, id) {
                /// <summary>
                /// Handles a remove command from the server
                /// </summary>
                /// <param name="modelType">Model Type, e.g. Project|Feature...</param>
                /// <param name="actionType">Action Type, e.g. Index|Details|Create</param>
                /// <param name="id">View Model object</param>

                var viewType = window.Scrum.LiveUpdate.ViewTypes[modelType][actionType];
                if (viewType.Model == null) {
                    //If the view model is null then there isn't anything displayed to remove from.
                    return;
                } else {
                    //Remove the item using the mapping
                    var key = viewType.Key != undefined ? viewType.Key : "Id"; //Note capital Id
                    var mapping = {};
                    mapping[key] = id;
                    viewType.Model[viewType.ListItem].mappedRemove(mapping);
                    //refresh view if setup
                    if (viewType.Model.ForceRefresh != undefined)
                        viewType.Model.ForceRefresh();
                    window.Scrum.LiveUpdate.Log("Removed item from " + viewType.Element);
                }
            },
            ParseJson: function (json) {
                /// <summary>
                /// Convert model from a json string
                /// </summary>
                /// <param name="json">json string</param>
                return JSON.parse(this.EscapeSpecialChars(json));
            },
            StartMonitoring: function (obj, monitorMethod) {
                /// <summary>
                /// Posts the object back to the server to register this client as listening for changes
                /// </summary>
                /// <param name="obj">View Model object</param>
                /// <param name="monitorMethod">String name for registration</param>
                if (this.Hub != null) {
                    if (this.Connecting === true) {
                        //Connection not finished initialising
                        //try again in a timeout
                        setTimeout(function () {
                            window.Scrum.LiveUpdate.StartMonitoring(obj, monitorMethod);
                        }, 10);
                    } else {
                        this.Hub.server[monitorMethod](obj).done(function (result) {
                            if (result == true) {
                                //Store that this is monitored for reconnect
                                window.Scrum.LiveUpdate.MonitoredObjects[monitorMethod] = true;
                            }
                            window.Scrum.LiveUpdate.Log(monitorMethod + " => " + result);
                        });
                    }
                }
            },
            ViewTypes: {
                Project: {
                    Index: {
                        Model: null,
                        ListItem: "Projects",
                        Sort: $.throttle(function (model) {
                            var sortFunc = function (l, r) {
                                return l.Priority() > r.Priority() ? 1 : -1;
                            };
                            setTimeout(function () {
                                model.Projects.sort(sortFunc);
                                window.Scrum.LiveUpdate.Log("Sorted Projects List");
                            }, 50);
                        }, 50),
                        Mapping: {
                            'Projects': {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.Id);
                                }
                            }
                        },
                        Element: "#ProjectsPanel",
                        MonitorMethod: "startMonitoring_ProjectIndexViewModel"
                    },
                    Details: {
                        Model: null,
                        Element: "[data-panel-details=\"Project\"]",
                        MonitorMethod: "startMonitoring_ProjectDetailsViewModel"
                    }
                },
                Feature: {
                    Index: {
                        Model: null,
                        ListItem: "Features",
                        Create: function (model) {
                            window.Scrum.LiveUpdate.ViewTypes.Feature.Index.Sort(model);
                            model.FilterState = ko.observable(null);
                            model.FilterSprint = ko.observable(null);;
                            var forceChange = ko.observable(false);
                            model.FilteredList = ko.computed(function () {
                                return ko.utils.arrayFilter(model.Features(), function (feature) {
                                    if (forceChange())
                                        return false;

                                    var passedState = true;
                                    if (model.FilterState() != null)
                                        passedState = feature.State() === model.FilterState();

                                    var passedSprint = true;
                                    if (model.FilterSprint() != null && feature.Sprints != undefined)
                                        passedSprint = (feature.Sprints().indexOf(model.FilterSprint()) !== -1);

                                    return passedState && passedSprint;
                                });
                            }).extend({ notify: "always" });
                            model.AfterRenderHandler = function () {
                                //Call bind modal to ensure buttons are linked
                                window.bindModalButtons();
                                window.bindAjaxButtons();
                                window.bindDblClicks();
                                window.bindSortLists();
                                if (window.bindSprintLists != undefined)
                                    window.bindSprintLists();
                            }
                            model.ForceRefresh = function () {
                                var beforeData = model.FilteredList();
                                forceChange(true);
                                var changedData = model.FilteredList();
                                forceChange(false);
                                var afterData = model.FilteredList();
                            }
                            model.FilterSprintIteration = ko.computed(function () {
                                if (model.FilterSprint() != null) {
                                    var sprints = model.Sprints();
                                    for (var i = 0; i < sprints.length; i++) {
                                        if(sprints[i].Id() === model.FilterSprint())
                                            return sprints[i].Iteration();
                                    }
                                }
                                return null;
                            });
                        },
                        Sort: $.throttle(function (model) {
                            var sortFunc = function (l, r) {
                                return l.Priority() > r.Priority() ? 1 : -1;
                            };
                            setTimeout(function () {
                                model.Features.sort(sortFunc);
                                window.Scrum.LiveUpdate.Log("Sorted Features List");
                            }, 50);
                        }, 50),
                        Mapping: {
                            'Features': {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.Id);
                                }
                            }
                        },
                        Element: "#FeaturesPanel",
                        MonitorMethod: "startMonitoring_FeatureIndexViewModel"
                    },
                    Details: {
                        Model: null,
                        Element: "[data-panel-details=\"Feature\"]",
                        MonitorMethod: "startMonitoring_FeatureDetailsViewModel"
                    }
                },
                Task: {
                    Index: {
                        Model: null,
                        ListItem: "Tasks",
                        Create: function (model) {
                            window.Scrum.LiveUpdate.ViewTypes.Task.Index.Sort(model);
                            window.Scrum.LiveUpdate.ViewTypes.Task.Index.AddStateComputed(model, "TasksNotStarted", 0);
                            window.Scrum.LiveUpdate.ViewTypes.Task.Index.AddStateComputed(model, "TasksInProgress", 1);
                            window.Scrum.LiveUpdate.ViewTypes.Task.Index.AddStateComputed(model, "TasksDone", 2);
                        },
                        Sort: $.throttle(function (model) {
                            var sortFunc = function (l, r) {
                                return l.Priority() > r.Priority() ? 1 : -1;
                            };
                            setTimeout(function () {
                                model.Tasks.sort(sortFunc);
                                window.Scrum.LiveUpdate.Log("Sorted Tasks List");
                            }, 50);
                        }, 50),
                        AddStateComputed: function (model, name, state) {
                            model[name] = ko.computed(function () {
                                return ko.utils.arrayFilter(model.Tasks(), function (task) {
                                    return task.State() === state;
                                });
                            });
                        },
                        Mapping: {
                            'Tasks': {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.Id);
                                }
                            }
                        },
                        Element: "#TasksPanel",
                        MonitorMethod: "startMonitoring_TaskIndexViewModel"
                    }
                },
                Colleague: {
                    Index: {
                        Key: "Email",
                        Model: null,
                        ListItem: "Colleagues",
                        Mapping: {
                            'Colleagues': {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.Email);
                                }
                            }
                        },
                        Element: "#ColleaguesPartial",
                        MonitorMethod: "startMonitoring_ColleagueViewModel"
                    }
                },
                Team: {
                    Index: {
                        Model: null,
                        ListItem: "Teams",
                        Mapping: {
                            'Teams': {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.Id);
                                }
                            }
                        },
                        Element: "#TeamsPartial",
                        MonitorMethod: "startMonitoring_TeamViewModel"
                    }
                },
                Sprint: {
                    Index: {
                        Key: "ProjectId",
                        Model: null,
                        ListItem: "Projects",
                        Mapping: {
                            "Projects": {
                                key: function (data) {
                                    return ko.utils.unwrapObservable(data.ProjectId);
                                }
                            }
                        },
                        Element: "#TasksPanel",
                        MonitorMethod: "startMonitoring_SprintIndexViewModel"
                    }
                }
            }
        }
    }

    //Ready Event
    window.Scrum.LiveUpdate.SetupHub();

})(window, jQuery.noConflict());