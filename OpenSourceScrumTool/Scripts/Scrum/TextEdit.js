(function (window, $, undefined) {
    //Ready
    $(function () {
        tinyMCE.init({
            // General options
            mode: "textareas",
            theme: "modern",
            // Theme options
            menubar: false,
            toolbar: "bold,italic,underline,|,bullist,numlist",
            statusbar: false,
            init_instance_callback: function (editor) {
                console.log("tinymce init: " + editor.id);
            }
        });

        $("#myModal").on("show.bs.modal", function () {
            //Put in a time out to ensure that the element is defiantly visible before setting up tinymce
            setTimeout(function() {
                var textAreas = $("textarea", $("#myModal"));
                for (var i = 0; i < textAreas.length; i++) {
                    //Check if element already has editor enabled
                    if (tinymce.get(textAreas[i].id)) {
                        //Remove existing editor
                        tinyMCE.execCommand("mceRemoveEditor", false, textAreas[i].id);
                    }

                    //Add editor
                    tinyMCE.execCommand("mceAddEditor", false, textAreas[i].id);
                }
            }, 25);
        });

        $("#myModal").on("hidden.bs.modal", function () {
            //Remove all editors in dialog
            var textAreas = $("textarea", $("#myModal"));
            for (var i = 0; i < textAreas.length; i++) {
                //Check if element already has editor enabled
                if (tinymce.get(textAreas[i].id))
                    tinyMCE.execCommand("mceRemoveEditor", false, textAreas[i].id);
            }
        });
    });
})(window, jQuery.noConflict());