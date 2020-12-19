const emptyGuid = "00000000-0000-0000-0000-000000000000";

let dirArray = {"00000000-0000-0000-0000-000000000000": "Home"};

let currentDir = emptyGuid;
let currentGroup = emptyGuid;

let browsingSharedFiles = false;

let currentUser = null;

let previewFiles = ['.png', '.jpg', '.jpeg', '.mp4', '.avi', '.webm', '.mp3', '.wav'];

$(function () {
    browseDirectoryFiles(emptyGuid);

    $.contextMenu({
        selector: '.file',
        build: function($trigger, e) {
            let items = {};
            
            let fileName = $trigger.find('.file-name').text();
            let classes = $trigger.attr('class');
            if (endsWithAny(previewFiles, fileName)) {
                let view = {
                    view: {
                        name: "View",
                        callback: function (key, opt) {
                            let $element = opt.$trigger;
                            let id = $element.attr('id');
                            let fileName = $element.find('.file-name').text();
                            
                            previewFile(id, fileName);
                        }
                    }
                }
                Object.assign(items, view);
            }
            if (classes.includes('txt-file')) {
                let edit = {
                    edit: {
                        name: "View / Edit",
                        callback: function (key, opt) {
                            showEditFileModal(key, opt);
                        }
                    }
                };
                Object.assign(items, edit);
            }
            if (!classes.includes('folder')) {
                let download = {
                    download: {
                        name: "Download",
                        callback: function (key, opt) {
                            let $element = opt.$trigger;
                            let id = $element.attr('id');
                            
                            initFileDownload(id);
                        }
                    }
                };
                Object.assign(items, download);
            }
            
            if (currentGroup === emptyGuid && currentUser.permissions.hasAdministrate) {
                let sharing = {
                    sharing: {
                        name: "Sharing",
                        callback: function (key, opt) {
                            let $element = opt.$trigger;
                            let id = $element.attr('id');

                            showSharingModal(id);
                        }
                    }
                }
                Object.assign(items, sharing);
            }
            
            if (currentUser.permissions.hasManage) {
                let move = {
                    move: {
                        name: "Move to folder",
                        callback: function (key, opt) {
                            let $element = opt.$trigger;
                            let id = $element.attr('id');

                            showMoveFileModal(id);
                        }
                    }
                }
                Object.assign(items, move);
            }
            
            if (currentUser.permissions.hasWrite) {
                let standardItems = {
                    rename: {
                        name: "Rename",
                        callback: function (key, opt) {
                            showRenameFileModal(key, opt);
                        }
                    },
                    delete: {
                        name: "Delete",
                        callback: function (key, opt) {
                            showDeleteFileModal(key, opt);
                        }
                    }
                }

                Object.assign(items, standardItems);
            }
            
            return {items: items};
        }
    });
    $.contextMenu({
        selector: '#file-container',
        items: {
            createFolder: {
                name: "Create folder",
                callback: function (key, opt) {
                    showCreateFolderModal();
                }
            },
            uploadFile: {
                name: "Upload file",
                callback: function (key, opt) {
                    showUploadFileModal();
                }
            }
        }
    });
    
    $("#file-container").on("dblclick", '.folder', function () {
        let id = this.id;
        browseDirectoryFiles(id);
    });

    $("#sidebar").mCustomScrollbar({
        theme: "minimal"
    });

    $('#dismiss, .overlay').on('click', function () {
        $('#sidebar').removeClass('active');
        $('.overlay').removeClass('active');
    });

    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').addClass('active');
        $('.overlay').addClass('active');
        $('.collapse.in').toggleClass('in');
        $('a[aria-expanded=true]').attr('aria-expanded', 'false');
    });
    
    loadUserGroups();
});

function showRenameFileModal(key, opt) {

    let $element = opt.$trigger;
    let fileId = $element.attr("id");

    let fileName = $element.find('.file-name').text();

    $("#file-name").val(fileName);
    $("#file-id").val(fileId);

    $("#renameFileModal").modal();

}

