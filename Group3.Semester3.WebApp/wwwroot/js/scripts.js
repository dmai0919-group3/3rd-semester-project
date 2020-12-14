
function alert(message, title = 'Error') {
    
    $('#alert-modal-title').text(title);
    $('#alert-modal-message').text(message);
    
    $('#alertModal').modal();
}

function endsWithAny(suffixes, string) {
    for (let suffix of suffixes) {
        if(string.endsWith(suffix))
            return true;
    }
    return false;
}