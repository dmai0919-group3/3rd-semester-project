﻿let dirArray = {"00000000-0000-0000-0000-000000000000": "Home"};

let currentDir = "00000000-0000-0000-0000-000000000000";

$(function () {
    browseDirectoryFiles("00000000-0000-0000-0000-000000000000");

    $.contextMenu({
        selector: '.file',
        items: {
            move: {
                name: "Move to folder",
                callback: function (key, opt) {
                    let $element = opt.$trigger;
                    let id = $element.attr('id');
                    
                    showMoveFileModal(id);
                }
            },
            rename: {
                name: "Rename",
                callback: function (key, opt) {
                    showRenameFileModal(key, opt);
                }
            },
            delete: {
                name: "Delete",
                callback: function (key, opt) {
                    // TODO: Show confirm modal first in the future
                    showDeleteFileModal(key, opt);
                }
            }
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
    })
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
            // TODO: Handle better in the future
            alert("Failed to rename a file");
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
            // TODO: Handle better in the future
            alert("Failed to delete a file");
        }
    });
}

function browseDirectoryFiles(parentId) {

    let url = "/api/file/browse/" + parentId;

    $.ajax({
        url: url,
        type: "GET",
        success: function (result) {
            
            if (parentId in dirArray) {
                let found = false;
                Object.keys(dirArray).reverse()
                    .forEach(function(index) {
                        if (index !== parentId && !found) {
                            console.log(index);
                            delete dirArray[index];
                        } else {
                            found = true;
                        }
                    });
            } else {
                let name = $("#"+parentId).find(".file-name").text();
                dirArray[parentId] = name;
            }

            currentDir = parentId;
            
            updateDirectoryPath();
            
            changeFiles(result);
        },
        error: function (result) {
            // TODO: Handle better in the future
            alert("Failed to load files");
        }
    })
}

const fileMarkup = `
                <div class="col-md-2 {{classes}} justify-content-center" id="{{fileId}}">
                    <div>
                        <img src="{{icon}}" />
                    </div>
                    <p class="file-name">{{fileName}}</p>
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
    }

    $.ajax({
        url: url,
        data: JSON.stringify(data),
        type: "POST",
        contentType: "application/json",
        success: function (result) {
            addFileToFileList(result);
        },
        error: function (result) {
            // TODO: Handle better in the future
            alert("Failed to delete a file");
        }
    });
}

function showUploadFileModal() {
    clearProgressBar();

    $('#file-upload-form').trigger("reset");
    
    $("#upload-file-parentId").val(currentDir);

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
    
    Object.values(dirArray)
        .forEach(function(index) {
            dirPathElement.append(index + " / ");
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
        markup = markup.replaceAll("{{classes}}", "file");
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

                let link = '<a href="#" ' +
                    'data-id="' + fileId + '" data-parent-id="' + parentId + '" ' +
                    'class="list-group-item list-group-item-action list-group-item-info move-file-folder-choice">' + name + '</a>';

                $list.append(link);
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
});

function moveFile(id, parentId) {
    console.log(id, parentId);

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
            // TODO: Handle better in the future
            alert("Failed to move a file");
        }
    });
}