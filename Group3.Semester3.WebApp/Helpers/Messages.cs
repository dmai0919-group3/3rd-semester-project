using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Helpers
{
    public static class Messages
    {
        public const string SystemError = "System error, please contact Administrator";
        public const string OperationForbidden = "Operation Forbidden";
        public const string FailedToCreateComment = "Failed to create comment";
        public const string NoFiles = "No files chosen";
        public const string FileNotExistsDeleted = "File does not exist or not deleted";
        public const string FileNotExistsRenamed = "File does not exist or not renamed";
        public const string FileNotFound = "No file found";
        public const string FailedToCreateFolder = "Failed to create folder";
        public const string MoveFileIntoItself = "Can not move file into itself";
        public const string FileNotMoved = "Failed to move file, please try again";
        public const string FileDeletedByAnother = "File was deleted by another user";
        public const string FileChangedByAnother = "File was changed by another user. Please try again";
        public const string FailedToUpdateFile = "Failed to update a file";
        public const string FailedToRevertVersion = "Failed to revert file version";
        public const string FileIdEmpty = "File id can not be empty";
        public const string UserNotFoundByEmail = "User with this email not found";
        public const string ShareGroupFiles = "Cannot share group files";
        public const string FailedToCreateGroup = "Failed to create group";
        public const string GroupNotExistsDeleted = "Group does not exist or not deleted";
        public const string GroupNotExistsRenamed = "Group does not exist or not renamed";
        public const string GroupNotFound = "No group found";
        public const string UserAlreadyInGroup = "User is already part of the group";
        public const string FailedToAddUser = "Failed to add user";
        public const string UserNotFound = "User not found";
        public const string FailedToUpdatePermissions = "Failed to update users permissions";
        public const string UserNotInGroup = "User is not part of this group";
        public const string EmailEmpty = "Email or password cannot be empty";
        public const string IncorrectInitials = "Incorrect email or password";
        public const string PasswordIsRequired = "Password is required";
        public const string UserNotCreated = "User not created";
        public const string OldPasswordEmpty = "Old password cannot be empty";
        public const string WrongPassword = "Wrong password";
        public const string PasswordsNotMatching = "Passwords are not matching";
        public const string UserNotExistsDeleted = "User does not exist or not deleted";
        public const string UserNotExistsAltered = "User does not exist or not altered";
        public const string PasswordEmpty = "The password cannot be empty";
        public const string LoginSuccessful = "User logged in successfully";
        public const string RegistrationSuccessful = "User registered successfully";
        public const string UserUpdateSuccessful = "User updated successfully";
        
        public const string PasswordHashLenghtInvalid = "Invalid length of password hash (64 bytes expected)";
        public const string PasswordSaltLenghtInvalid = "Invalid length of password salt (128 bytes expected)";
    }
}
