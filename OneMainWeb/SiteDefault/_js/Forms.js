/* global $, trace, location, ga */
'use strict'

$(document).ready(function () {
  Forms.init()
})

window.onload = function (e) {
  Forms.load()
}

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
  },
  load: function (evt) {
    if (window.FormId > 0) {
      var page = document.location.pathname + '/thank-you-for-form-' + window.FormId + location.search + location.hash
      if (window.ga) {
        ga('send', 'pageview', page)
      } else {
        console.log('no GA... but if it was here, we would track this: ' + page)
      }
    }

    if (window.language === 'bs' || window.language === 'hr' || window.language === 'sl' || window.language === 'sr') {
      // Replace the builtin US date validation with bs/hr/sl/sr date validation
      $.validator.addMethod(
        'date',
        function (value, element) {
          // have to do this stuff because of Firefox date implementation
          var bits = value.match(/([0-9]+)/gi)

          if (!bits || bits.length < 3) {
            return this.optional(element) || false
          }
          var reconstructed = bits[0] + '.' + bits[1] + '.' + bits[2]
          if (value !== reconstructed) {
            return this.optional(element) || false
          }

          var day = parseInt(bits[0], 10)
          var month = parseInt(bits[1], 10)
          var year = parseInt(bits[2], 10)

          if (month < 1 || month > 12 || day < 1 || day > 31 || year < 1 || year > 9999) {
            return this.optional(element) || false
          }

          var timestamp = Date.parse(month + '/' + day + '/' + year)
          if (isNaN(timestamp) === false) {
            // var d = new Date(timestamp)
            return this.optional(element) || true
          }
          return this.optional(element) || false
        }
      )
    }
  }
}
