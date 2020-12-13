using Group3.Semester3.DesktopClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Group3.Semester3.DesktopClient.Model
{
    public class RegisterWindowParams
    {
        public ApiService ApiService { get; set; }
    }

    //public class RegisterWindowModel
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    public bool NameRequiredPromptShown
    //    {
    //        get => nameRequiredPromptShownValue;

    //        set
    //        {
    //            if (value != nameRequiredPromptShownValue)
    //            {
    //                nameRequiredPromptShownValue = value;
    //                NotifyPropertyChanged();
    //            }
    //        }
    //    }

    //    public bool PasswordRequiredPromptShown
    //    {
    //        get => passwordRequiredPromptShownValue;

    //        set
    //        {
    //            if (value != passwordRequiredPromptShownValue)
    //            {
    //                passwordRequiredPromptShownValue = value;
    //                NotifyPropertyChanged();
    //            }
    //        }
    //    }

    //    public bool PasswordRepeatRequiredPromptShown
    //    {
    //        get => passwordRepeatRequiredPromptShownValue;

    //        set
    //        {
    //            if (value != passwordRepeatRequiredPromptShownValue)
    //            {
    //                passwordRepeatRequiredPromptShownValue = value;
    //                NotifyPropertyChanged();
    //            }
    //        }
    //    }


    //    public bool EmailRequiredPromptShown
    //    {
    //        get => emailRequiredPromptShownValue;

    //        set
    //        {
    //            if (value != emailRequiredPromptShownValue)
    //            {
    //                emailRequiredPromptShownValue = value;
    //                NotifyPropertyChanged();
    //            }
    //        }
    //    }

    //    private bool passwordRequiredPromptShownValue;
    //    private bool passwordRepeatRequiredPromptShownValue;
    //    private bool emailRequiredPromptShownValue;
    //    private bool nameRequiredPromptShownValue;
    //}

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
