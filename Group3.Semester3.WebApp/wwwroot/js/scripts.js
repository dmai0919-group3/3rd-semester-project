
function alert(message, title = 'Error') {
    
    $('#alert-modal-title').text(title);
    $('#alert-modal-message').text(message);
    
    $('#alertModal').modal();
}