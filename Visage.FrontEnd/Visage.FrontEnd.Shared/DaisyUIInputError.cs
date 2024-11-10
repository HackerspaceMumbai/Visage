using Microsoft.AspNetCore.Components.Forms;


namespace Visage.FrontEnd.Shared
{
    internal class DaisyUIInputError: FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext,
       in FieldIdentifier fieldIdentifier)
        {
            var isValid = editContext.IsValid(fieldIdentifier);

            return isValid ? "" : "input-error";
        }
    }
}
