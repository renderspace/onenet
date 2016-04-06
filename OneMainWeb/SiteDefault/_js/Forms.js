/* global $, trace */
'use strict'

$(document).ready(function () {
  Forms.init()
})

var Forms = {
  init: function () {
    $('#aspnetForm').validate({
      onsubmit: false,
      invalidHandler: function (event, validator) {
        trace('default invalidHandler for validator')
      },
      errorPlacement: function (error, element) {
        if ($(element).attr('type') === 'checkbox' || $(element).attr('type') === 'radio' || $(element).hasClass('datepicker')) {
          $(element).parent().parent().append(error)
        } else {
          $(element).parent().append(error)
        }
      }
    })
    $('.mi .validationGroup .causesValidation').on('click', Forms.validate)
    $('.mi .validationGroup :text').on('keydown', function (evt) {
      if (evt.keyCode === 13) {
        // Find and store the next input element that comes after the
        // one in which the enter key was pressed.
        var $nextInput = $(this).nextAll(':input:first')
        var $nextLinkButton = $(this).nextAll('a:first')
        // trace($nextLinkButton)
        // If the next input is a submit button, go into validation.
        // Else, focus the next form element as if enter == tab.
        if ($nextLinkButton.is('.causesValidation')) {
          if (Forms.validate(evt)) {
            eval($nextLinkButton.attr('href'))
          }
        } else if ($nextInput.is(':submit')) {
          Forms.validate(evt)
        } else {
          trace('13 preventDefault')
          evt.preventDefault()
          $nextInput.focus()
        }
      }
    })
  },
  validate: function (evt) {
    trace('Validating form..')
    var $group = $(this).closest('.validationGroup')
    var isValid = true
    var $firstInvalidItem
    $group.find(':input').each(function (i, item) {
      if (!$(item).valid()) {
        isValid = false
        if (typeof ($firstInvalidItem) === 'undefined') {
          $firstInvalidItem = $(item)
        }
        $(item).closest('.form-group').addClass('has-error')
      }
    })
    if (!isValid) {
      evt.preventDefault()
      trace('preventDefault')
      $('html, body').animate({ scrollTop: $firstInvalidItem.offset().top - 40 }, 'slow', function () {
        $firstInvalidItem.focus()
      })
    }
    trace('isValid:' + isValid)
    return isValid
  }
}
