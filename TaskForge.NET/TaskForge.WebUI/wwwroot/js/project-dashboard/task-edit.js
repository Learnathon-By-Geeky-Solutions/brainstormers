
$(document).ready(function () {
    $('#taskDetailsModal').on('shown.bs.modal', function () {
        // Initialize Select2 for the Depends On field
        $('#todo-dependent-tasks').select2({
            placeholder: "Select a task to depend on",
            dropdownParent: $('#taskDetailsModal'),
            allowClear: true,
            width: '100%'
        });

        $('#inprogress-dependent-tasks').select2({
            placeholder: "Select a task to depend on",
            dropdownParent: $('#taskDetailsModal'),
            allowClear: true,
            width: '100%'
        });

        $('#done-dependent-tasks').select2({
            placeholder: "Select a task to depend on",
            dropdownParent: $('#taskDetailsModal'),
            allowClear: true,
            width: '100%'
        });

        $('#blocked-dependent-tasks').select2({
            placeholder: "Select a task to depend on",
            dropdownParent: $('#taskDetailsModal'),
            allowClear: true,
            width: '100%'
        });

        $('#task-members').select2({
            placeholder: "Select a member to the task",
            dropdownParent: $('#taskDetailsModal'),
            allowClear: true,
            width: '100%'
        });
    });

    $('.open-task-btn').on('click', function (event) {
        const taskId = $(this).data('task-id');

        if (!taskId) {
            console.error("No taskId found on button");
            return;
        }

        GetTaskDetails(taskId);
    });
});


// Get task and populate modal
function GetTaskDetails(taskId) {

    $.ajax({
        url: `/Task/GetTask/${taskId}`,
        method: 'GET',
        success: function (data) {
            //console.log("Fetched Task Data:", data);

            $('#task-id').val(data.id);
            $('#task-title').val(data.title);
            $('#task-description').val(data.description);
            $('#task-status').val(data.status);
            $('#task-priority').val(data.priority);
            if (data.startDate && data.startDate.includes('T')) {
                $('#task-startdate').val(data.startDate.split('T')[0]);
            } else {
                $('#task-startdate').val('');
            }
            if (data.dueDate && data.dueDate.includes('T')) {
                $('#task-duedate').val(data.dueDate.split('T')[0]);
            } else {
                $('#task-duedate').val('');
            }

            // Members
            $('#task-members').empty();
            data.allUsers.forEach(user => {
                const selected = data.assignedUserIds.includes(user.id) ? 'selected' : '';
                $('#task-members').append(`<option value="${user.id}" ${selected}>${user.name}</option>`);
            });

            // Dependencies
            $('#wrapper-todo-dependent-tasks').hide();
            $('#wrapper-inprogress-dependent-tasks').hide();
            $('#wrapper-done-dependent-tasks').hide();
            $('#wrapper-blocked-dependent-tasks').hide();

            if (data.status === 0) {
                $('#wrapper-todo-dependent-tasks').show();
            } else if (data.status === 1) {
                $('#wrapper-inprogress-dependent-tasks').show();
            } else if (data.status === 2) {
                $('#wrapper-done-dependent-tasks').show();
            } else if (data.status === 3) {
                $('#wrapper-blocked-dependent-tasks').show();
            }

            // Attachments
            $('#task-attachment-list').empty();
            data.attachments.forEach(att => {
                $('#task-attachment-list').append(`
                                    <li class="list-group-item d-flex justify-content-between align-items-center">
                                        <a href="${att.downloadUrl}" target="_blank">${att.fileName}</a>
                                        <button type="button" class="btn btn-sm btn-danger" onclick="RemoveAttachment(${att.id})">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </li>
                                `);
            });
        },
        error: function (xhr) {
            console.error("Failed to fetch task", xhr.responseText);
            alert("Failed to load task.");
        }
    });
}


function RemoveAttachment(attachmentId) {
    if (!confirm("Are you sure you want to delete this attachment?")) return;

    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `/Task/DeleteAttachment/${attachmentId}`,
        type: 'DELETE',
        headers: {
            'RequestVerificationToken': token
        },
        success: function () {
            // remove the list item from UI without full reload
            $(`#task-attachment-list button[onclick="RemoveAttachment(${attachmentId})"]`)
                .closest('li')
                .remove();
        },
        error: function (xhr) {
            console.error("Failed to delete attachment:", xhr.responseText);
            if (xhr.status === 403) {
                alert("Permission denied. You do not have rights to delete this attachment.");
            }
            else if (xhr.status === 404) {
                alert("Attachment not found. It may have been already deleted.");
            }
            else {
                alert("Failed to delete attachment: " + (xhr.responseJSON?.message || "Unknown error"));
            }
        }
    });
}


$('input[name="Attachments"]').on('change', function () {
    const files = this.files;
    let error = '';

    if (files.length > 10) {
        error = "You can't upload more than 10 files.";
    } else {
        for (let file of files) {
            if (file.size > 10 * 1024 * 1024) {
                error = `${file.name} is larger than 10MB.`;
                break;
            }
        }
    }

    if (error) {
        $(this).parent().find('.attachments-error').text(error).show();
        $(this).addClass('is-invalid');
        this.value = '';
    } else {
        $(this).parent().find('.attachments-error').text('').hide();
        $(this).removeClass('is-invalid');
    }
});


// Task Edit Form Submission
$('#taskEditForm').on('submit', function (e) {
    $(this).find('button[type="submit"]').prop('disabled', true);

    e.preventDefault();
    const formData = new FormData(this);

    const title = $('#task-title').val().trim();
    const description = $('#task-description').val().trim();
    const startDate = $('#task-startdate').val();
    const dueDate = $('#task-duedate').val();
    const attachments = $('#task-attachments')[0].files;

    // Reset previous errors
    $('.is-invalid').removeClass('is-invalid');

    let hasError = false;

    // Title length
    if (title.length === 0 || title.length > 200) {
        $('#task-title').addClass('is-invalid');
        hasError = true;
    }

    // Description length
    if (description.length > 1000) {
        $('#task-description').addClass('is-invalid');
        hasError = true;
    }

    // DueDate >= StartDate
    if (startDate && dueDate && new Date(dueDate) < new Date(startDate)) {
        $('#task-duedate').addClass('is-invalid');
        hasError = true;
    }

    if (hasError) {
        $(this).find('button[type="submit"]').prop('disabled', false);
        return;
    }

    //console.log(formData);
    $.ajax({
        url: '/Task/Update',
        type: 'PUT',
        data: formData,
        processData: false,
        contentType: false,
        success: (response) => {
            if (response.success) {
                $('#taskDetailsModal').modal('hide');
                location.reload();
            } else {
                alert("Failed to update task: " + response.message);
            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || "Something went wrong. Please try again.";
            alert("Request failed: " + message);
            console.error(xhr);
        }
    });
});