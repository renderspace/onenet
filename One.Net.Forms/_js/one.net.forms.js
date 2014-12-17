(function($){

    "use strict";

    $(document).ready(function () {
        forms.init();
    });

    var forms = {

        init : function() {

            $('.validationGroup .causesValidation').on('click', forms.validate);
            $('.validationGroup :text').on('keydown', function (evt) {
                if (evt.keyCode == 13) {
                    // Find and store the next input element that comes after the
                    // one in which the enter key was pressed.
                    var $nextInput = $(this).nextAll(':input:first');
                    var $nextLinkButton = $(this).nextAll('a:first');
                    trace($nextLinkButton);
                    // If the next input is a submit button, go into validation.
                    // Else, focus the next form element as if enter == tab.
                    if ($nextLinkButton.is('.causesValidation')) {
                        if (forms.validate(evt)) {
                            eval($nextLinkButton.attr('href'));
                        }
                    } else if ($nextInput.is(':submit')) {
                        forms.validate(evt);
                    }
                    else {
                        evt.preventDefault();
                        $nextInput.focus();
                    }
                }
            });
        },
        validate : function(evt) {
            trace('Validate evt');
            var $group = $(this).parents('.validationGroup');
            var isValid = true;
            $group.find(':input').each(function (i, item) {
                if (!$(item).valid()) {
                    isValid = false;
                    $(item).closest('.form-group').addClass('has-error');
                }

            });
            if (!isValid)
                evt.preventDefault();
            trace(isValid);
            return isValid;
        }
    }
})(jQuery);