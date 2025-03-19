using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyProject.Web.Models.Tasks
{
    internal class selectListItem : SelectListItem
    {
        public object ApplicationLanguageText { get; set; }
        public string Value { get; set; }
        public bool SelectedTaskState { get; set; }
    }
}