function renameFile() {
    let fileName = $("#file-name").val();
    let fileId = $("#file-id").val();

    let url = renameUrl;

    let file = {
        Id: fileId,
        Name: fileName
    }

    $.ajax({
        url: url,
        data: JSON.stringify(file),
        type: "PUT",
        contentType: "application/json",
        success: function (result) {
            let id = result.id;
            let fileName = result.name;

            $("#renameFileModal").modal("hide");

            let $fileNameElement = $('#' + id).find('.file-name');

            $fileNameElement.text(fileName);
        },
        error: function (result) {
            alert(result.responseText);
        }
    });

}

function showDeleteFileModal(key, opt) {

    let $element = opt.$trigger;
    let fileId = $element.attr("id");

    $("#delete-file-id").val(fileId);

    $("#deleteFileModal").modal();

}

function deleteFile() {

    let fileId = $("#delete-file-id").val();

    let url = deleteUrl;

    let data = {
        Id: fileId
    }

    $.ajax({
        url: url,
        data: JSON.stringify(data),
        type: "DELETE",
        contentType: "application/json",
        success: function (result) {

            $("#deleteFileModal").modal("hide");
            $('#' + fileId).remove();
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function browseDirectoryFiles(parentId) {
    if (parentId === 'shared') {
        browsingSharedFiles = true;
        currentGroup = emptyGuid;
    }

    let url = browseFilesUrl;
    
    if (browsingSharedFiles) {
        url = browseSharedFilesUrl;
    }
    
    let data = {
        parentId: parentId,
        groupId: currentGroup
    }

    $.ajax({
        url: url,
        type: "GET",
        data: data,
        success: function (result) {
            
            if (parentId in dirArray) {
                let found = false;
                Object.keys(dirArray).reverse()
                    .forEach(function(index) {
                        if (index !== parentId && !found) {
                            delete dirArray[index];
                        } else {
                            found = true;
                        }
                    });
            } else {
                if (parentId !== 'shared') {
                    let name = $("#"+parentId).find(".file-name").text();
                    dirArray[parentId] = name;
                }
            }

            currentDir = parentId;
            
            if (currentDir !== emptyGuid && currentDir !== 'shared') {
                $('#go-back-button').show();
            } else {
                $('#go-back-button').hide();
            }
            
            updateDirectoryPath();
            
            currentUser = result.user;
            
            if (currentUser.permissions.hasWrite) {
                $('#dropdownMenuButton').show();
            } else {
                $('#dropdownMenuButton').hide();
            }
            
            
            changeFiles(result.files);
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

const fileMarkup = `
                <div class="col-sm-3 col-xl-2 {{classes}} justify-content-center" id="{{fileId}}">
                    <div class="col-12 text-center">
                        <img src="{{icon}}" width="80%" />
                    </div>
                    <p class="file-name text-center">{{fileName}}</p>
                </div>
            `;

function changeFiles(result) {
    let $fileContainer = $('#file-container');

    $fileContainer.empty();

    result.forEach(file => {
        addFileToFileList(file);
    });
}

function showCreateFolderModal() {
    $("#createFolderModal").modal();

}

function createFolder() {
    let folderName = $("#create-folder-name").val();

    let url = createFolderUrl;

    let data = {
        Name: folderName,
        ParentId: currentDir,
        groupId: currentGroup,
    }

    $.ajax({
        url: url,
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json",
        success: function (result) {
            addFileToFileList(result);

            $("#createFolderModal").modal("hide");
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function showUploadFileModal() {
    clearProgressBar();

    $('#file-upload-form').trigger("reset");
    
    $("#upload-file-parentId").val(currentDir);
    $("#upload-file-groupId").val(currentGroup);

    $("#uploadFileModal").modal();
}

function clearProgressBar() {
    let $progressBar = $("#upload-progress-bar");

    $progressBar.attr("aria-valuenow", 0);
    $progressBar.attr("style", "width: " + 0 + "%");
    $progressBar.text(0 + " %");
}

function startFileUpload() {

    const xhr = new XMLHttpRequest();

    let $progressBar = $("#upload-progress-bar");

    xhr.open("POST", uploadFilesUrl);
    xhr.upload.addEventListener("progress", e => {
        const percent = e.lengthComputable ? (e.loaded / e.total) * 100 : 0;

        const intPercent = percent.toFixed(2);

        $progressBar.attr("aria-valuenow", intPercent);
        $progressBar.attr("style", "width: " + intPercent + "%");
        $progressBar.text(intPercent+" %");
    });

    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            if (xhr.status >= 200 && xhr.status <= 299) {
                let files = JSON.parse(xhr.responseText);

                files.forEach(file => {
                    addFileToFileList(file);
                })
                clearProgressBar();

                $("#uploadFileModal").modal("hide");
            } else {
                clearProgressBar();
            }
        }

    }

    let formData = new FormData(document.getElementById("file-upload-form"));

    xhr.send(formData);
}

function oneDirUp() {
    let keys = Object.keys(dirArray).reverse();
    
    if (keys.length >= 2) {
        browseDirectoryFiles(keys[1]);
    }
}

function updateDirectoryPath() {
    let dirPathElement = $("#directory-path");
    dirPathElement.empty();
    
    Object.keys(dirArray)
        .forEach(function(index) {
            let append = "<span data-id='" + index + "'>" + dirArray[index] + "</span>";
            dirPathElement.append(append);
            dirPathElement.append(" / ");
        });
}

function addFileToFileList(file) {
    let $fileContainer = $('#file-container');

    let markup = fileMarkup;

    markup = markup.replaceAll("{{fileId}}", file.id);
    markup = markup.replaceAll("{{fileName}}", file.name);

    if (file.isFolder) {
        markup = markup.replaceAll("{{classes}}", "file folder");
        markup = markup.replaceAll("{{icon}}", folderIconUrl);
    } else {
        if (file.name.endsWith('.txt')) {
            markup = markup.replaceAll("{{classes}}", "file txt-file");
        } else {
            markup = markup.replaceAll("{{classes}}", "file");
        }
        markup = markup.replaceAll("{{icon}}", fileIconUrl);
    }

    $fileContainer.append(markup);
}

function showMoveFileModal(fileId) {
    $("#move-file-id").val(fileId);
    
    let $list = $("#move-folder-list");
    
    $list.empty();

    let keys = Object.keys(dirArray);
    
    if (keys.length >= 2) {
        keys.forEach(parentId => {
            if (parentId !== currentDir) {
                let name = dirArray[parentId];
                
                // Check for file and parent ID, this ensures folder can't be moved into itself
                if (fileId !== parentId) {
                    let link = '<a href="#" ' +
                        'data-id="' + fileId + '" data-parent-id="' + parentId + '" ' +
                        'class="list-group-item list-group-item-action list-group-item-info move-file-folder-choice">' + name + '</a>';

                    $list.append(link);
                }
            }
        });
    }
    
    $("#file-container div").each(function () {
        let classes = this.className;
        
        if (classes.includes("folder")) {
            let name = $(this).find(".file-name").text();
            let parentId = this.id;
            
            let link = '<a href="#" ' +
                'data-id="' + fileId + '" data-parent-id="' + parentId + '" ' +
                'class="list-group-item list-group-item-action move-file-folder-choice">' + name + '</a>';
            
            $list.append(link);
        }
    });
    
    $("#moveFileModal").modal();
}

$(document).ready(function() {
    $('#move-folder-list').on('click', '.move-file-folder-choice', function (e) {
        e.preventDefault();
        let $this = $(this);

        let id = $this.data("id");
        let parentId = $this.data("parent-id");

        moveFile(id, parentId);
    });
    
    $('#directory-path').on('click', 'span', function () {
        let $this = $(this);
        let id = $this.data('id');
        
        if (currentDir !== id) {
            browseDirectoryFiles(id);
        }
    });
    
});

function moveFile(id, parentId) {
    let url = moveFileUrl;

    let data = {
        Id: id,
        ParentId: parentId,
    }

    $.ajax({
        url: url,
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json",
        success: function (result) {
            if (result === true) {
                $("#"+id).remove();

                $("#moveFileModal").modal("hide");
            } else {
                // TODO: Handle better in the future
                alert("Failed to move a file");
            }
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

$(document).ready(function () {
    $('#editModalSave').click(function () {
       editFileSave();
    });
});

function showEditFileModal(key, opt) 
{
    let $element = opt.$trigger;
    let fileId = $element.attr("id");
    let fileName = $element.find('.file-name').text();

    $.ajax({
        url: "/api/file/content/" + fileId,
        type: "GET",
        success: function (result) {
            $('#edit-file-name').text(fileName);

            $('#editFileId').val(fileId);
            $('#editFileContent').val(result.contents);
            $('#editFileTimestamp').val(result.timestamp);
            $('#editFileOverwrite').val(0);
            
            $('#editFileModal').modal();
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
    
}

function editFileSave() {
    let contents = $('#editFileContent').val();
    let id = $('#editFileId').val();
    let timestamp = $('#editFileTimestamp').val();
    let overwrite =$('#editFileOverwrite').val();

    let data = {
        id: id,
        contents: contents,
        timestamp: timestamp,
        overwrite: (overwrite === 'true'),
    }
    
    $.ajax({
        url: updateFileUrl,
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json",
        statusCode: {
            200: function (result) {

                alert("File updated!");
                $("#editFileModal").modal("hide");
            },
            400: function (result) {
                alert(result.responseText);
            },
            409: function (result) {
                $('#overwriteEditModal').modal();
                $('#overwriteEditModalOverwrite').click(function () {
                    $('#editFileOverwrite').val('true');
                    $('#overwriteEditModal').modal('hide');
                    editFileSave();
                });
            },
        }
    });
}

function initFileDownload(fileId, versionId = "") {
    let data = {versionId: versionId};
    $.ajax({
        url: "/api/file/download/" + fileId,
        data: data,
        success: function (result) {
            startFileDownload(result.file, result.downloadLink);
        },
        error: function (result) {
            alert(result.responseText);
        }
    })
}

function startFileDownload(file, link) {
    let $downloadLink = $('#downloadLink');
    $downloadLink.remove();
    
    let downloadLink = "<a href='" + link + "' download='"+ file.name +"' id='downloadLink'>Click here to download</a>"
    $('#downloadFileModal .modal-body').append(downloadLink);
    
    $('#downloadFileModal').modal();
}

function previewFile(fileId, fileName) {
    if (fileName.endsWith('.txt')) {
        // Separate preview
    }
    
    $('#filePreviewModal').modal();
    
    $modalBody = $('#filePreviewModal .modal-body');
    $('#preview-file-name').text(fileName);
    $modalBody.empty();
    
    $.ajax({
        url: "/api/file/download/" + fileId,
        success: function (result) {
            
            setTimeout(() => {
                let element = null;

                if (endsWithAny(['.png', '.jpg', '.jpeg'], fileName)) {
                    element = '<img src="' + result.downloadLink + '" class="img-fluid" />';
                }

                if (endsWithAny(['.mp4', '.avi', '.webm'], fileName)) {
                    element = '<video class="video-fluid" width="100%" controls>\n' +
                        '  <source src="' + result.downloadLink + '" type="video/mp4">\n' +
                        '</video>';
                }

                let mp3 = endsWithAny(['.mp3', '.waw'], fileName);

                if (mp3) {
                    element = '<audio controls>' +
                        '<source src="'+result.downloadLink+'">' +
                        '</audio>';
                }

                $modalBody.append(element);
            }, 900);
        },
        error: function (result) {
            alert(result.responseText);
        }
    })
}

$(document).ready(function () {
    $('#filePreviewModal').on('hide.bs.modal', function () {
        $(this).find('.modal-body').empty();
    })
})

function loadUserGroups() {
    $.ajax({
        url: listUserGroups,
        success: function (result) {
            result.forEach(group => {
                addGroupToSubmenu(group);
            })
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function addGroupToSubmenu(group) {
    let groupElement = '<li>\n' +
        '<a href="#" class="group-toggle justify-content-between overflow-auto" data-id="'+group.id+'">'+group.name+'' +
        '<button class="btn-primary group-setting" style="float: right; border-radius: 5px" href="#"><i class="fas fa-cog"></i></button>' +
        '</a>\n' +
        '</li>';
    $('#groupsSubmenu').append(groupElement);
}

$(document).ready(function () {
    $('#sidebar').on('click', '.group-toggle', function () {
        let id = $(this).data('id');

        if (id !== 'shared') {
            if (id === '0') {
                currentGroup = emptyGuid;
            } else {
                currentGroup = id;
            }
            
            browsingSharedFiles = false;
            browseDirectoryFiles(emptyGuid);
        } else {
            
            browsingSharedFiles = true;
            browseDirectoryFiles(id)
        }
        $('#sidebar').removeClass('active');
        $('.overlay').removeClass('active');
    });
    
    $('#sidebar').on('click', '.group-setting', function () {
        let id = $(this).parent().data('id');

        window.location.href = "/group/" + id;
    });
    
    $('#addGroupBtn').click(function () {
        showCreateGroupModal();
    })
});

function showCreateGroupModal() {
    $('#create-group-name').val('');
    $('#createGroupModal').modal();
}

function createGroup() {
    let name = $('#create-group-name').val();

    let data = {
        Name: name,
    };

    $.ajax({
        url: createGroupUrl,
        method: 'POST',
        data: JSON.stringify(data),
        contentType: "application/json",
        success: function (group) {
            window.location.href = "/group/"+group.id;
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function showSharedFiles() {
    currentDir = emptyGuid;
    currentGroup = emptyGuid;

    $.ajax({
        url: browseSharedFilesUrl,
        success: function (result) {

            Object.keys(dirArray).reverse()
                .forEach(function(index) {
                    if (index !== emptyGuid && !found) {
                        delete dirArray[index];
                    } else {
                        found = true;
                    }
                });

            $('#go-back-button').hide();

            updateDirectoryPath();

            changeFiles(result);
        },
        error: function (result) {
            alert(result.responseText);
        }
    })
}

function showSharingModal(fileId) {
    
    $('#file-share-id').val(fileId);
    
    $('#file-share-users').empty();
    
    $('#file-share-enable-link').hide();
    $('#file-share-disable-link').hide();
    
    $.ajax({
        url: '/api/file/share/'+fileId,
        success: function (result) {
            $('#file-share-link').val(result.link);
            
            if (result.link == null) {
                $('#file-share-enable-link').show();
                $('#file-share-disable-link').hide();
            } else {
                $('#file-share-enable-link').hide();
                $('#file-share-disable-link').show();
            }
            
            result.users.forEach(user => {
                addUserToSharedUsersList(user);
            });
        }
    });
    
    $('#fileSharingModal').modal();
}

function addUserToSharedUsersList(user) {
    let html = '<li class="list-group-item d-flex justify-content-between" data-id="'+user.id+'">' +
                    user.name +
        '          <button class="btn btn-danger user-remove">Remove</button>' +
        '       </li>';
    $('#file-share-users').append(html);
}

function showShareWithUserModal() {
    $('#shareWithUserModal').modal();
}

function shareWithUser() {
    let fileId = $('#file-share-id').val();
    let email = $('#file-share-user-email').val();
    
    let data = {
        FileId: fileId,
        Email: email
    }
    
    $.ajax({
        url: shareWithUserUrl,
        type: 'post',
        data: JSON.stringify(data),
        contentType: "application/json",
        success: function (result) {
            addUserToSharedUsersList(result);
            $('#shareWithUserModal').modal('hide');
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

$(document).ready(function () {
    $('#file-share-enable-link').on('click', function () {
        let fileId = $('#file-share-id').val();

        let data = {
            Id: fileId
        }

        $.ajax({
            url: enableLinkSharing,
            type: 'post',
            data: JSON.stringify(data),
            contentType: "application/json",
            success: function (result) {
                $('#file-share-link').val(result);
                $('#file-share-enable-link').hide();
                $('#file-share-disable-link').show();
            },
            error: function (result) {
                alert(result.responseText);
            }
        });
    });

    $('#file-share-disable-link').on('click', function () {
        let fileId = $('#file-share-id').val();

        let data = {
            Id: fileId
        }

        $.ajax({
            url: disableLinkSharing,
            type: 'post',
            data: JSON.stringify(data),
            contentType: "application/json",
            success: function (result) {
                $('#file-share-link').val('');
                $('#file-share-enable-link').show();
                $('#file-share-disable-link').hide();
            },
            error: function (result) {
                alert(result.responseText);
            }
        });
    });
    
    $('#file-share-users').on('click', '.user-remove', function () {
        let $userElement = $(this).parent();
        let userId = $userElement.data('id');
        let fileId = $('#file-share-id').val();

        let data = {
            FileId: fileId,
            UserId: userId,
        }
        
        $.ajax({
            url: disableSharingWithUser,
            type: 'delete',
            data: JSON.stringify(data),
            contentType: "application/json",
            success: function (result) {
                $userElement.remove();
            },
            error: function (result) {
                alert(result.responseText);
            }
        });
    });
});

// Commenting section

let sidebarFileId = null;

let connection = new signalR.HubConnectionBuilder().withUrl("/api/comments").build();

connection.start().catch(function (err) {
    return console.error(err.toString());
});

connection.on("NewComment", function (comment, fileId) {
    if (fileId === sidebarFileId) {
        addCommentToList(comment);
    }
});

$(document).ready(function () {
    
    $('#send-comment-button').on('click', function () {
        let $textField = $('#comment-text');
        let text = $textField.val();
        
        let data = {
            Text: text,
            FileId: sidebarFileId
        };

        $textField.val('');

        connection.invoke("NewComment", data);
    });

    $("#file-container").on("click", '.file:not(.folder)', function () {
        
        let id = this.id;
        let name = $(this).find('.file-name').text();
        sidebarFileId = id;
        
        $('#file-sidebar-name').text(name);
        $('.file-sidebar-element').hide();
    });
    
    $("#file-sidebar-buttons").on('click', 'button', function () {
        if (sidebarFileId === null) {
            alert("Please click on the file to choose it");
            return;
        }
        switch ($(this).data('id')) {
            case 'comments': {
                $('.file-sidebar-element').hide();
                $('#file-sidebar-comments').show();
                loadComments();
                break;
            }
            case 'versions': {
                $('.file-sidebar-element').hide();
                $('#file-sidebar-versions').show();
                loadVersions();
                break;
            }
        } 
    });
    
    $('#file-versions').on('click', '.file-version-revert', function () {
        let id = $(this).data('id');

        let data = {
            Id: id
        }
        
        $.ajax({
            url: revertVersionUrl,
            method: 'post',
            data: JSON.stringify(data),
            contentType: "application/json",
            success: function (result) {
                addVersionToList(result, true);
            },
            error: function (result) {
                alert(result.responseText);
            }
        });
    }).on('click', '.file-version-download', function () {
        let id = $(this).data('id');

        let fileId = sidebarFileId;

        initFileDownload(fileId, id);
    });
});

function loadComments() {
    $('#file-comments').empty();

    let data = {
        fileId: sidebarFileId,
        parentId: 0,
    }

    connectToSignalGroup(sidebarFileId);

    $.ajax({
        url: getCommentsUrl,
        data: data,
        success: function (result) {
            result.forEach(comment => {
                addCommentToList(comment);
            });
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function loadVersions() {
    $('#file-versions').empty();

    let data = {
        fileId: sidebarFileId
    }

    $.ajax({
        url: getVersionsUrl,
        data: data,
        success: function (result) {
            result.forEach(version => {
                addVersionToList(version);
            });
        },
        error: function (result) {
            alert(result.responseText);
        }
    });
}

function addCommentToList(comment) {
    let element = '<li class="list-group-item">' +
        '<b>' + comment.username + '</b><br>' +
        comment.text +
        '</li>';

    $('#file-comments').append(element);
}

function addVersionToList(version, toStart = false) {
    let date = new Date(version.created);
    let element = '<li class="list-group-item">' +
        '<b>' + date.toLocaleString() + '</b><br>' +
        version.note +
        '<br><span>by <b>' + version.username + '</b></span>' +
        '<div class="file-version-options">' +
        '<button class="btn btn-secondary file-version-revert" data-id="' + version.id + '">Revert</button>' +
        '<button class="btn btn-primary file-version-download" data-id="' + version.id + '">Download</button>' +
        '</div>' +
        '</li>';

    if (toStart) {
        $('#file-versions').prepend(element);
    } else {
        $('#file-versions').append(element);
    }
}

function connectToSignalGroup(fileId) {
    try {
        connection.invoke("AddToGroup", fileId).then(function () {
            //console.log('Successfully connected to group');
        });
    }
    catch (e) {
        console.error(e.toString());
    }
}