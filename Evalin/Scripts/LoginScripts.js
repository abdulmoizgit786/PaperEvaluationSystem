jQuery(document).ready(function () {

    $.backstretch("/Images/Background_Login.png");
    

    $('#datepicker').datepicker({
        uiLibrary: 'bootstrap'
    });


    if ($('.show-login-form').hasClass('active')) {
        $('.l-form-username').focus();
    }
    else {
        $('.r-form-first-name').focus();
    }

    var lfs = 0;
    var rfs = 0;

    
    $('.show-register-form').on('click', function () {
        
    	if( ! $(this).hasClass('active') ) {
    		$('.show-login-form').removeClass('active');
    		$(this).addClass('active');
    		$('.login-form').fadeOut('fast', function(){
    		    rfs = 0;
    		    $('.validation').text('');
    		    $('.register-form').fadeIn('fast');
    		    $('.r-form-first-name').focus();
    		});
    	}
    });
    

    $('.show-login-form').on('click', function(){
 
        if (!$(this).hasClass('active')) {
    		$('.show-register-form').removeClass('active');
    		$(this).addClass('active');
    		$('.register-form').fadeOut('fast', function () {
    		    lfs = 0;
    		    $('.validation').text('');
    		    $('.login-form').fadeIn('fast');
    		    $('.l-form-username').focus();
    		});
    		
        }
     });
    
   
    $('.l-form input').blur(function () {
        if (lfs > 0)
        {
            if ($(this).val() == '') {
                $(this).addClass('input-error');
                $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for'));
            }
            else {
                $(this).removeClass('input-error');
                $(this).next().text('');
            }
        }
    });
    
    $('.l-form').on('submit', function(e) {
        lfs++;
    	$(this).find('input').each(function(){
    		if( $(this).val() == "" ) {
    			e.preventDefault();
    			$(this).addClass('input-error');
    			if ($(this).next().prop('tagName') == 'SPAN') {
    			    $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for'));
    			}
    		}
    		else {
    		    $(this).removeClass('input-error');
    		    $(this).next().text('');
    		}
    	});
    	
    });
    
  
    $('.r-form input').blur(function () {
       
        if (rfs > 0) {
            if ($(this).attr("name") != "DOB" && $(this).attr("name") != "Image" && $(this).attr("name") != "Usertype" && $(this).attr("name") != "Age" && $(this).prop('id') != 'r-form-cpassword' && $(this).prop('id') != 'r-form-password') {


                if ($(this).val() == '') {
                    $(this).addClass('input-error');
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for'));
                    }
                }
                else {
                    $(this).removeClass('input-error');
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }
                }

            }
            else if ($(this).attr("name") == "Age") {
                if ($(this).val() >= 5 && $(this).val() <= 100) {
                    $(this).removeClass('input-error');
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }
                }
                else {
                    $(this).addClass('input-error');
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('Age 5~100');
                    }
                }
            }
            else if ($(this).prop('id') == 'r-form-cpassword') {
                if ($(this).val() != $('#r-form-password').val()) {
                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('Confirm Password Should Be Same');
                    }
                }
                else {
                    if ($(this).hasClass('input-error'))
                    { $(this).removeClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }
                }

            }

            else if ($(this).prop('id') == 'r-form-password') {
                
                if ($(this).val() == '' || $(this).val().length < 10) {

                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }

                    if ($(this).next().prop('tagName') == 'SPAN') {
                        if ($(this).val() == '')
                        {
                            $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for'));
                        }
                        else {
                            $(this).next().text('Password Length Should be 10 or more Characters');
                        }
                    }
                }
                else {
                    if ($(this).hasClass('input-error'))
                    { $(this).removeClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }

                }

            }

        }
    });

    $('.r-form input[name="DOB"]').change(function () {

        var date = new Date($(this).val())
        var cdate = new Date()
       
        if ((Number)(date.getYear().toString().substring(1)) >= (Number)((cdate.getYear() - 5).toString().substring(1)) || $(this).val() == '') {

            if (!$(this).hasClass('input-error'))
            { $(this).addClass('input-error'); }
            $('#rf_dv').text('Invalid Date Of Birth');

        }
        else {
            if ($(this).hasClass('input-error'))
            { $(this).removeClass('input-error'); }
            $('#rf_dv').text('');
        }

    });

    

    $('.r-form').on('submit', function(e) {
       
        rfs++;
        $(this).find('input').each(function(){
            
            if ($(this).prop('name') != 'Image' && $(this).prop('name') != 'DOB' && $(this).prop('type') != 'number' && $(this).prop('Usertype') != 'Usertype' && $(this).prop('id') != 'r-form-cpassword' && $(this).prop('id') != 'r-form-password')
            {
                if ($(this).val() == '')
                {
                    e.preventDefault();
                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN')
                    {
                        $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for'));
                    }
                }
                else
                {
                    if ($(this).hasClass('input-error'))
                    {$(this).removeClass('input-error');}
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }
                }
            
            }
            else if ($(this).prop('type') == 'number')
            {
                if ($(this).val() == '' || $(this).val() < 5 ||  $(this).val() > 100)
                {
                    e.preventDefault();
                    if (!$(this).hasClass('input-error'))
                    {$(this).addClass('input-error');}
                    if ($(this).next().prop('tagName') == 'SPAN')
                    {
                        $(this).next().text('Age 5~100');
                    }
                }
                else
                {
                    if ($(this).hasClass('input-error'))
                    {$(this).removeClass('input-error');}
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }
                
                }
            }
            else if ($(this).attr('name') == 'DOB')
            {
                var date = new Date($(this).val())
                var cdate = new Date()
             
                if ((Number)(date.getYear().toString().substring(1)) >= (Number)((cdate.getYear() - 5).toString().substring(1)) || $(this).val() == '') {
                    e.preventDefault();
                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }
                    $('#rf_dv').text('Invalid Date Of Birth');
                    
                }
                else {
                    if ($(this).hasClass('input-error'))
                    { $(this).removeClass('input-error'); }
                    $('#rf_dv').text('');
                }
            }
            else if ($(this).attr('id') == 'r-form-cpassword')
            {
                if ($(this).val() != $('#r-form-password').val()) {
                    e.preventDefault();
                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('Confirm Password Should Be Same');
                    }
                }
                else {
                    if ($(this).hasClass('input-error'))
                    { $(this).removeClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }

                }
            
            }
            else if ($(this).attr('id') == 'r-form-password') {
                if ($(this).val()== '' || $(this).val().length < 10) {
                    e.preventDefault();
                    if (!$(this).hasClass('input-error'))
                    { $(this).addClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {

                        if ($(this).val() == '')
                        { $(this).next().text('Please Enter ' + $(this).next().attr('data-valmsg-for')); }
                        else {
                            $(this).next().text('Password Length Should be 10 or more Characters');
                        }
                    }
                }
                else {
                    if ($(this).hasClass('input-error'))
                    { $(this).removeClass('input-error'); }
                    if ($(this).next().prop('tagName') == 'SPAN') {
                        $(this).next().text('');
                    }

                }

            }
      
        });
        });


  
    $(".showonhover").click(function () {
        $("#selectfile").trigger('click');
    });
  
    $("#file1").change(function (e,t) {

        if (e.target.files.length != 0)
        {
            var extension = e.target.files[0].name.substring(e.target.files[0].name.lastIndexOf('.') + 1);
            if (extension == 'png' || extension == 'jpg' || extension == 'jpeg') {
                displayAsImage(e.target.files[0]);
            }
       }
    });


    $("#upfile1").click(function () {
        $("#file1").trigger('click');
    });


});

function displayAsImage(file) {
    $("#upfile1").attr('src', URL.createObjectURL(file));
}
