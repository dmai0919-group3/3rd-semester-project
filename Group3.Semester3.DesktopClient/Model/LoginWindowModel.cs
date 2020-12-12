using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Group3.Semester3.DesktopClient.Model
{
    class LoginWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool PasswordRequiredPromptShown
        {
            get
            {
                return passwordRequiredPromptShownValue;
            }

            set
            {
                if (value != passwordRequiredPromptShownValue)
                {
                    passwordRequiredPromptShownValue = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool EmailRequiredPromptShown
        {
            get
            {
                return this.emailRequiredPromptShownValue;
            }

            set
            {
                if (value != this.emailRequiredPromptShownValue)
                {
                    this.emailRequiredPromptShownValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool passwordRequiredPromptShownValue;
        private bool emailRequiredPromptShownValue;
    }
}
