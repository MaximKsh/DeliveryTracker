using System.Collections.Generic;

namespace backend.ViewModels.Errors
{
    /// <summary>
    /// ViewModel ошибки принятия приглашения.
    /// </summary>
    public class InvalidInvitationCodeViewModel: ErrorListViewModel
    {
        public InvalidInvitationCodeViewModel(): base()
        {
            this.Errors = new List<ErrorItemViewModel>
            {
                new ErrorItemViewModel
                {
                    Code = "Groups.AcceptInvitation",
                    Message = "Invalid invitation code",
                }
            };
        }
    }
}