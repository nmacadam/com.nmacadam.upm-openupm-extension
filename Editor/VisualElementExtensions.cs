using UnityEngine.UIElements;

namespace PackageWizard.Editor
{
    internal static class VisualElementExtensions
    {
        public static VisualElement GetRoot(this VisualElement element)
        {
            // given element is null (how?)
            if (element == null) { return null; }

            // recursively look at parents
            while (element.parent != null)
            {
                element = element.parent;
            }

            // the parent element is null
            return element;
        }
    }
}
